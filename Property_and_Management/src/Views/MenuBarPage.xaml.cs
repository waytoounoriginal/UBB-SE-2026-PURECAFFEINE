using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Property_and_Management.Src.Interface;
using Property_and_Management.Src.Viewmodels;

namespace Property_and_Management.Src.Views
{
    public sealed partial class MenuBarView : Page
    {
        public MenuBarViewModel ViewModel { get; }

        private static readonly Dictionary<AppPage, Type> PageTypeMap = new()
        {
            { AppPage.Listings,            typeof(ListingsPage) },
            { AppPage.RequestsFromOthers,  typeof(RequestsFromOthersPage) },
            { AppPage.RentalsFromOthers,   typeof(RentalsFromOthersPage) },
            { AppPage.RequestsToOthers,    typeof(RequestsToOthersPage) },
            { AppPage.RentalsToOthers,     typeof(RentalsToOthersPage) },
            { AppPage.Notifications,       typeof(NotificationsPage) }
        };

        private IGameService injectedGameService;

        public MenuBarView()
        {
            this.InitializeComponent();
            ViewModel = App.Services.GetRequiredService<MenuBarViewModel>();
            this.DataContext = ViewModel;
            ViewModel.RequestNavigation += OnViewModelRequestedNavigation;
            this.Unloaded += OnMenuBarViewUnloaded;
        }

        protected override void OnNavigatedTo(NavigationEventArgs navigationEventArgs)
        {
            base.OnNavigatedTo(navigationEventArgs);

            if (navigationEventArgs.Parameter is IGameService gameService)
            {
                injectedGameService = gameService;
            }
        }

        private void OnViewModelRequestedNavigation(AppPage page)
        {
            if (!PageTypeMap.TryGetValue(page, out var pageType))
            {
                return;
            }

            ContentFrame.Navigate(pageType, injectedGameService);
        }

        private void OnMenuBarViewUnloaded(object pageSender, RoutedEventArgs unloadedEventArgs)
        {
            ViewModel.RequestNavigation -= OnViewModelRequestedNavigation;
        }

        public void NavigateToNotifications()
        {
            var resolvedNotificationsViewModel = App.Services.GetRequiredService<NotificationsViewModel>();
            ContentFrame.Navigate(typeof(NotificationsPage), resolvedNotificationsViewModel);
            ViewModel.SelectedPageName = "Notifications";
        }
    }
}
