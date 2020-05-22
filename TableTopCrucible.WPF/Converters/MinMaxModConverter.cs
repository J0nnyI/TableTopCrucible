using System;
using System.Globalization;
using System.Windows.Data;

namespace TableTopCrucible.WPF.Converters
{


    public class MinMaxModConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return parseParams(parameter as string, (double)value);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"could not parse parameters for {nameof(MinMaxModConverter)} ({value}, {parameter})", ex);
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();

        private double parseParams(string rawValue, double value)
        {
            var values = rawValue.ToLower().Split("_");

            if (values.Length != 3)
                throw new InvalidOperationException("3 values (min, target,max separated by '_' are expected.");

            double min = parseNumber(values[0], value);
            double target = parseNumber(values[1], value);
            double max = parseNumber(values[2], value);

            if (target < min)
                return min;
            if (target > max)
                return max;
            return target;
        }
        private double parseNumber(string rawValue, double inputSize)
        {
            double value = double.Parse(rawValue.Replace("%", ""));
            return rawValue.EndsWith("%") ? inputSize * (value / 100) : value;
        }
    }
}
