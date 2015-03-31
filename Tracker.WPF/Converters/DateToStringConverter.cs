using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Tracker.WPF.Converters
{
    [ValueConversion(typeof(DateTime), typeof(String))]
    class DateToStringConverter : IValueConverter
    {
        private const string Format = "d";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime time = (DateTime)value;

            return time.ToString(Format, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime result;
            DateTime.TryParseExact((string)value, Format, culture, DateTimeStyles.None, out result);
            return result;
        }
    }
}
