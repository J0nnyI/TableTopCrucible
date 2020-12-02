
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace TableTopCrucible.FeatureCore.WPF.Tagging.Converters
{
    public class TagBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isPrimary)
            {
                return isPrimary ? Brushes.Black : Brushes.DarkGray;
            }
            throw new InvalidOperationException("tagEx expected");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
