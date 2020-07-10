using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Effects;

namespace TableTopCrucible.Core.WPF.Converters
{
    public class BlurConverter : IValueConverter
    {
        private BlurEffect _blur = new BlurEffect();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value == null || (value is bool bv && bv == false )? _blur : null;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
