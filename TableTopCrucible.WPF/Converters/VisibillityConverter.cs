using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace TableTopCrucible.WPF.Converters
{
    public class VisibillityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => _convert(value, targetType, parameter as string);
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => _convert(value, targetType, parameter as string);

        private object _convert(object value, Type targetType, string parameter)
        {
            bool invert = false;
            Visibility hiddenVisibillity = Visibility.Hidden;
            if (parameter != null)
            {
                parameter = parameter.ToLower();
                invert = parameter.Contains("invert");
                if (parameter.Contains("hidden"))
                    hiddenVisibillity = Visibility.Hidden;
                else if (parameter.Contains("collapsed"))
                    hiddenVisibillity = Visibility.Collapsed;
            }

            switch (value)
            {
                case Visibility visibillity when targetType == typeof(bool):
                    return invert ^ visibillity != Visibility.Visible;
                case bool show when targetType == typeof(Visibility):
                    return invert ^ show ? Visibility.Visible : hiddenVisibillity;
                default:
                    throw new InvalidOperationException($"cant convert value '{value}' to '{targetType}'");
            }
        }
    }
}
