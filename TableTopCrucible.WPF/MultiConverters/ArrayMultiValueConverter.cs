using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace TableTopCrucible.WPF.MultiConverters
{
    class ArrayMultiValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) 
            => values.ToArray();
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => value as object[];
    }
}
