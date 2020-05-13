using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace TableTopCrucible.WPF.Converters
{
    public class ValueSwitchConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(parameter is string param && value is bool val)
            {
                var parameters = 
                    param
                    .Replace(" ", "")
                    .Split("_")
                    .Select(x=>double.Parse(x))
                    .ToArray();
                if (val)
                    return parameters[1];
                else
                    return parameters[0];
            }
            throw new InvalidOperationException($"invalid parameter '{parameter}' or value '{value}'");
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
