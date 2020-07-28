using System;

namespace TableTopCrucible.WPF.Helper
{
    public static class UriHelper
    {
        public static bool IsAbsolute(this Uri uri)
            => Uri.IsWellFormedUriString(uri.ToString(), UriKind.Absolute);
        public static bool IsRelative(this Uri uri)
            => Uri.IsWellFormedUriString(uri.ToString(), UriKind.Relative);

        public static Uri MakeUnescapedRelativeUri(this Uri dirUri, string fileUri)
            => dirUri.MakeUnescapedRelativeUri(new Uri(fileUri));
        public static Uri MakeUnescapedRelativeUri(this Uri dirUri, Uri fileUri)
            => new Uri(Uri.UnescapeDataString(dirUri.MakeRelativeUri(fileUri).ToString()), UriKind.Relative);
    }
}
