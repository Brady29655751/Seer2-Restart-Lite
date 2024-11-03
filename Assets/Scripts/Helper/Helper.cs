using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace System {
    public static class String {
        public static string ConcatToString(this IEnumerable<string> stringList, string eachConcatWith) {
            string result = string.Empty;
            foreach (var str in stringList) {   
                result += (str + eachConcatWith);
            }
            return result.TrimEnd(eachConcatWith);
        }

        public static string TrimEmpty(this string str) {
            return str.Replace(" ", string.Empty).Replace("\n", string.Empty).Replace("\t", string.Empty);
        } 
        public static string TrimEnd(this string str, string suffix) {
            if (str == suffix)
                return string.Empty;

            return str.EndsWith(suffix) ? str.Substring(0, str.Length - suffix.Length) : str;
        }
        public static bool TryTrimEnd(this string str, string suffix, out string trim) {
            trim = TrimEnd(str, suffix);
            return str.EndsWith(suffix);
        }
        public static string TrimStart(this string str, string prefix) {
            if (str == prefix)
                return string.Empty;
                
            return str.StartsWith(prefix) ? str.Substring(prefix.Length) : str;
        }
        public static bool TryTrimStart(this string str, string prefix, out string trim) {
            trim = TrimStart(str, prefix);
            return str.StartsWith(prefix);
        }

        public static string TrimParentheses(this string str) {
            int startIndex = str.IndexOf('[');
            int endIndex = str.IndexOf(']');
            if ((startIndex == -1) || (endIndex == -1) || (endIndex < startIndex))
                return str;

            return str.Substring(startIndex + 1, endIndex - startIndex - 1);
        }

        public static bool TryTrimParentheses(this string str, out string trim) {
            trim = TrimParentheses(str);
            return trim != str;
        }

        public static string ReplaceColorAndNewline(this string str) {
            return str.Replace("[ENDL]", "\n").Replace("[-]", "</color>").Replace("[", "<color=#").Replace("]", ">");
        }

        public static string GetDescription(this string str) {
            var description = string.Empty;
            var lines = str.Trim().Split('\n');
            for(int i = 0; i < lines.Length; i++) {
                description += lines[i].TrimStart() + "\n";
            }
            return description;
        }

        /// <summary>
        /// Count length without rich-text tags.
        /// </summary>
        public static int GetPlainLength(this string str) {
            string trimStr = str.Trim();
            return trimStr.Length - ((trimStr.Length - trimStr.Replace("color", string.Empty).Length) / 10) * 23;
        }

        public static Vector2 GetPreferredSize(this string str, int maxCharX, int fontSize, int padX = 21, int padY = 21) {
            string[] split = str.Trim().Split('\n');
            int lineSizeX = -1;
            int line = split.Length;
            for (int i = 0; i < split.Length; i++) {
                int length = split[i].GetPlainLength();
                line += (length - 1) / maxCharX;
                lineSizeX = Mathf.Min(Mathf.Max(lineSizeX, length), maxCharX);
            }
            int sizeX = padX + (fontSize) * lineSizeX;
            int sizeY = padY + (fontSize + 5) * line;
            return new Vector2(sizeX, sizeY);
        }

        /// <summary>
        /// Parse the given string to list of float. <br/>
        /// We expect input to be "xxx,yyy,zzz....."
        /// </summary>
        public static List<float> ToFloatList(this string str, char delimeter = ',') {
            if (string.IsNullOrEmpty(str))
                return new List<float>();

            float tmp = 0;
            var split = str.Split(delimeter);
            List<float> list = new List<float>();

            for (int i = 0; i < split.Length; i++) {
                if (float.TryParse(split[i], out tmp)) {
                    list.Add(tmp);
                    continue;
                } 
                tmp = Identifier.GetNumIdentifier(split[i]);
                if (tmp != 0) {
                    list.Add(tmp);
                    continue;
                }
                return null;
            }
            return list;
        }

        public static List<int> ToIntList(this string str, char delimeter = ',') {
            if (string.IsNullOrEmpty(str) || (str == "none"))
                return new List<int>();

            return str.ToFloatList(delimeter)?.Select(x => (int)x).ToList();
        }

        public static List<int> ToIntRange(this string str) {
            if (string.IsNullOrEmpty(str) || (str == "none"))
                return new List<int>();

            var strRanges = str.Replace("－", "-").TrimParentheses().Split('|');
            var allRanges = new List<int>();

            for (int i = 0; i < strRanges.Length; i++) {
                var range = strRanges[i].Split('~').Select(int.Parse).ToList();
                if (range.Count == 1) {
                    allRanges.Add(range[0]);
                    continue;
                }
                int min = Mathf.Min(range[0], range[1]);
                int max = Mathf.Max(range[0], range[1]);
                allRanges.AddRange(Enumerable.Range(min, max - min + 1));               
            }

            return allRanges;
        }

        /// <summary>
        /// Parse the given string to Vector2. <br/>
        /// We expect input to be "xxx,yyy"
        /// </summary>
        public static Vector2 ToVector2(this string pos, Vector2 defaultValue = default(Vector2)) {
            var list = pos.ToFloatList();
            return ((list == null) || (list.Count != 2)) ? defaultValue : new Vector2(list[0], list[1]);
        }

        /// <summary>
        /// Parse the given string to Vector4. <br/>
        /// We expect input to be "xxx,yyy,zzz,www"
        /// </summary>
        public static Vector4 ToVector4(this string pos, Vector4 defaultValue = default(Vector4)) {
            var list = pos.ToFloatList();
            return ((list == null) || (list.Count != 4)) ? defaultValue : new Vector4(list[0], list[1], list[2], list[3]);
        }

        /// <summary>
        /// Parse the given string to Color. <br/>
        /// We expect input to be "rrr,ggg,bbb,aaa"
        /// </summary>
        public static Color ToColor(this string color, Color defaultValue = default(Color)) {
            var list = color.ToIntList().Select(x => (byte)x).ToList();
            Color color32 = new Color32(list[0], list[1], list[2], list[3]);
            return ((list == null) || (list.Count != 4)) ? defaultValue : color32;
        }

        /// <summary>
        /// Parse the given string to Quaternion. <br/>
        /// We expect input to be "xxx,yyy,zzz"
        /// </summary>
        public static Quaternion ToQuaternion(this string rotation, Quaternion defaultValue = default(Quaternion)) {
            var q = Quaternion.identity;
            var list = rotation.ToFloatList();
            q.eulerAngles = ((list == null) || (list.Count != 3)) ? defaultValue.eulerAngles : new Vector3(list[0], list[1], list[2]);
            return q;
        }
    }
}
