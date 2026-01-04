using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MyManual.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isCompleted)
            {
                return isCompleted ? new SolidColorBrush(Color.FromRgb(0, 168, 120)) // #00A878 (Green)
                                   : new SolidColorBrush(Color.FromRgb(158, 158, 158)); // #9E9E9E (Gray)
            }
            return new SolidColorBrush(Color.FromRgb(158, 158, 158)); // Default gray
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}