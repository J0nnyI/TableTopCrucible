using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Navigation;

namespace TableTopCrucible.WPF.Helper
{
    public static class UriHelper
    {
        public static bool IsAbsolute(this Uri uri)
            => Uri.IsWellFormedUriString(uri.ToString(), UriKind.Absolute);
        public static bool IsRelative(this Uri uri)
            => Uri.IsWellFormedUriString(uri.ToString(), UriKind.Relative);
    }
}
