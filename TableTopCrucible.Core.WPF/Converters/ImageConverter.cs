using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TableTopCrucible.Core.WPF.Converters
{
    public class ImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType == typeof(ImageSource))
            {
                if (value is string str && File.Exists(str))
                {
                    return new BitmapImage(new Uri(str, UriKind.RelativeOrAbsolute));
                }
                else if (value is Uri uri && File.Exists(uri.LocalPath))
                {
                    return new BitmapImage(uri);
                }
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
