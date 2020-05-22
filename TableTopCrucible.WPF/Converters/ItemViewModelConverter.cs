using System;
using System.Globalization;
using System.Windows.Data;

using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.WPF.ViewModels;

namespace TableTopCrucible.WPF.Converters
{
    public class ItemViewModelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => _convert(value, targetType);
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => _convert(value, targetType);

        private object _convert(object value, Type targetType)
        {
            if (targetType != typeof(Item) && targetType != typeof(Item?))
                throw new InvalidCastException($"value {value} of type {value.GetType()} is not convertable to {typeof(Item)}");
            return value switch
            {
                ItemCardViewModel itemCardVm
                    => itemCardVm.Item,
                null when targetType == typeof(Item?)
                    => null,
                _ => throw new InvalidOperationException()
            };
        }
    }
}
