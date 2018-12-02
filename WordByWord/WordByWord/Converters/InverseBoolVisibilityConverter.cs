using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace WordByWord.Converters
{
    class InverseBoolVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intval)
            {
                if (intval == 0)
                {
                    return Visibility.Hidden;
                }
            }
            if (value is bool boolval)
            {
                if (boolval)
                {
                    return Visibility.Hidden;
                }
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return Visibility.Hidden;
            }
            else
            {
                return Visibility.Visible;
            }
        }
    }
}
