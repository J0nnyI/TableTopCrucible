using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

using TableTopCrucible.Core.Helper;

namespace TableTopCrucible.Core.WPF.Converters
{
    public class EnumTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is Enum val ? val.GetValueForComboBox(val.GetType()) : value.ToString();

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException(nameof(EnumTextConverter));
        }
    }
}
