using System.Globalization;

namespace Zlibrary.Noproxy.Maui.Converters
{
    public class IsNotNullOrEmptyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool result = value != null;
            
            // 如果有inverse参数，则取反结果
            if (parameter?.ToString()?.ToLower() == "inverse")
            {
                result = !result;
            }
            
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}