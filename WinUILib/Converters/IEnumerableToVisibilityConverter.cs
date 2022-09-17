using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System.Collections.Generic;

namespace Scighost.WinUILib.Converters;

public class IEnumerableToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is IEnumerable<int> e)
        {
            using var et = e.GetEnumerator();
            return et.MoveNext() ? Visibility.Visible : Visibility.Collapsed;
        }
        else
        {
            return Visibility.Collapsed;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}


public class IEnumerableToVisibilityReversedConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is IEnumerable<int> e)
        {
            using var et = e.GetEnumerator();
            return et.MoveNext() ? Visibility.Collapsed : Visibility.Visible;
        }
        else
        {
            return Visibility.Visible;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
