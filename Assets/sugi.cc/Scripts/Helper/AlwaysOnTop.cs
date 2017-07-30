#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
using System.Runtime.InteropServices;

/// <summary>
/// This code is from Unity Forums [How to set windowed game to be "Always on top" in C#]
/// https://forum.unity3d.com/threads/solved-how-to-set-windowed-game-to-be-always-on-top-in-c.328040/
/// </summary>
public class AlwaysOnTop : MonoBehaviour
{
    #region WIN32API

    public static readonly System.IntPtr HWND_TOPMOST = new System.IntPtr(-1);
    public static readonly System.IntPtr HWND_NOT_TOPMOST = new System.IntPtr(-2);
    const System.UInt32 SWP_SHOWWINDOW = 0x0040;

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left, Top, Right, Bottom;

        public RECT(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public RECT(System.Drawing.Rectangle r)
          : this(r.Left, r.Top, r.Right, r.Bottom)
        {
        }

        public int X
        {
            get
            {
                return Left;
            }
            set
            {
                Right -= (Left - value);
                Left = value;
            }
        }

        public int Y
        {
            get
            {
                return Top;
            }
            set
            {
                Bottom -= (Top - value);
                Top = value;
            }
        }

        public int Height
        {
            get
            {
                return Bottom - Top;
            }
            set
            {
                Bottom = value + Top;
            }
        }

        public int Width
        {
            get
            {
                return Right - Left;
            }
            set
            {
                Right = value + Left;
            }
        }

        public static implicit operator System.Drawing.Rectangle(RECT r)
        {
            return new System.Drawing.Rectangle(r.Left, r.Top, r.Width, r.Height);
        }

        public static implicit operator RECT(System.Drawing.Rectangle r)
        {
            return new RECT(r);
        }
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern System.IntPtr FindWindow(String lpClassName, String lpWindowName);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetWindowRect(HandleRef hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowPos(System.IntPtr hWnd, System.IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

    #endregion


    // Use this for initialization
    void Start()
    {
        AssignTopmostWindow(Application.productName, true);
    }

    public bool AssignTopmostWindow(string WindowTitle, bool MakeTopmost)
    {
        UnityEngine.Debug.Log("Assigning top most flag to window of title: " + WindowTitle);

        System.IntPtr hWnd = FindWindow((string)null, WindowTitle);

        RECT rect = new RECT();
        GetWindowRect(new HandleRef(this, hWnd), out rect);

        return SetWindowPos(hWnd, MakeTopmost ? HWND_TOPMOST : HWND_NOT_TOPMOST, rect.X, rect.Y, rect.Width, rect.Height, SWP_SHOWWINDOW);
    }

    private string[] GetWindowTitles()
    {
        List<string> WindowList = new List<string>();

        Process[] ProcessArray = Process.GetProcesses();
        foreach (Process p in ProcessArray)
        {
            if (!IsNullOrWhitespace(p.MainWindowTitle))
            {
                WindowList.Add(p.MainWindowTitle);
            }
        }

        return WindowList.ToArray();
    }

    public bool IsNullOrWhitespace(string Str)
    {
        if (Str.Equals("null"))
        {
            return true;
        }
        foreach (char c in Str)
        {
            if (c != ' ')
            {
                return false;
            }
        }
        return true;
    }
}
#endif
