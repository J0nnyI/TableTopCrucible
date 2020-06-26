﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TableTopCrucible.Core.Utilities
{
    public static class ObjectHelper
    {
        public static T[] AsArray<T>(this T obj, params T[] otherValues)
        {
            List<T> values = otherValues.ToList();
            values.Insert(0, obj);
            return values.ToArray();
        }
    }
}
