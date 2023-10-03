using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace JiME.Common
{
    enum CloseButtonVisibility
    {
        Visible,
        Hidden,
        CloseDisabled,
    }

    // https://stackoverflow.com/questions/743906/how-to-hide-close-button-in-wpf-window

    static class WindowExtension
    {
        private static readonly CancelEventHandler _cancelCloseHandler = (sender, e) => e.Cancel = true;

        public static readonly DependencyProperty CloseButtonVisibilityProperty =
            DependencyProperty.RegisterAttached(
                "CloseButtonVisibility",
                typeof(CloseButtonVisibility),
                typeof(WindowExtension),
                new FrameworkPropertyMetadata(CloseButtonVisibility.Visible, new PropertyChangedCallback(_OnCloseButtonChanged)));

        [AttachedPropertyBrowsableForType(typeof(Window))]
        public static CloseButtonVisibility GetCloseButtonVisibility(Window obj)
        {
            return (CloseButtonVisibility)obj.GetValue(CloseButtonVisibilityProperty);
        }

        [AttachedPropertyBrowsableForType(typeof(Window))]
        public static void SetCloseButtonVisibility(Window obj, CloseButtonVisibility value)
        {
            obj.SetValue(CloseButtonVisibilityProperty, value);
        }

        private static void _OnCloseButtonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is Window window))
            {
                return;
            }

            if (e.OldValue is CloseButtonVisibility oldVisibility)
            {
                if (oldVisibility == CloseButtonVisibility.CloseDisabled)
                {
                    window.Closing -= _cancelCloseHandler;
                }
            }

            if (e.NewValue is CloseButtonVisibility newVisibility)
            {
                if (newVisibility == CloseButtonVisibility.CloseDisabled)
                {
                    window.Closing += _cancelCloseHandler;
                }

                if (!window.IsLoaded)
                {
                    // NOTE: if the property is set multiple times before the window is loaded,
                    // the window will wind up with multiple event handlers. But they will all
                    // set the same value, so this is fine from a functionality point of view.
                    //
                    // The handler is never unsubscribed, so there is some nominal overhead there.
                    // But it would be incredibly unusual for this to be set more than once
                    // before the window is loaded, and even a handful of delegate instances
                    // being around that are no longer needed is not really a big deal.
                    window.Loaded += _ApplyCloseButtonVisibility;
                }
                else
                {
                    _SetVisibility(window, newVisibility);
                }
            }
        }

        #region Win32 imports

        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        #endregion

        private static void _ApplyCloseButtonVisibility(object sender, RoutedEventArgs e)
        {
            Window window = (Window)sender;
            CloseButtonVisibility visibility = GetCloseButtonVisibility(window);

            _SetVisibility(window, visibility);
        }

        private static void _SetVisibility(Window window, CloseButtonVisibility visibility)
        {
            var hwnd = new WindowInteropHelper(window).Handle;

            if (visibility == CloseButtonVisibility.Visible)
            {
                SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) | WS_SYSMENU);
            }
            else
            {
                SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
            }
        }
    }
}
