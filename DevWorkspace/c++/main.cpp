#include <windows.h>

LRESULT CALLBACK WndProc(HWND hwnd, UINT msg, WPARAM wp, LPARAM lp) {
    static HWND hLbl, hBtn;
    switch (msg) {
        case WM_CREATE:
            hLbl = CreateWindow("STATIC", "", WS_CHILD | WS_VISIBLE,
                20, 20, 260, 24, hwnd, nullptr, nullptr, nullptr);
            hBtn = CreateWindow("BUTTON", "Say Hello", WS_CHILD | WS_VISIBLE,
                20, 60, 120, 30, hwnd, (HMENU)1, nullptr, nullptr);
            break;
        case WM_COMMAND:
            if (LOWORD(wp) == 1)
                SetWindowText(hLbl, "Hello, World!");
            break;
        case WM_DESTROY:
            PostQuitMessage(0);
            break;
    }
    return DefWindowProc(hwnd, msg, wp, lp);
}

int WINAPI WinMain(HINSTANCE hInst, HINSTANCE, LPSTR, int) {
    WNDCLASS wc = { 0 };
    wc.lpfnWndProc   = WndProc;
    wc.hInstance     = hInst;
    wc.lpszClassName = "HelloWin32";
    wc.hbrBackground = (HBRUSH)(COLOR_WINDOW + 1);
    RegisterClass(&wc);

    HWND hwnd = CreateWindow("HelloWin32", "Hello WinForms - C++ Win32",
        WS_OVERLAPPEDWINDOW, 100, 100, 320, 180, nullptr, nullptr, hInst, nullptr);
    ShowWindow(hwnd, SW_SHOW);

    MSG msg;
    while (GetMessage(&msg, nullptr, 0, 0)) {
        TranslateMessage(&msg);
        DispatchMessage(&msg);
    }
    return 0;
}