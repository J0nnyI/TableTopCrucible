using System;
using System.Globalization;
using System.Windows.Data;

using TableTopCrucible.Domain.Models.Sources;

namespace TableTopCrucible.WPF.Converters
{
    public class ItemChangesetConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => _convert(value, targetType);
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => _convert(value, targetType);

        private object _convert(object value, Type targetType)
        {
            return value switch
            {
                Item item when targetType == typeof(ItemChangeset)
                    => new ItemChangeset(item),
                ItemChangeset _ when targetType == typeof(Item)
                    => throw new InvalidOperationException("the way back should be explicit"),
                null
                    => value,
                _ => throw new InvalidOperationException("invalid cast"),
            };
        }
    }
}
