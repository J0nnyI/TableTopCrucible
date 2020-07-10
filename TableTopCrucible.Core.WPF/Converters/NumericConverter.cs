using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace TableTopCrucible.Core.WPF.Converters
{
    public class NumericConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => _convert(value, targetType, parameter as string);
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => _convert(value, targetType, parameter as string);

        private object _convert(object value, Type targetType, string parameter)
        {
            return value switch
            {
                // implicit
                string str when targetType == typeof(short)
                    => short.Parse(str),
                string str when targetType == typeof(ushort)
                    => ushort.Parse(str),

                string str when targetType == typeof(int)
                    => int.Parse(str),
                string str when targetType == typeof(uint)
                    => uint.Parse(str),

                string str when targetType == typeof(decimal)
                    => decimal.Parse(str),

                string str when targetType == typeof(long)
                    => long.Parse(str),
                string str when targetType == typeof(ulong)
                    => ulong.Parse(str),
                //explicit
                string str when targetType == typeof(object) && parameter.Contains("short")
                    => short.Parse(str),
                string str when targetType == typeof(object) && parameter.Contains("ushort")
                    => ushort.Parse(str),

                string str when targetType == typeof(object) && parameter.Contains("int")
                    => int.Parse(str),
                string str when targetType == typeof(object) && parameter.Contains("uint")
                    => uint.Parse(str),

                string str when targetType == typeof(object) && parameter.Contains("decimal")
                    => decimal.Parse(str),

                string str when targetType == typeof(object) && parameter.Contains("long")
                    => long.Parse(str),
                string str when targetType == typeof(object) && parameter.Contains("ulong")
                    => ulong.Parse(str),

                _ => value.ToString()
            };
        }
    }
}
