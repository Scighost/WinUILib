﻿using Microsoft.UI.Xaml.Data;

namespace Scighost.WinUILib.Converters;

public class DateTimeToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is DateTimeOffset time1)
        {
            if (parameter is string { Length: > 0 } param)
            {
                return time1.ToString(param);
            }
            else
            {
                return time1.ToString("G");
            }
        }
        if (value is DateTime time2)
        {
            if (parameter is string { Length: > 0 } param)
            {
                return time2.ToString(param);
            }
            else
            {
                return time2.ToString("G");
            }
        }
        return "";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
