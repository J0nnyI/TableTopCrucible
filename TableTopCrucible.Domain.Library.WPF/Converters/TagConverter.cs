using System;
using System.Globalization;
using System.Windows.Data;

using TableTopCrucible.Data.Models.ValueTypes;

namespace TableTopCrucible.Domain.Library.WPF.Converters
{
    public class TagConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => convert(value, targetType);
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => convert(value, targetType);
        private object convert(object value, Type targetType)
        {
            switch (value)
            {
                case Tag name when targetType == typeof(string) || targetType == typeof(object):
                    return (string)name;
                case string name when targetType == typeof(Tag):
                    return (Tag)name;
                default:
                    throw new InvalidOperationException("invalid conversion");
            }
        }
    }
}
