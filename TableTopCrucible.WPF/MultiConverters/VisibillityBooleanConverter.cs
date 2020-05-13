using MaterialDesignThemes.Wpf.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using TableTopCrucible.WPF.Converters;

namespace TableTopCrucible.WPF.MultiConverters
{
    public class VisibillityBooleanConverter : BooleanMultiValueConverter, IMultiValueConverter
    {
        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            object res = base.Convert(values, targetType, parameter, culture);

            return VisibillityConverter.Convert(res,targetType, parameter as string);
        }
    }
}
