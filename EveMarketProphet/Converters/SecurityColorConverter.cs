using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using EveMarketProphet.Utils;

namespace EveMarketProphet.Converters
{
    [ValueConversion(typeof(double), typeof(SolidColorBrush))]
    public class SecurityColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var s = SecurityUtils.RoundSecurity((double)value);
            
            var c = SecurityUtils.SecurityColors[s];
            c.A = 127; // 50% alpha, 7F

            return new SolidColorBrush(c);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
