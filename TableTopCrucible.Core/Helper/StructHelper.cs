using System;
using System.Collections.Generic;
using System.Text;

namespace TableTopCrucible.Core.Helper
{
    public static class StructHelper
    {
        public static T? ToNullable<T>(this T value) where T : struct
            => value.Equals(default(T)) ? (T?)null : value;
    }
}
