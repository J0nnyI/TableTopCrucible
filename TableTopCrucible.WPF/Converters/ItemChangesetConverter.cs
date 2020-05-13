using System;
using System.Globalization;
using System.Windows.Data;

using TableTopCrucible.Domain.Models.Sources;

namespace TableTopCrucible.WPF.Converters
{
    public class ItemChangesetConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => convert(value, targetType);
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => convert(value, targetType);

        private object convert(object value, Type targetType)
        {
            switch (value)
            {
                case Item item when targetType == typeof(ItemChangeset):
                    return new ItemChangeset(item);
                case ItemChangeset changeset when targetType == typeof(Item):
                    throw new InvalidOperationException("the way back should be explicit");
                case null:
                    return value;
                default:
                    throw new InvalidOperationException("invalid cast");
            }
        }
    }
}
