using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Automation;

namespace CharlieTheSlave;

/// <summary>
/// Launches an installer and drives its wizard by clicking the "forward" buttons
/// (Next, I Agree, Install, Finish…) until every process it started has exited.
/// </summary>
public sealed partial class InstallerAutomation
{
    // Order matters: the first match on a page wins.
    static readonly string[] ForwardButtons =
        { "next", "i agree", "agree", "accept", "install", "yes", "ok", "continue", "finish", "close" };

    static readonly TimeSpan Timeout = TimeSpan.FromMinutes(45);
    const int MaxSameClicks = 4;

    readonly IProgress<string> _log;
    readonly int _selfPid = Environment.ProcessId;
    readonly HashSet<int> _pids = new();
    readonly Dictionary<string, int> _clickCounts = new();
    readonly HashSet<string> _warned = new();
    readonly Dictionary<string, DateTime> _recentOptionChanges = new();
    readonly Dictionary<string, DateTime> _lastClickAt = new();
    readonly HashSet<string> _seenOnce = new();
    string _titleHint = "";

    /// <param name="components">
    /// Components to leave ticked on an NSIS "Choose Components" page (everything else is unticked),
    /// or null to keep the installer's defaults.
    /// </param>
    public InstallerAutomation(IProgress<string> log, IReadOnlyList<string>? components = null)
    {
        _log = log;
        _components = components;
    }

    /// <returns>The installer's exit code, or null if it couldn't be read.</returns>
    public async Task<int?> RunAsync(string installerPath, CancellationToken ct)
    {
        _titleHint = FileVersionInfo.GetVersionInfo(installerPath).ProductName is { Length: > 0 } p ? p : "TrueTest";

        using var root = Process.Start(new ProcessStartInfo(installerPath)
        {
            UseShellExecute = true,
            WorkingDirectory = Path.GetDirectoryName(installerPath)!,
        }) ?? throw new InvalidOperationException("The installer did not start.");

        _pids.Add(root.Id);
        _log.Report($"Started installer (PID {root.Id}). Waiting for its window…");

        var deadline = DateTime.UtcNow + Timeout;
        int quietPolls = 0;
        while (true)
        {
            ct.ThrowIfCancellationRequested();
            if (DateTime.UtcNow > deadline)
                throw new TimeoutException($"The installer is still running after {Timeout.TotalMinutes:0} minutes. Please check it.");

            int alive = RefreshProcessTree();
            var windows = FindInstallerWindows();

            bool clicked = false;
            foreach (var w in windows)
            {
                if (TryAdvance(w)) { clicked = true; break; }
            }

            // Done once nothing from the installer is running or showing for a couple of seconds.
            quietPolls = alive == 0 && windows.Count == 0 ? quietPolls + 1 : 0;
            if (quietPolls >= 4) break;

            await Task.Delay(clicked ? 1200 : 500, ct);
        }

        try { return root.ExitCode; }
        catch { return null; }
    }

    /// <summary>Clicks the best forward button in one window. Returns true if it clicked something.</summary>
    bool TryAdvance(AutomationElement window)
    {
        try
        {
            // Main window disabled = a message box is open over it; that box is handled as its own window.
            if (!IsTopLevelEnabled(window)) return false;

            string title = window.Current.Name;
            string pageText = string.Join(" | ", window
                .FindAll(TreeScope.Descendants, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Text))
                .Cast<AutomationElement>().Select(e => e.Current.Name));

            // Change options first and click on a later poll, once the change has landed.
            if (AcceptLicenseAndPostponeReboot(window)) return true;
            switch (AdjustComponents(window))
            {
                case PageAction.Waiting: return true;
                case PageAction.Blocked: return false; // user has been asked to take over this page
            }

            var buttons = window
                .FindAll(TreeScope.Descendants, new AndCondition(
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Button),
                    new PropertyCondition(AutomationElement.IsEnabledProperty, true)))
                .Cast<AutomationElement>()
                .Where(b => !b.Current.IsOffscreen && !IsTitleBarButton(b) && IsTopLevelEnabled(b))
                .ToList();

            // Never answer "Restart Windows now?" style questions on the user's behalf.
            bool isYesNo = buttons.Any(b => Normalize(b.Current.Name) == "no");
            if (isYesNo && ContainsAny(pageText, "reboot", "restart"))
            {
                WarnOnce($"reboot:{window.Current.NativeWindowHandle}",
                    $"\"{title}\" is asking about restarting the computer. Please answer that one yourself.");
                return false;
            }

            foreach (var wanted in ForwardButtons)
            {
                var button = buttons.FirstOrDefault(b => Matches(Normalize(b.Current.Name), wanted));
                if (button == null) continue;

                string name = button.Current.Name.Replace("&", "");
                string key = $"{window.Current.NativeWindowHandle}|{name}|{pageText.GetHashCode()}";

                // Only act on a page that looks the same on two polls in a row, so we never click while it is
                // half-drawn (e.g. "Finish" visible before the "reboot later" option we need to select first).
                if (_seenOnce.Add(key)) return false;

                // Same button on the same page clicked moments ago: the page is just slow to change, not stuck.
                if (_lastClickAt.TryGetValue(key, out var last) && DateTime.UtcNow - last < TimeSpan.FromSeconds(5))
                    return false;
                _lastClickAt[key] = DateTime.UtcNow;

                int count = _clickCounts.GetValueOrDefault(key) + 1;
                if (count > MaxSameClicks)
                {
                    WarnOnce(key, $"Looks stuck on \"{title}\": \"{name}\" isn't moving forward. Please take over this page.");
                    return false;
                }
                _clickCounts[key] = count;

                _log.Report($"{TopLevelTitle(button) ?? title}: clicking \"{name}\"");
                Click(button);
                return true;
            }
        }
        catch (ElementNotAvailableException) { /* page changed mid-read; try again next poll */ }
        catch (COMException) { }
        return false;
    }

    /// <summary>
    /// Ticks "I accept" options, and picks "reboot later" on NSIS finish pages.
    /// Returns true if it changed something (the caller should wait before clicking on).
    /// </summary>
    bool AcceptLicenseAndPostponeReboot(AutomationElement window)
    {
        var options = window.FindAll(TreeScope.Descendants, new OrCondition(
                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.CheckBox),
                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.RadioButton)))
            .Cast<AutomationElement>();

        bool changed = false;
        foreach (var opt in options)
        {
            var c = opt.Current;
            var n = Normalize(c.Name);
            bool want = (n.Contains("accept") && !n.Contains("not")) || n.Contains("reboot later") || n.Contains("restart later");
            if (!want || !c.IsEnabled || c.IsOffscreen || !IsTopLevelEnabled(opt)) continue;

            bool alreadyOn =
                opt.TryGetCurrentPattern(TogglePattern.Pattern, out var tp) ? ((TogglePattern)tp).Current.ToggleState == ToggleState.On :
                opt.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var sp) && ((SelectionItemPattern)sp).Current.IsSelected;
            if (alreadyOn) continue;

            changed = true;
            // Our previous click hasn't landed yet: wait rather than flip it back off.
            string key = $"{c.NativeWindowHandle}|{c.Name}";
            if (_recentOptionChanges.TryGetValue(key, out var at) && DateTime.UtcNow - at < TimeSpan.FromSeconds(5))
                continue;

            Click(opt);
            _recentOptionChanges[key] = DateTime.UtcNow;
            _log.Report($"Selected \"{c.Name.Replace("&", "")}\"");
        }
        return changed;
    }

    /// <summary>
    /// Clicks without waiting. UIA's Invoke() blocks until the click handler returns (forever, if it opens a
    /// message box) and also stalls every other UIA call into that process, so for real Win32 controls
    /// (NSIS buttons, check boxes, radio buttons) we post BM_CLICK, like AutoHotkey's ControlClick.
    /// </summary>
    static void Click(AutomationElement element)
    {
        var hwnd = new IntPtr(element.Current.NativeWindowHandle);
        if (hwnd != IntPtr.Zero)
        {
            PostMessage(hwnd, BM_CLICK, IntPtr.Zero, IntPtr.Zero);
            return;
        }

        // Not a native control: fall back to UIA on a throwaway thread so a blocked click can't freeze us.
        Action? act =
            element.TryGetCurrentPattern(InvokePattern.Pattern, out var ip) ? ((InvokePattern)ip).Invoke :
            element.TryGetCurrentPattern(TogglePattern.Pattern, out var tp) ? ((TogglePattern)tp).Toggle :
            element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var sp) ? ((SelectionItemPattern)sp).Select :
            null;
        if (act != null)
            _ = Task.Run(() => { try { act(); } catch { /* element went away; next poll retries */ } });
    }

    /// <summary>False while the control's top-level window is disabled, e.g. a message box is open over it.</summary>
    static bool IsTopLevelEnabled(AutomationElement element)
    {
        var hwnd = new IntPtr(element.Current.NativeWindowHandle);
        if (hwnd == IntPtr.Zero) return true;
        var root = GetAncestor(hwnd, GA_ROOT);
        return root == IntPtr.Zero || IsWindowEnabled(root);
    }

    static string? TopLevelTitle(AutomationElement element)
    {
        var hwnd = new IntPtr(element.Current.NativeWindowHandle);
        if (hwnd == IntPtr.Zero) return null;
        var sb = new System.Text.StringBuilder(256);
        return GetWindowText(GetAncestor(hwnd, GA_ROOT), sb, sb.Capacity) > 0 ? sb.ToString() : null;
    }

    /// <summary>
    /// Uses EnumWindows rather than the UIA tree: UIA nests owned dialogs (like "Uninstall the previous
    /// version?") under their owner, so they never show up as top-level windows there.
    /// </summary>
    List<AutomationElement> FindInstallerWindows()
    {
        var handles = new List<IntPtr>();
        EnumWindows((hwnd, _) =>
        {
            if (!IsWindowVisible(hwnd)) return true;
            GetWindowThreadProcessId(hwnd, out uint pid);
            if (pid == _selfPid) return true;

            bool ours = _pids.Contains((int)pid);
            // NSIS uninstallers re-launch themselves from %TEMP%, so also match the product's dialogs by title.
            bool sameProductDialog = false;
            if (!ours)
            {
                var cls = new System.Text.StringBuilder(64);
                var text = new System.Text.StringBuilder(256);
                GetClassName(hwnd, cls, cls.Capacity);
                GetWindowText(hwnd, text, text.Capacity);
                sameProductDialog = cls.ToString() == "#32770" &&
                                    text.ToString().Contains(_titleHint, StringComparison.OrdinalIgnoreCase);
            }
            if (ours || sameProductDialog) handles.Add(hwnd);
            return true;
        }, IntPtr.Zero);

        var result = new List<AutomationElement>();
        foreach (var h in handles)
        {
            try { result.Add(AutomationElement.FromHandle(h)); }
            catch (ElementNotAvailableException) { }
            catch (ArgumentException) { }
        }
        return result;
    }

    /// <summary>Adds child processes of the installer to the tracked set. Returns how many are still alive.</summary>
    int RefreshProcessTree()
    {
        var all = SnapshotProcesses();
        var alive = all.Select(p => p.Pid).ToHashSet();
        _pids.RemoveWhere(pid => !alive.Contains(pid));

        bool added;
        do
        {
            added = false;
            foreach (var (pid, parent) in all)
                if (_pids.Contains(parent) && _pids.Add(pid)) added = true;
        } while (added);

        return _pids.Count;
    }

    void WarnOnce(string key, string message)
    {
        if (_warned.Add(key)) _log.Report("⚠ " + message);
    }

    static bool IsTitleBarButton(AutomationElement button)
    {
        var parent = TreeWalker.ControlViewWalker.GetParent(button);
        return parent != null && parent.Current.ControlType == ControlType.TitleBar;
    }

    static string Normalize(string s) =>
        s.Replace("&", "").Replace(">", "").Replace("<", "").Trim().ToLowerInvariant();

    static bool Matches(string name, string wanted) => name == wanted || name.StartsWith(wanted + " ");

    static bool ContainsAny(string text, params string[] words) =>
        words.Any(w => text.Contains(w, StringComparison.OrdinalIgnoreCase));

    #region Process snapshot (Toolhelp32)

    static List<(int Pid, int ParentPid)> SnapshotProcesses()
    {
        var list = new List<(int, int)>();
        IntPtr snap = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
        if (snap == INVALID_HANDLE_VALUE) return list;
        try
        {
            var entry = new PROCESSENTRY32 { dwSize = (uint)Marshal.SizeOf<PROCESSENTRY32>() };
            if (!Process32First(snap, ref entry)) return list;
            do list.Add(((int)entry.th32ProcessID, (int)entry.th32ParentProcessID));
            while (Process32Next(snap, ref entry));
        }
        finally { CloseHandle(snap); }
        return list;
    }

    const uint TH32CS_SNAPPROCESS = 0x2;
    static readonly IntPtr INVALID_HANDLE_VALUE = new(-1);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct PROCESSENTRY32
    {
        public uint dwSize;
        public uint cntUsage;
        public uint th32ProcessID;
        public IntPtr th32DefaultHeapID;
        public uint th32ModuleID;
        public uint cntThreads;
        public uint th32ParentProcessID;
        public int pcPriClassBase;
        public uint dwFlags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] public string szExeFile;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr CreateToolhelp32Snapshot(uint flags, uint processId);

    [DllImport("kernel32.dll", EntryPoint = "Process32FirstW", CharSet = CharSet.Unicode, SetLastError = true)]
    static extern bool Process32First(IntPtr snapshot, ref PROCESSENTRY32 entry);

    [DllImport("kernel32.dll", EntryPoint = "Process32NextW", CharSet = CharSet.Unicode, SetLastError = true)]
    static extern bool Process32Next(IntPtr snapshot, ref PROCESSENTRY32 entry);

    [DllImport("kernel32.dll")]
    static extern bool CloseHandle(IntPtr handle);

    const uint BM_CLICK = 0x00F5;
    const uint GA_ROOT = 2;

    [DllImport("user32.dll")]
    static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    static extern IntPtr GetAncestor(IntPtr hWnd, uint flags);

    [DllImport("user32.dll")]
    static extern bool IsWindowEnabled(IntPtr hWnd);

    delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll")]
    static extern bool EnumWindows(EnumWindowsProc callback, IntPtr lParam);

    [DllImport("user32.dll")]
    static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    static extern int GetClassName(IntPtr hWnd, System.Text.StringBuilder className, int maxCount);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder text, int maxCount);

    #endregion
}
