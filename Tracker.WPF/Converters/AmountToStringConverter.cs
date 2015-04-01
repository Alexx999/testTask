using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Microsoft.SqlServer.Server;

namespace Tracker.WPF.Converters
{
    [ValueConversion(typeof(decimal), typeof(String))]
    class AmountToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            decimal amount = (decimal)value;

            return amount.ToString("C", culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            decimal result;
            decimal.TryParse((string) value, NumberStyles.Currency, culture, out result);
            return result;
        }
    }
}
