using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MyManual.Converters
{
    /// <summary>
    /// Day 버튼 색상 변환기
    /// Parameter: 해당 버튼의 Day 번호
    /// Value: 현재 Day (WorkingDay 기준)
    /// 현재 Day 이하면 초록색, 초과면 회색
    /// </summary>
    public class DayToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int currentDay && parameter is string dayStr && int.TryParse(dayStr, out int buttonDay))
            {
                // 현재 Day 이하면 초록색 (도달한 Day)
                if (buttonDay <= currentDay)
                {
                    return new SolidColorBrush(Color.FromRgb(0, 150, 45)); // #00962D (Primary Green)
                }
                else
                {
                    return new SolidColorBrush(Color.FromRgb(200, 200, 200)); // 회색 (아직 도달 안함)
                }
            }
            return new SolidColorBrush(Color.FromRgb(200, 200, 200));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Day 버튼 텍스트 색상 변환기
    /// 현재 Day 이하면 흰색, 초과면 회색
    /// </summary>
    public class DayToTextColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int currentDay && parameter is string dayStr && int.TryParse(dayStr, out int buttonDay))
            {
                if (buttonDay <= currentDay)
                {
                    return new SolidColorBrush(Colors.White);
                }
                else
                {
                    return new SolidColorBrush(Color.FromRgb(100, 100, 100)); // 어두운 회색
                }
            }
            return new SolidColorBrush(Color.FromRgb(100, 100, 100));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
