SetTitleMatchMode, 2
IfWinExist, Installer Language
{
    WinActivate ; Use the window found by IfWinExist.

    SendInput, {Enter}
    return
}
IfWinExist, TrueTest Setup
{
    WinActivate ; Use the window found by IfWinExist.

    SendInput, {Enter}
    return
}