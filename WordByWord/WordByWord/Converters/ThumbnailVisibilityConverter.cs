using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WordByWord.Converters
{
    public class ThumbnailVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(bool)value)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Hidden;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(bool)value)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Hidden;
            }
        }
    }
}
