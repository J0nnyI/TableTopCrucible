using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TableTopCrucible.Core.Helper
{
    public static class ObjectHelper
    {
        public static T[] AsArray<T>(this T obj, params T[] otherValues)
        {
            List<T> values = otherValues.ToList();
            values.Insert(0, obj);
            return values.ToArray();
        }
        public static bool IsIn<T>(this T obj, params T[] compValues) => compValues.Contains(obj);
    }
}
