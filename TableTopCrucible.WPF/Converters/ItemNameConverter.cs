using System;
using System.Globalization;
using System.Windows.Data;

using TableTopCrucible.Domain.ValueTypes;

namespace TableTopCrucible.WPF.Converters
{
    public class ItemNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => convert(value, targetType);
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => convert(value, targetType);
        private object convert(object value, Type targetType)
        {
            switch (value)
            {
                case ItemName name when targetType == typeof(string):
                    return (string)name;
                case string name when targetType == typeof(ItemName):
                    return (ItemName)name;
                default:
                    throw new InvalidOperationException("invalid conversion");
            }
        }
    }
}
