using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace TableTopCrucible.WPF.MultiConverters
{
    public class BooleanMultiValueConverter : IMultiValueConverter
    {
        public virtual object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            switch (parameter)
            {
                case string par when par.Contains("and"):
                    return !values.Any(x => (bool)x == false);
                case string par when par.Contains("or"):
                    return values.Any(x => (bool)x);
                case string par when par.Contains("xor"):
                    return (bool)values[0] ^ (bool)values[1];
                case string par when par.Contains("equals"):
                    return values.Any(x => x != values[0]);
                default:
                    throw new InvalidOperationException($"invalid boolean operator parameter: {parameter}");
            }
        }
        public virtual object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) 
            => throw new NotImplementedException();

    }
}
