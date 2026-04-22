using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using H.NotifyIcon;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinRT.Interop;

namespace Property_and_Management
{
    public sealed partial class MainWindow : Window
    {
        private const int SuccessExitCode = 0;

        public new AppWindow AppWindow { get; }

        public MainWindow()
        {
            InitializeComponent();

            AppWindow = GetAppWindow();

            AppWindow.Closing += (sender, args) =>
            {
                Environment.Exit(SuccessExitCode);
            };
        }

        private AppWindow GetAppWindow()
        {
            var mainWindowHandle = WindowNative.GetWindowHandle(this);
            var mainWindowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(mainWindowHandle);
            return AppWindow.GetFromWindowId(mainWindowId);
        }
    }
}