using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Utilities;

namespace Zlibrary.Noproxy.Avalonia.Converters
{
    public class BoolToClassesConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue && parameter is string classes)
            {
                var classList = classes.Split('|');
                return boolValue ? classList[0] : classList.Length > 1 ? classList[1] : string.Empty;
            }
            return string.Empty;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IsNotNullOrEmptyToDouble : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            bool result;
            if (value == null)
                result = false;
            else if (value is string str)
                result = !string.IsNullOrEmpty(str);
            else
                result = true;

            // 检查是否需要反转结果
            if (parameter is string param && param.ToLower() == "inverse")
                result = !result;

            return result;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}