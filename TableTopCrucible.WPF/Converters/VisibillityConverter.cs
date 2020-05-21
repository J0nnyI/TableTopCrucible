using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TableTopCrucible.WPF.Converters
{
    /// <summary>
    /// parameters: invert, hidden, collapsed
    /// </summary>
    public class VisibillityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => Convert(value, targetType, parameter as string);
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Convert(value, targetType, parameter as string);

        public static object Convert(object value, Type targetType, string parameter)
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

            return value switch
            {
                Visibility visibillity when targetType == typeof(bool)
                    => invert ^ visibillity != Visibility.Visible,
                bool show when targetType == typeof(Visibility)
                    => invert ^ show ? Visibility.Visible : hiddenVisibillity,
                _ => throw new InvalidOperationException($"cant convert value '{value}' to '{targetType}'")
            };
        }
    }
}
