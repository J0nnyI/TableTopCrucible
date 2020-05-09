using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace TableTopCrucible.WPF.Converters
{
    class DebugConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
#if DEBUG
            Debugger.Break();
            return value;
#else
            throw new NotImplementedException("Debug only");
#endif
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
#if DEBUG
            Debugger.Break();
            return value;
#else
            throw new NotImplementedException("Debug only");
#endif
        }
    }
}
