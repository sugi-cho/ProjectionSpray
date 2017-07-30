using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace WindowController {
    public static class CommandLineHelper {
        private static readonly string KEY_PREFIX = "--";
        private static Dictionary<string, string> pairs = null;


        private static void Init() {
            if (pairs != null) {
                return;
            }

            var args = System.Environment.GetCommandLineArgs();
            var key = "";
            pairs = new Dictionary<string, string>();

            foreach(string arg in args) {
                if (arg.StartsWith(KEY_PREFIX)) {
                    // key
                    key = arg;
                } else if(key.Length > 0) {
                    // value
                    pairs[key] = arg;
                    key = "";
                }
            }
        }

        public static List<string> GetKeys() {
            Init();
            return new List<string>(pairs.Keys);
        }

        public static string GetStringValue(string key) {
            Init();
            if (pairs == null || !pairs.ContainsKey(key)) {
                return null;
            }

            return pairs[key];
        }

        public static bool GetFloatValue(string key, out float value) {
            Init();

            var str = GetStringValue(key);
            if (str == null) {
                value = default(float);
                return false;
            }

            return float.TryParse(str, out value);
        }

        public static bool GetIntValue(string key, out int value) {
            Init();

            var str = GetStringValue(key);
            if (str == null) {
                value = default(int);
                return false;
            }

            return int.TryParse(str, out value);
        }

        public static bool GetBoolValue(string key, out bool value) {
            Init();

            var integer = 0;
            if (GetIntValue(key, out integer)) {
                value = integer != 0;
                return true;
            }

            value = default(bool);
            return false;
        }
    }
}
