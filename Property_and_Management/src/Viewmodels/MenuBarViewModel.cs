using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Property_and_Management.Src.Viewmodels
{
    public class MenuBarViewModel : INotifyPropertyChanged
    {
        public event Action<AppPage> RequestNavigation;

        public Dictionary<string, Action> NavigationActionsByMenuLabel { get; }

        private string selectedMenuPageName;

        public MenuBarViewModel()
        {
            NavigationActionsByMenuLabel = new Dictionary<string, Action>
            {
                { "My Games",           () => RequestNavigation?.Invoke(AppPage.Listings) },
                { "Others' Requests",   () => RequestNavigation?.Invoke(AppPage.RequestsFromOthers) },
                { "Others' Rentals",    () => RequestNavigation?.Invoke(AppPage.RentalsToOthers) },
                { "My Requests",        () => RequestNavigation?.Invoke(AppPage.RequestsToOthers) },
                { "My Rentals",         () => RequestNavigation?.Invoke(AppPage.RentalsFromOthers) },
                { "Notifications",      () => RequestNavigation?.Invoke(AppPage.Notifications) }
            };
        }

        public string SelectedPageName
        {
            get => selectedMenuPageName;
            set
            {
                if (selectedMenuPageName != value)
                {
                    selectedMenuPageName = value;
                    OnPropertyChanged();
                    HandleMenuNavigation(value);
                }
            }
        }

        private void HandleMenuNavigation(string selectedMenuLabel)
        {
            OnPropertyChanged();
            if (!string.IsNullOrEmpty(selectedMenuLabel) && NavigationActionsByMenuLabel.TryGetValue(selectedMenuLabel, out var navigationAction))
            {
                navigationAction.Invoke();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}