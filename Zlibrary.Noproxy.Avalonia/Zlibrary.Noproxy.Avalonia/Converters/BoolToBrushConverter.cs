using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace Zlibrary.Noproxy.Avalonia.Converters
{
    public class BoolToBrushConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isSelected && parameter is string brushNames)
            {
                var names = brushNames.Split('|');
                if (names.Length == 2)
                {
                    var trueBrush = names[0].Trim();
                    var falseBrush = names[1].Trim();
                    
                    // 处理简单颜色名称
                    if (trueBrush.Equals("Blue", StringComparison.OrdinalIgnoreCase))
                    {
                        return isSelected ? Brushes.Blue : Brushes.Gray;
                    }
                    
                    if (Application.Current?.Resources != null)
                    {
                        return isSelected 
                            ? (Application.Current.Resources.TryGetValue(trueBrush, out var trueValue) ? trueValue as IBrush : null)
                            : (Application.Current.Resources.TryGetValue(falseBrush, out var falseValue) ? falseValue as IBrush : null);
                    }
                }
            }
            
            return Brushes.Gray; // 默认颜色
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 