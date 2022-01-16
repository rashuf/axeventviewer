using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace GUI.Convertor
{
    public class DateTImeConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TimeZone tz = TimeZone.CurrentTimeZone;
            DateTime dateTime = tz.ToLocalTime((DateTime)value);

            return dateTime.ToString("dd.MM.yy HH:mm:ss");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
