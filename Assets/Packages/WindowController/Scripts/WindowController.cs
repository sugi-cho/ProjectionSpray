using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Timers;


namespace WindowController {
    using HWND = System.IntPtr;

    public static class WindowController {
        private static Timer timer = null;
        private static int timerRepeatCount = 0;
        private static Vector2 windowPos = Vector2.zero;

        private const int SWP_NOSIZE = 0x0001;
        private const int SWP_NOMOVE = 0x0002;
        private const int HWND_TOPMOST = -1;
        private const int HWND_NOTOPMOST = -2;


        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool MoveWindow(HWND hWnd, int x, int y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll")]
        private static extern HWND GetActiveWindow();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(HWND hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        public static void MoveWindow(int x, int y) {
            HWND hWnd = GetCurrentWindow();
            RECT rc;

            GetWindowRect(hWnd, out rc);
            MoveWindow(hWnd, x, y, rc.Right - rc.Left, rc.Bottom - rc.Top, false);
        }

        public static void TryMoveWindow(int x, int y, float interval, int repeatCount = -1) {
            if (timer == null) {
                timer = new Timer();
                timer.AutoReset = true;
                timer.Elapsed += OnTimerElapsed;
            }

            windowPos.Set(x, y);
            timerRepeatCount = repeatCount < 0 ? -1 : repeatCount; // -1:forever
            timer.Interval = interval;
            timer.Enabled = true;
        }

        static void OnTimerElapsed(object sender, System.Timers.ElapsedEventArgs e) {
            int x = (int)windowPos.x, y = (int)windowPos.y;
            MoveWindow(x, y);

            RECT rc;
            GetWindowRect(GetCurrentWindow(), out rc);
            if (rc.Left == x && rc.Top == y) {
                timer.Enabled = false;
                return;
            }

            if (timerRepeatCount != -1 && --timerRepeatCount < 0) {
                timer.Enabled = false;
                return;
            }
        }

        public static int GetProcessId() {
            return System.Diagnostics.Process.GetCurrentProcess().Id;
        }

        public static Dictionary<HWND, string> GetWindows() {
            HWND shellWindow = GetShellWindow();
            var windows = new Dictionary<HWND, string>();

            EnumWindows((hWnd, lParam) => {
                var shell = hWnd == shellWindow;
                var visible = IsWindowVisible(hWnd);
                var length = GetWindowTextLength(hWnd);
                var builder = new StringBuilder(length);

                GetWindowText(hWnd, builder, length + 1);
                if (shell || !visible || length == 0) {
                    return true;
                }

                windows[hWnd] = builder.ToString();
                return true;
            }, 0);

            return windows;
        }

        public static HWND GetWindow(int processId) {
            var pair = GetWindows().Select(x => {
                long pid = 0;
                GetWindowThreadProcessId(x.Key, out pid);

                return new KeyValuePair<HWND, long>(x.Key, pid);
            }).FirstOrDefault(x => x.Value == processId);

            return pair.Key;
        }

        public static HWND GetCurrentWindow() {
            return GetWindow(GetProcessId());
        }

        public static bool SetToTopMost(HWND hWnd) {
            return SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
        }

        public static bool SetToTopMost() {
            return SetToTopMost(GetCurrentWindow());
        }

        delegate bool EnumWindowsProc(HWND hWnd, int lParam);

        [DllImport("user32.dll")]
        static extern bool EnumWindows(EnumWindowsProc enumFunc, int lParam);

        [DllImport("user32.dll")]
        static extern int GetWindowText(HWND hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        static extern int GetWindowTextLength(HWND hWnd);

        [DllImport("user32.dll")]
        static extern bool IsWindowVisible(HWND hWnd);

        [DllImport("user32.dll")]
        static extern HWND GetShellWindow();

        [DllImport("user32.dll")]
        static extern long GetWindowThreadProcessId(HWND hWnd, out long lpdwProcessId);
        
        [DllImport("user32.dll")]
        static extern bool SetWindowPos(HWND hWnd, int hWndInsertAfter, int x, int y, int cx, int cy, int uFlags);
    }
}