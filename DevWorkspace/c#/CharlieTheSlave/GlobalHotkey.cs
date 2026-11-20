using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace CharlieTheSlave;

/// <summary>
/// A system-wide hotkey. The POI export keeps TrueTest in front and moves the mouse, so the assistant's
/// own window can't be typed into while it runs; this is how the user gets a word in to stop it.
/// </summary>
public sealed class GlobalHotkey : IDisposable
{
    const int WM_HOTKEY = 0x0312;
    const uint MOD_ALT = 0x0001, MOD_CONTROL = 0x0002, MOD_NOREPEAT = 0x4000;

    readonly int _id;
    readonly IntPtr _hwnd;
    readonly HwndSource _source;
    readonly Action _pressed;
    bool _registered;

    GlobalHotkey(HwndSource source, int id, uint modifiers, uint key, Action pressed)
    {
        _source = source;
        _hwnd = source.Handle;
        _id = id;
        _pressed = pressed;
        _registered = RegisterHotKey(_hwnd, _id, modifiers | MOD_NOREPEAT, key);
        _source.AddHook(Hook);
    }

    /// <summary>True when Windows actually gave us the key (another program may already own it).</summary>
    public bool IsRegistered => _registered;

    /// <summary>Registers Ctrl+Alt+<paramref name="key"/> for <paramref name="window"/>, which must be shown already.</summary>
    public static GlobalHotkey? CtrlAlt(Window window, System.Windows.Input.Key key, Action pressed)
    {
        if (PresentationSource.FromVisual(window) is not HwndSource source) return null;
        uint vk = (uint)System.Windows.Input.KeyInterop.VirtualKeyFromKey(key);
        return new GlobalHotkey(source, id: (int)vk, MOD_CONTROL | MOD_ALT, vk, pressed);
    }

    IntPtr Hook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg != WM_HOTKEY || wParam.ToInt32() != _id) return IntPtr.Zero;
        handled = true;
        _pressed();
        return IntPtr.Zero;
    }

    public void Dispose()
    {
        if (_registered)
        {
            UnregisterHotKey(_hwnd, _id);
            _registered = false;
        }
        _source.RemoveHook(Hook);
    }

    [DllImport("user32.dll")] static extern bool RegisterHotKey(IntPtr hwnd, int id, uint modifiers, uint key);
    [DllImport("user32.dll")] static extern bool UnregisterHotKey(IntPtr hwnd, int id);
}
