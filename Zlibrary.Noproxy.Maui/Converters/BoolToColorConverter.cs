using System.Globalization;

namespace Zlibrary.Noproxy.Maui.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                // 根据参数确定颜色
                var paramString = parameter?.ToString()?.ToLower();
                if (!string.IsNullOrEmpty(paramString))
                {
                    var colors = paramString.Split('|');
                    if (colors.Length >= 2)
                    {
                        // 第一个颜色表示true时的颜色，第二个表示false时的颜色
                        return boolValue ? GetColorFromString(colors[0]) : GetColorFromString(colors[1]);
                    }
                }
                
                // 默认颜色
                return boolValue ? Colors.Orange : Colors.Red;
            }
            
            return Colors.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private Color GetColorFromString(string colorString)
        {
            return colorString.ToLower() switch
            {
                "blue" => Colors.Blue,
                "gray" => Colors.Gray,
                "red" => Colors.Red,
                "orange" => Colors.Orange,
                _ => Colors.Black
            };
        }
    }
}