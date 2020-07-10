using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using TableTopCrucible.Domain.Models.ValueTypes;

namespace TableTopCrucible.Domain.Library.WPF.Converters
{
    public class ThumbnailConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => convert(value, targetType);
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => convert(value, targetType);
        private object convert(object value, Type targetType)
        {
            switch (value)
            {
                case Thumbnail thumbnail when targetType == typeof(string) || targetType == typeof(object):
                    return (string)thumbnail;
                case Thumbnail thumbnail when targetType == typeof(ImageSource):
                    return new BitmapImage(new Uri((string)thumbnail));
                case string path when targetType == typeof(Thumbnail):
                    return (Thumbnail)path;
                case string path when targetType == typeof(ImageSource):
                    Uri res;
                    if (!Uri.TryCreate(path, UriKind.RelativeOrAbsolute, out res) || !File.Exists(path))
                        return null;
                    return new BitmapImage(res);
                case null:
                    return null;
                default:
                    throw new InvalidOperationException(
                        $"thumbnail converter: invalid conversion from {value} ({value.GetType()}) to {targetType}");
            }
        }
    }
}
