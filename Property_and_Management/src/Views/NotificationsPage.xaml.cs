using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Property_and_Management.src.Viewmodels;
using Property_and_Management.src.Service;
using Property_and_Management.src.Repository;
using Property_and_Management.src.DTO;
using System.Diagnostics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Property_and_Management.src.Views
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NotificationsPage : Window
    {

        public NotificationsPage(NotificationsViewModel viewModel)
        {
            InitializeComponent();

            if (Content is FrameworkElement root)
            {
                root.DataContext = viewModel;
            }

            // Bind the ItemsListView explicitly in code-behind in case DataContext binding isn't active yet
            ItemsListView.ItemsSource = viewModel.Notifications;
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get the viewmodel from the window's DataContext
                var root = this.Content as FrameworkElement;
                var vm = root?.DataContext as NotificationsViewModel;
                if (vm == null)
                {
                    Debug.WriteLine("NotificationsPage: viewmodel not found on DeleteButton_Click");
                    return;
                }

                // Pick a random user id different from current
                // vm.ChangeFirstText();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DeleteButton_Click error: {ex}");
            }
        }
    }
}
