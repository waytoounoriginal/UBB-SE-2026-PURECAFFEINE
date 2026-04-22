using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using H.NotifyIcon;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Microsoft.Windows.AppLifecycle;
using Microsoft.Windows.AppNotifications;
using Property_and_Management.Src.DataTransferObjects;
using Property_and_Management.Src.Interface;
using Property_and_Management.Src.Mapper;
using Property_and_Management.Src.Model;
using Property_and_Management.Src.Repository;
using Property_and_Management.Src;
using Property_and_Management.Src.Service;
using Property_and_Management.Src.Service.Listeners;
using Property_and_Management.Src.Viewmodels;
using Property_and_Management.Src.Views;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace Property_and_Management
{
    public partial class App : Application
    {
        private const int DefaultUserId = 1;
        private const int UserIdentifierArgumentIndex = 1;
        private const int KeyPartIndex = 0;
        private const int ValuePartIndex = 1;
        private const int SplitKeyValuePartsCount = 2;
        private const int DevModePrimaryUserIdentifier = 1;
        private const int DevModeSecondaryUserIdentifier = 2;
        private const int NoRunningProcessCount = 0;
        private const int SuccessExitCode = 0;

        private const string TwoWindowsEnvironmentKey = "TWO_WINDOWS";
        private const string EnabledEnvironmentValue = "true";
        private const string NotificationNavigationArgumentKey = "navigate";

        public static IServiceProvider Services { get; private set; } = default!;

        public static Window MainWindow { get; set; }
        public Frame RootFrame { get; set; }
        public string AppUserModelId { get; }
        public int CurrentUserId { get; }
        public NotificationsViewModel NotificationsViewModel { get; private set; }

        private TaskbarIcon trayIcon;

        private static Process? notificationServerProcess;
        private static Process? secondClientProcess;

        private Window? mainWindow;
        private INotificationRepository notificationRepository;
        private INotificationService notificationService;
        private IGameRepository gameRepository;
        private IGameService gameService;
        private readonly NotificationManager notificationManager;

        public App()
        {
            CurrentUserId = GetUserIdFromArgs();

            DatabaseInitializer.EnsureDatabaseInitialized();

            if (CurrentUserId == DevModePrimaryUserIdentifier && IsTwoWindowsEnabled())
            {
                StartNotificationServer();
                LaunchSecondClient();
            }

            AppUserModelId = $"BoardRent -- user-{CurrentUserId}";

            notificationManager = new NotificationManager();

            SetupNotificationManager();

            EnsureSingleInstance(AppUserModelId);

            ConfigureServices();
            InitializeServices(CurrentUserId);

            InitializeComponent();
        }

        private void ConfigureServices()
        {
            var applicationServiceCollection = new ServiceCollection();

            applicationServiceCollection.AddSingleton<IMapper<User, UserDTO>, UserMapper>();
            applicationServiceCollection.AddSingleton<IMapper<Game, GameDTO>, GameMapper>();
            applicationServiceCollection.AddSingleton<IMapper<Notification, NotificationDTO>, NotificationMapper>();
            applicationServiceCollection.AddSingleton<IMapper<Rental, RentalDTO>, RentalMapper>();
            applicationServiceCollection.AddSingleton<IMapper<Request, RequestDTO>, RequestMapper>();

            applicationServiceCollection.AddSingleton<ICurrentUserContext>(new CurrentUserContext(CurrentUserId));
            applicationServiceCollection.AddSingleton<IToastNotificationService, ToastNotificationService>();
            applicationServiceCollection.AddSingleton<IServerClient, NotificationClient>();

            applicationServiceCollection.AddSingleton<IUserRepository, UserRepository>();
            applicationServiceCollection.AddSingleton<IGameRepository, GameRepository>();
            applicationServiceCollection.AddSingleton<IRequestRepository, RequestRepository>();
            applicationServiceCollection.AddSingleton<IRentalRepository, RentalRepository>();
            applicationServiceCollection.AddSingleton<INotificationRepository, NotificationRepository>();

            applicationServiceCollection.AddSingleton<IUserService, UserService>();
            applicationServiceCollection.AddSingleton<IGameService, GameService>();
            applicationServiceCollection.AddSingleton<IRentalService, RentalService>();
            applicationServiceCollection.AddSingleton<INotificationService, NotificationService>();
            applicationServiceCollection.AddSingleton<IRequestService, RequestService>();

            applicationServiceCollection.AddSingleton<NotificationsViewModel>();
            applicationServiceCollection.AddSingleton<MenuBarViewModel>();
            applicationServiceCollection.AddTransient(serviceProvider => new ListingsViewModel(
                serviceProvider.GetRequiredService<IGameService>(),
                serviceProvider.GetRequiredService<ICurrentUserContext>().CurrentUserId));
            applicationServiceCollection.AddTransient<CreateGameViewModel>();
            applicationServiceCollection.AddTransient<EditGameViewModel>();
            applicationServiceCollection.AddTransient<CreateRequestViewModel>();
            applicationServiceCollection.AddTransient<CreateRentalViewModel>();
            applicationServiceCollection.AddTransient<RequestsFromOthersViewModel>();
            applicationServiceCollection.AddTransient<RequestsToOthersViewModel>();
            applicationServiceCollection.AddTransient<RentalsFromOthersViewModel>();
            applicationServiceCollection.AddTransient<RentalsToOthersViewModel>();

            Services = applicationServiceCollection.BuildServiceProvider();
        }

        private int GetUserIdFromArgs()
        {
            string[] commandLineArgs = Environment.GetCommandLineArgs();
            if (commandLineArgs.Length > UserIdentifierArgumentIndex && int.TryParse(commandLineArgs[UserIdentifierArgumentIndex], out int parsedUserIdentifier))
            {
                return parsedUserIdentifier;
            }

            return DefaultUserId;
        }

        #region Two-window dev mode

        private static string? FindRepoRoot()
        {
            var currentDirectory = new DirectoryInfo(AppContext.BaseDirectory);
            while (currentDirectory != null)
            {
                if (Directory.Exists(System.IO.Path.Combine(currentDirectory.FullName, ".git")))
                {
                    return currentDirectory.FullName;
                }

                currentDirectory = currentDirectory.Parent;
            }
            return null;
        }

        private static bool IsTwoWindowsEnabled()
        {
            try
            {
                var repoRoot = FindRepoRoot();
                if (repoRoot == null)
                {
                    return false;
                }

                var envPath = System.IO.Path.Combine(repoRoot, ".env");
                if (!File.Exists(envPath))
                {
                    return false;
                }

                foreach (var line in File.ReadAllLines(envPath))
                {
                    var trimmed = line.Trim();
                    if (trimmed.StartsWith('#') || !trimmed.Contains('='))
                    {
                        continue;
                    }

                    var parts = trimmed.Split('=', SplitKeyValuePartsCount);
                    if (parts[KeyPartIndex].Trim() == TwoWindowsEnvironmentKey)
                    {
                        return parts[ValuePartIndex].Trim().Equals(EnabledEnvironmentValue, StringComparison.OrdinalIgnoreCase);
                    }
                }
            }
            catch
            {
            }
            return false;
        }

        private static void StartNotificationServer()
        {
            try
            {
                if (Process.GetProcessesByName("NotificationServer").Length > NoRunningProcessCount)
                {
                    return;
                }

                var repoRoot = FindRepoRoot();
                if (repoRoot == null)
                {
                    return;
                }

                var serverBinDir = System.IO.Path.Combine(repoRoot, "NotificationServer", "bin");
                if (!Directory.Exists(serverBinDir))
                {
                    return;
                }

                var serverExe = Directory.GetFiles(serverBinDir, "NotificationServer.exe", SearchOption.AllDirectories)
                    .FirstOrDefault();
                if (serverExe == null)
                {
                    return;
                }

                notificationServerProcess = Process.Start(new ProcessStartInfo
                {
                    FileName = serverExe,
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Minimized
                });
            }
            catch
            {
            }
        }

        private static void LaunchSecondClient()
        {
            try
            {
                var currentExe = Environment.ProcessPath;
                if (currentExe == null)
                {
                    return;
                }

                secondClientProcess = Process.Start(new ProcessStartInfo
                {
                    FileName = currentExe,
                    Arguments = DevModeSecondaryUserIdentifier.ToString(),
                    UseShellExecute = true,
                    WorkingDirectory = System.IO.Path.GetDirectoryName(currentExe)
                });
            }
            catch
            {
            }
        }

        private static void KillSpawnedChildProcesses()
        {
            try
            {
                if (secondClientProcess != null && !secondClientProcess.HasExited)
                {
                    secondClientProcess.Kill(entireProcessTree: true);
                }
            }
            catch
            {
            }

            try
            {
                if (notificationServerProcess != null && !notificationServerProcess.HasExited)
                {
                    notificationServerProcess.Kill(entireProcessTree: true);
                }
            }
            catch
            {
            }
        }

        #endregion

        private void SetupNotificationManager()
        {
            AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) =>
            {
                notificationManager.Unregister();
                (notificationService as IDisposable)?.Dispose();
                KillSpawnedChildProcesses();
            };

            notificationManager.NotificationClicked += (sender, notificationActivationArgs) =>
            {
                mainWindow?.DispatcherQueue.TryEnqueue(() =>
                {
                    mainWindow?.Activate();

                    if (notificationActivationArgs.Arguments.ContainsKey(NotificationNavigationArgumentKey) &&
                        notificationActivationArgs.Arguments[NotificationNavigationArgumentKey] == nameof(NotificationsPage))
                    {
                        ActivateWindow();
                        NavigateToNotificationsWithinShell();
                    }
                });
            };

            notificationManager.Init();
        }

        private void NavigateToNotificationsWithinShell()
        {
            if (RootFrame?.Content is MenuBarView currentShell)
            {
                currentShell.NavigateToNotifications();
                return;
            }

            void OnShellLoaded(object sender, NavigationEventArgs navigationEventArgs)
            {
                if (navigationEventArgs.Content is MenuBarView loadedShell)
                {
                    RootFrame.Navigated -= OnShellLoaded;
                    loadedShell.NavigateToNotifications();
                }
            }

            RootFrame.Navigated += OnShellLoaded;
            RootFrame.Navigate(typeof(MenuBarView), gameService);
        }

        private void EnsureSingleInstance(string appUserModelId)
        {
            var appInstance = Microsoft.Windows.AppLifecycle.AppInstance.FindOrRegisterForKey(appUserModelId);
            if (!appInstance.IsCurrent)
            {
                appInstance.RedirectActivationToAsync(Microsoft.Windows.AppLifecycle.AppInstance.GetCurrent().GetActivatedEventArgs()).AsTask().Wait();
                Environment.Exit(SuccessExitCode);
            }

            appInstance.Activated += (sender, activationEventArgs) =>
            {
                ActivateWindow();
            };
        }

        private void InitializeServices(int startupUserId)
        {
            RootFrame = new Frame();

            notificationRepository = Services.GetRequiredService<INotificationRepository>();
            notificationService = Services.GetRequiredService<INotificationService>();
            gameRepository = Services.GetRequiredService<IGameRepository>();
            gameService = Services.GetRequiredService<IGameService>();
            NotificationsViewModel = Services.GetRequiredService<NotificationsViewModel>();

            notificationService.StartListening();
            notificationService.SubscribeToServer(startupUserId);
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            CreateAndShowMainWindow();

            Grid rootGrid = new Grid();
            rootGrid.Children.Add(RootFrame);
            MainWindow.Content = rootGrid;

            RootFrame.Navigate(typeof(MenuBarView), gameService);

            CreateTrayIcon();
        }

        private void CreateAndShowMainWindow()
        {
            MainWindow = mainWindow = new MainWindow();
            mainWindow.Content = RootFrame;
            mainWindow.Activate();

            mainWindow.Title = AppUserModelId;
        }

        private void ActivateWindow()
        {
            mainWindow?.DispatcherQueue.TryEnqueue(() =>
            {
                if (mainWindow is MainWindow activatedMainWindow)
                {
                    activatedMainWindow.AppWindow.Show();
                }
                mainWindow.Activate();
            });
        }

        private void CreateTrayIcon()
        {
            trayIcon = new TaskbarIcon
            {
                ToolTipText = $"{AppUserModelId}",
                IconSource = new BitmapImage(new Uri(Constants.AppTrayIconUri)),
            };

            var trayOpenCommand = new XamlUICommand();
            trayOpenCommand.ExecuteRequested += (commandSender, executeRequestedEventArgs) =>
            {
                ActivateWindow();
            };

            var trayOpenMenuItem = new MenuFlyoutItem
            {
                Text = "Open",
                Command = trayOpenCommand
            };

            var trayExitCommand = new XamlUICommand();
            trayExitCommand.ExecuteRequested += (commandSender, executeRequestedEventArgs) =>
            {
                trayIcon.Dispose();
                Environment.Exit(SuccessExitCode);
            };

            var trayExitMenuItem = new MenuFlyoutItem
            {
                Text = "Exit",
                Command = trayExitCommand
            };

            trayIcon.ContextFlyout = new MenuFlyout
            {
                Items =
                {
                    trayOpenMenuItem, trayExitMenuItem
                }
            };

            if (mainWindow.Content is Grid rootGrid)
            {
                rootGrid.Children.Add(trayIcon);
            }
        }
    }
}