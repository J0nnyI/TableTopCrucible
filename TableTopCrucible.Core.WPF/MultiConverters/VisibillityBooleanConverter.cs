using System;
using System.Globalization;
using System.Windows.Data;

using TableTopCrucible.Core.WPF.Converters;

namespace TableTopCrucible.Core.WPF.MultiConverters
{
    public class VisibillityBooleanConverter : BooleanMultiValueConverter, IMultiValueConverter
    {
        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            object res = base.Convert(values, targetType, parameter, culture);

            return VisibillityConverter.Convert(res, targetType, parameter as string);
        }
    }
}
