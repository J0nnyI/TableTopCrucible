using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using TableTopCrucible.Domain.ValueTypes;

namespace TableTopCrucible.WPF.Converters
{
    public class ThumbnailConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => convert(value, targetType);
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => convert(value, targetType);
        private object convert(object value, Type targetType)
        {
            switch (value)
            {
                case Thumbnail thumbnail when targetType == typeof(string) || targetType == typeof(object):
                    return (string)thumbnail;
                case string path when targetType == typeof(Thumbnail):
                    return (Thumbnail)path;
                default:
                    throw new InvalidOperationException("invalid conversion");
            }
        }
    }
}
