using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Microsoft.Windows.AppLifecycle;
using Microsoft.Windows.AppNotifications;
using Property_and_Management.src.Model;
using Property_and_Management.src.Repository;
using Property_and_Management.src.Service;
using Property_and_Management.src.Viewmodels;
using Property_and_Management.src.Views;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace Property_and_Management
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        // Public application state
        public Frame RootFrame { get; set; }
        public string AppUserModelId { get; set; }
        public int CurrentUserID { get; set; }

        // Private dependencies and state
        private Window? _mainWindow;
        private NotificationRepository _notificationRepository;
        private NotificationService _notification_service;
        private NotificationsViewModel _notificationsViewModel;
        private readonly NotificationManager _notificationManager;

        /// <summary>
        /// Initializes the singleton application object.
        /// </summary>
        public App()
        {
            int userId = GetUserIdFromArgs();

            AppUserModelId = $"user-{userId}";

            // Create manager and wire its generic handlers (handlers may reference fields initialized later)
            _notificationManager = new NotificationManager();
            SetupNotificationManager();

            EnsureSingleInstance(AppUserModelId);

            InitializeServices(userId);

            InitializeComponent();
        }

        private int GetUserIdFromArgs()
        {
            int defaultUserId = 1;
            string[] commandLineArgs = Environment.GetCommandLineArgs(); // first arg is the executable path
            if (commandLineArgs.Length > 1 && int.TryParse(commandLineArgs[1], out int parsedUserId))
            {
                return parsedUserId;
            }

            return defaultUserId;
        }

        private void SetupNotificationManager()
        {
            // Ensure the manager is unregistered when the process exits
            AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) =>
            {
                _notificationManager.Unregister();
            };

            // When a notification is clicked, bring the window to foreground and optionally navigate
            _notificationManager.NotificationClicked += (sender, eventArguments) =>
            {
                _mainWindow?.DispatcherQueue.TryEnqueue(() =>
                {
                    _mainWindow?.Activate();

                    if (eventArguments.Arguments.ContainsKey("navigate") &&
                        eventArguments.Arguments["navigate"] == nameof(NotificationsPage))
                    {
                        RootFrame.Navigate(typeof(NotificationsPage), _notificationsViewModel);
                    }
                });
            };

            _notificationManager.Init();
        }

        private void EnsureSingleInstance(string appUserModelId)
        {
            var appInstance = Microsoft.Windows.AppLifecycle.AppInstance.FindOrRegisterForKey(appUserModelId);
            if (!appInstance.IsCurrent)
            {
                appInstance.RedirectActivationToAsync(Microsoft.Windows.AppLifecycle.AppInstance.GetCurrent().GetActivatedEventArgs()).AsTask().Wait();
                Environment.Exit(0);
            }
        }

        private void InitializeServices(int userId)
        {
            // Initialize navigation frame
            RootFrame = new Frame();

            // Instantiate repository/service/viewmodel
            _notificationRepository = new NotificationRepository();
            _notification_service = new NotificationService(_notificationRepository);
            _notificationsViewModel = new NotificationsViewModel(_notification_service);

            // Start listening and subscribe for the configured user
            _notification_service.StartListening();
            _notification_service.SubscribeToServer(userId);
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            CreateAndShowMainWindow();
        }

        private void CreateAndShowMainWindow()
        {
            _mainWindow = new MainWindow();
            _mainWindow.Content = RootFrame;
            _mainWindow.Activate();

            // Display the AppUserModelId in the window title for debugging / identification
            _mainWindow.Title = AppUserModelId;
        }
    }
}
