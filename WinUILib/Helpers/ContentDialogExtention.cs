using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;

namespace Scighost.WinUILib.Helpers;

public static class ContentDialogExtention
{


    public static async Task<ContentDialogResult> ShowWithZeroMarginAsync(this ContentDialog dialog)
    {
        dialog.Loaded += (_, _) =>
        {
            var popups = VisualTreeHelper.GetOpenPopupsForXamlRoot(dialog.XamlRoot);
            foreach (var popup in popups)
            {
                if (popup.Child is Rectangle { Name: "SmokeLayerBackground" } rectangle)
                {
                    rectangle.Margin = new Thickness();
                    rectangle.RegisterPropertyChangedCallback(FrameworkElement.MarginProperty, (s, e) => s.ClearValue(e));
                }
            }
        };
        return await dialog.ShowAsync();
    }


}

