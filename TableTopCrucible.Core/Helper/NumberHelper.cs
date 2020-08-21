﻿using System;
using System.Collections.Generic;
using System.Text;

namespace TableTopCrucible.Core.Helper
{
    public static class NumberHelper
    {
        public static bool Between(this int value, int min, int max)
            => value >= min && value <= max;
    }
}
