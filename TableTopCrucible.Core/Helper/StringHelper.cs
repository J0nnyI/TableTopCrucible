using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace TableTopCrucible.Core.Helper
{
    public static class StringHelper
    {
        public static bool Like(this string value, string wildcard)
            => Regex.IsMatch(value, WildcardToRegex(wildcard));
        private static string WildcardToRegex(string pattern)
        {
            return "^" + Regex.Escape(pattern).
            Replace("\\*", ".*").
            Replace("\\?", ".") + "$";
        }
    }
}
