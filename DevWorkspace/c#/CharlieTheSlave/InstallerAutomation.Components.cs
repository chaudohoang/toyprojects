using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Automation;

namespace CharlieTheSlave;

/// <summary>
/// NSIS "Choose Components" page: a Win32 tree view whose check boxes are custom state images, so UI Automation
/// can't see whether they're ticked. We read state images with TVM_GETITEMSTATE and toggle with the Space key,
/// which is exactly how NSIS expects keyboard users to do it.
/// </summary>
public sealed partial class InstallerAutomation
{
    enum PageAction { None, Waiting, Blocked }

    const int MaxTogglesPerItem = 2;

    readonly IReadOnlyList<string>? _components;
    readonly Dictionary<string, int> _toggleCounts = new();

    PageAction AdjustComponents(AutomationElement window)
    {
        if (_components == null) return PageAction.None;

        var tree = window.FindFirst(TreeScope.Descendants,
            new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Tree));
        if (tree == null) return PageAction.None;
        var treeHwnd = new IntPtr(tree.Current.NativeWindowHandle);
        if (treeHwnd == IntPtr.Zero) return PageAction.None;

        // UIA gives us the names, the tree view gives us handles and check state; both are depth-first.
        var names = tree.FindAll(TreeScope.Descendants,
                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.TreeItem))
            .Cast<AutomationElement>().Select(e => e.Current.Name.Trim()).ToList();
        var items = TreeItemsDepthFirst(treeHwnd);
        if (names.Count != items.Count || names.Count == 0)
        {
            WarnOnce("components:unreadable",
                "I can't read the component list reliably. Please tick the components yourself, then click Next.");
            return PageAction.Blocked;
        }

        var missing = _components.Where(w => !names.Any(n => WordsMatch(w, n))).ToList();
        if (missing.Count > 0)
        {
            WarnOnce("components:missing",
                $"I couldn't find component(s): {string.Join(", ", missing)}. " +
                $"This installer has: {string.Join(", ", names)}. Please pick them yourself, then click Next.");
            return PageAction.Blocked;
        }

        var ancestors = new List<string>();
        for (int i = 0; i < items.Count; i++)
        {
            var (item, depth, hasChildren) = items[i];
            if (ancestors.Count > depth) ancestors.RemoveRange(depth, ancestors.Count - depth);
            string name = names[i];
            ancestors.Add(name);

            // A group's box just reflects its children, so set the children and leave the group alone.
            if (hasChildren) continue;

            int image = StateImage(treeHwnd, item);
            if (image == 0) continue; // no check box (e.g. a label row)
            bool isChecked = image is 2 or 5;
            bool readOnly = image >= 4;
            bool want = ancestors.Any(a => _components.Any(w => WordsMatch(w, a)));
            if (isChecked == want) continue;

            if (readOnly)
            {
                WarnOnce($"components:ro:{name}", $"\"{name}\" is locked by the installer, so I'm leaving it as it is.");
                continue;
            }

            string key = $"{treeHwnd}|{name}";
            if (_recentOptionChanges.TryGetValue(key, out var at) && DateTime.UtcNow - at < TimeSpan.FromSeconds(3))
                return PageAction.Waiting; // previous toggle still landing

            int count = (_toggleCounts.TryGetValue(key, out var n) ? n : 0) + 1;
            if (count > MaxTogglesPerItem)
            {
                WarnOnce($"components:stuck:{name}",
                    $"\"{name}\" won't stay {(want ? "ticked" : "unticked")}. Please set the components yourself, then click Next.");
                return PageAction.Blocked;
            }
            _toggleCounts[key] = count;
            _recentOptionChanges[key] = DateTime.UtcNow;

            TreeMessage(treeHwnd, TVM_SELECTITEM, new IntPtr(TVGN_CARET), item);
            PostMessage(treeHwnd, WM_KEYDOWN, new IntPtr(VK_SPACE), new IntPtr(0x00390001));
            PostMessage(treeHwnd, WM_KEYUP, new IntPtr(VK_SPACE), unchecked((IntPtr)(int)0xC0390001));
            _log.Report($"{(want ? "Ticked" : "Unticked")} \"{name}\"");
            return PageAction.Waiting; // one at a time; re-read the tree on the next poll
        }
        return PageAction.None;
    }

    /// <summary>True if every word of <paramref name="wanted"/> appears in <paramref name="actual"/>.</summary>
    static bool WordsMatch(string wanted, string actual)
    {
        static string[] Words(string s) =>
            Regex.Split(s.ToLowerInvariant(), @"[^a-z0-9]+").Where(w => w.Length > 0).ToArray();
        var have = Words(actual);
        var need = Words(wanted);
        return need.Length > 0 && need.All(have.Contains);
    }

    static List<(IntPtr Item, int Depth, bool HasChildren)> TreeItemsDepthFirst(IntPtr tree)
    {
        var list = new List<(IntPtr, int, bool)>();
        void Walk(IntPtr item, int depth)
        {
            while (item != IntPtr.Zero && list.Count < 500)
            {
                var child = TreeMessage(tree, TVM_GETNEXTITEM, new IntPtr(TVGN_CHILD), item);
                list.Add((item, depth, child != IntPtr.Zero));
                if (child != IntPtr.Zero) Walk(child, depth + 1);
                item = TreeMessage(tree, TVM_GETNEXTITEM, new IntPtr(TVGN_NEXT), item);
            }
        }
        Walk(TreeMessage(tree, TVM_GETNEXTITEM, new IntPtr(TVGN_ROOT), IntPtr.Zero), 0);
        return list;
    }

    /// <summary>
    /// State image index: 1 unticked, 2 ticked, 3 partly ticked; NSIS adds 4-6 for the read-only versions.
    /// </summary>
    static int StateImage(IntPtr tree, IntPtr item) =>
        (int)(((long)TreeMessage(tree, TVM_GETITEMSTATE, item, new IntPtr(TVIS_STATEIMAGEMASK)) & TVIS_STATEIMAGEMASK) >> 12);

    static IntPtr TreeMessage(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam) =>
        SendMessageTimeout(hwnd, msg, wParam, lParam, SMTO_ABORTIFHUNG, 2000, out var result) == IntPtr.Zero
            ? IntPtr.Zero
            : result;

    const uint TVM_GETNEXTITEM = 0x110A;
    const uint TVM_SELECTITEM = 0x110B;
    const uint TVM_GETITEMSTATE = 0x1127;
    const int TVGN_ROOT = 0x0, TVGN_NEXT = 0x1, TVGN_CHILD = 0x4, TVGN_CARET = 0x9;
    const int TVIS_STATEIMAGEMASK = 0xF000;
    const uint WM_KEYDOWN = 0x0100, WM_KEYUP = 0x0101;
    const int VK_SPACE = 0x20;
    const uint SMTO_ABORTIFHUNG = 0x2;

    [DllImport("user32.dll")]
    static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam,
        uint flags, uint timeoutMs, out IntPtr result);
}
