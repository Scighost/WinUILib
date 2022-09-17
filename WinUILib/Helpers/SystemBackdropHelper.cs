using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using System.Runtime.InteropServices;
using WinRT;

namespace Scighost.WinUILib.Helpers;

public class SystemBackdropHelper
{

    private readonly Window m_window;
    private WindowsSystemDispatcherQueueHelper? m_wsdqHelper; // See below for implementation.
    private SystemBackdropConfiguration? m_configurationSource;


    private bool m_alwaysActive;


    public MicaController? MicaController { get; private set; }
    public DesktopAcrylicController? AcrylicController { get; private set; }



    public SystemBackdropHelper(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);
        m_window = window;
    }




    public bool TrySetMica(bool useMicaAlt = false, bool fallbackToAcrylic = false, bool alwaysActive = false)
    {
        if (MicaController.IsSupported())
        {
            m_wsdqHelper = new WindowsSystemDispatcherQueueHelper();
            m_wsdqHelper.EnsureWindowsSystemDispatcherQueueController();

            // Create the policy object.
            m_configurationSource = new SystemBackdropConfiguration();
            m_window.Activated += Window_Activated;
            m_window.Closed += Window_Closed;
            ((FrameworkElement)m_window.Content).ActualThemeChanged += Window_ThemeChanged;

            // Initial configuration state.
            m_configurationSource.IsInputActive = true;
            SetConfigurationSourceTheme();

            MicaController = new MicaController { Kind = useMicaAlt ? MicaKind.BaseAlt : MicaKind.Base };

            // Enable the system backdrop.
            // Note: Be sure to have "using WinRT;" to support the Window.As<...>() call.
            MicaController.AddSystemBackdropTarget(m_window.As<ICompositionSupportsSystemBackdrop>());
            MicaController.SetSystemBackdropConfiguration(m_configurationSource);

            m_alwaysActive = alwaysActive;
            return true; // succeeded
        }
        else if (fallbackToAcrylic)
        {
            return TrySetAcrylic(alwaysActive);
        }
        else
        {
            return false;
        }
    }



    public bool TrySetAcrylic(bool alwaysActive = false)
    {
        if (DesktopAcrylicController.IsSupported())
        {
            m_wsdqHelper = new WindowsSystemDispatcherQueueHelper();
            m_wsdqHelper.EnsureWindowsSystemDispatcherQueueController();

            // Create the policy object.
            m_configurationSource = new SystemBackdropConfiguration();
            m_window.Activated += Window_Activated;
            m_window.Closed += Window_Closed;
            ((FrameworkElement)m_window.Content).ActualThemeChanged += Window_ThemeChanged;

            // Initial configuration state.
            m_configurationSource.IsInputActive = true;
            SetConfigurationSourceTheme();

            AcrylicController = new DesktopAcrylicController();

            // Enable the system backdrop.
            // Note: Be sure to have "using WinRT;" to support the Window.As<...>() call.
            AcrylicController.AddSystemBackdropTarget(m_window.As<ICompositionSupportsSystemBackdrop>());
            AcrylicController.SetSystemBackdropConfiguration(m_configurationSource);

            m_alwaysActive = alwaysActive;
            return true; // succeeded
        }
        else
        {
            return false;
        }
    }




    private void Window_Activated(object sender, WindowActivatedEventArgs args)
    {
        if (m_configurationSource != null)
        {
            m_configurationSource.IsInputActive = m_alwaysActive || (args.WindowActivationState != WindowActivationState.Deactivated);
        }
    }


    private void Window_ThemeChanged(FrameworkElement sender, object args)
    {
        if (m_configurationSource != null)
        {
            SetConfigurationSourceTheme();
        }
    }


    private void Window_Closed(object sender, WindowEventArgs args)
    {
        // Make sure any Mica/Acrylic controller is disposed
        // so it doesn't try to use this closed window.
        if (MicaController != null)
        {
            MicaController.Dispose();
            MicaController = null;
        }
        if (AcrylicController != null)
        {
            AcrylicController.Dispose();
            AcrylicController = null;
        }
        m_window.Activated -= Window_Activated;
        m_configurationSource = null;
    }



    private void SetConfigurationSourceTheme()
    {
        if (m_configurationSource != null)
        {
            m_configurationSource.Theme = ((FrameworkElement)m_window.Content).ActualTheme switch
            {
                ElementTheme.Light => SystemBackdropTheme.Light,
                ElementTheme.Dark => SystemBackdropTheme.Dark,
                _ => SystemBackdropTheme.Default,
            };
        }
    }



    private class WindowsSystemDispatcherQueueHelper
    {
        [StructLayout(LayoutKind.Sequential)]
        struct DispatcherQueueOptions
        {
            internal int dwSize;
            internal int threadType;
            internal int apartmentType;
        }

        [DllImport("CoreMessaging.dll")]
        private static extern int CreateDispatcherQueueController([In] DispatcherQueueOptions options, [In, Out, MarshalAs(UnmanagedType.IUnknown)] ref object? dispatcherQueueController);

        object? m_dispatcherQueueController = null;
        public void EnsureWindowsSystemDispatcherQueueController()
        {
            if (Windows.System.DispatcherQueue.GetForCurrentThread() != null)
            {
                // one already exists, so we'll just use it.
                return;
            }

            if (m_dispatcherQueueController == null)
            {
                DispatcherQueueOptions options;
                options.dwSize = Marshal.SizeOf(typeof(DispatcherQueueOptions));
                options.threadType = 2;    // DQTYPE_THREAD_CURRENT
                options.apartmentType = 2; // DQTAT_COM_STA

                CreateDispatcherQueueController(options, ref m_dispatcherQueueController);
            }
        }
    }


}
