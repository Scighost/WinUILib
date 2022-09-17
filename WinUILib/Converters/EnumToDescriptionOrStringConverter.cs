using Microsoft.UI.Xaml.Data;
using System.ComponentModel;

namespace Scighost.WinUILib.Converters;

public class EnumToDescriptionOrStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var e = (Enum)value;
        return e.ToDescription();
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }

}


file static class EnumExtension
{
    public static string ToDescription(this Enum value)
    {
        var text = value.ToString();
        var fieldInfo = value.GetType().GetField(text);
        var descriptionAttribute = fieldInfo?.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault();
        return (descriptionAttribute as DescriptionAttribute)?.Description ?? text;
    }

}
