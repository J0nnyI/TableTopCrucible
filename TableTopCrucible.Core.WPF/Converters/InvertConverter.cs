﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace TableTopCrucible.Core.WPF.Converters
{
    public class InvertConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => convert(value);
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => convert(value);
        private object convert(object value)
        {
            if (value is bool boolean)
                return !boolean;
            throw new InvalidOperationException($"{value} is not a boolean");
        }
    }
}
