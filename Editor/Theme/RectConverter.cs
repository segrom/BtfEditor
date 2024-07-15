using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BtfEditor.Theme
{
    public class RectConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return new Rect(0d, 0d, (double)values[0], 10);
            //if(values[0] is double d) return new Rect(0d, 0d, d, 10);
           // return new Rect(0d, 0d, 100, 10);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}