/*
 * Author is https://github.com/a3geek
 * this script is useful!
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
using System.Diagnostics;

using Debug = UnityEngine.Debug;


namespace FileUtility
{
    public static class OpenInFileBrowser
    {
        public static bool Open(string path)
        {
            if (IsWindows())
            {
                return Open4Windows(path);
            }
            else if (IsMac())
            {
                return Open4Mac(path);
            }

            return false;
        }

        public static bool Open4Windows(string path)
        {
            if (!IsWindows()) { return false; }

            var target = path.Replace("/", @"\");
            if (!File.Exists(target) && !Directory.Exists(target)) { return false; }

            try
            {
                Process.Start("EXPLORER.exe", @"/select," + target);
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
                return false;
            }

            return true;
        }

        public static bool Open4Mac(string path)
        {
            if (!IsMac()) { return false; }

            var target = path.Replace(@"\", "/");
            var isFile = File.Exists(target);
            if (!isFile && !Directory.Exists(target)) { return false; }

            if (!target.StartsWith("\"")) { target = "\"" + target; }
            if (!target.EndsWith("\"")) { target = target + "\""; }

            var arg = (isFile ? "-R " : "") + target;

            try
            {
                Process.Start("open", arg);
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
                return false;
            }

            return true;
        }

        public static bool IsWindows()
        {
            return Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer;
        }

        public static bool IsMac()
        {
            return Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer;
        }
    }
}