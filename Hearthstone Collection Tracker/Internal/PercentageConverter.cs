using System;
using System.Globalization;
using System.Windows.Data;

namespace Hearthstone_Collection_Tracker.Internal
{
    internal class PercentageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double num;
            if (Double.TryParse(System.Convert.ToString(value, culture), NumberStyles.Any, NumberFormatInfo.InvariantInfo, out num))
            {
                return string.Format("{0:0.##}%", num * 100);
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
