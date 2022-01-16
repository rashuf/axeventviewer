﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace GUI.Convertor
{
    public sealed class EnabledDisabledImageSourceConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string bitmapImageResourceName = (string)parameter;
            if (value == null || !((bool)value))
            {
                bitmapImageResourceName += "Disabled";
            }
            return App.Current.Resources[bitmapImageResourceName];
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
