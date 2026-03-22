using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls.Primitives;
using Property_and_Management.src.DTO;
using Property_and_Management.src.Service;
using ServerCommunication;
using Windows.ApplicationModel.VoiceCommands;

namespace Property_and_Management.src.Viewmodels
{
    public class NotificationsViewModel : INotifyPropertyChanged, IObserver<NotificationDTO>
    {
        private ObservableCollection<NotificationDTO> _notifications = new ObservableCollection<NotificationDTO>();
        private ObservableCollection<NotificationDTO> _pagedNotifications = new ObservableCollection<NotificationDTO>();
        private NotificationService _notificationService;

        private ImmutableList<NotificationDTO> _allNotifications = ImmutableList<NotificationDTO>.Empty;

        public int CurrentUserId { get; private set; }

        private const int PageSizeConst = 4;
        public int PageSize => PageSizeConst;

        private int _currentPage = 1;
        public int CurrentPage
        {
            get => _currentPage;
            private set
            {
                if (_currentPage != value)
                {
                    _currentPage = value;
                    OnProperyChanged();
                    UpdatePaging();
                }
            }
        }

        public int TotalCount => _allNotifications?.Count ?? 0;

        public int PageCount => Math.Max(1, (int)Math.Ceiling((double)TotalCount / PageSize));

        public int DisplayedCount => PagedNotifications?.Count ?? 0;

        public ObservableCollection<NotificationDTO> Notifications
        {
            get => _notifications;
            set
            {
                if (_notifications != value)
                {
                    _notifications = value;
                    OnProperyChanged();
                }
            }
        }

        public ObservableCollection<NotificationDTO> PagedNotifications
        {
            get => _pagedNotifications;
            private set
            {
                if (_pagedNotifications != value)
                {
                    _pagedNotifications = value;
                    OnProperyChanged(nameof(PagedNotifications));
                    OnProperyChanged(nameof(DisplayedCount));
                    OnProperyChanged(nameof(TotalCount));
                    OnProperyChanged(nameof(PageCount));
                    OnProperyChanged(nameof(CurrentPage));
                    OnProperyChanged(nameof(ShowingText));
                }
            }
        }

        public string ShowingText => $"Showing {DisplayedCount} of {TotalCount}";

        public NotificationsViewModel(NotificationService notificationService)
        {
            _notificationService = notificationService;

            // Default user
            LoadNotificationsForUser(1);

            notificationService.Subscribe(this);
        }

        public void LoadNotificationsForUser(int userId)
        {
            CurrentUserId = userId;
            _allNotifications = _notificationService.GetNotificationsForUser(userId);

            Notifications = new ObservableCollection<NotificationDTO>(_allNotifications);

            // reset paging
            _currentPage = 1;
            UpdatePaging();

            OnProperyChanged(nameof(TotalCount));
            OnProperyChanged(nameof(PageCount));
            OnProperyChanged(nameof(ShowingText));
        }

        // small internal wrapper to convert repository results -> DTO list (keeps call sites short)
        private System.Collections.Immutable.ImmutableList<NotificationDTO> _notification_service_wrapper()
        {
            return _notificationService.GetNotificationsForUser(CurrentUserId);
        }

        private void UpdatePaging()
        {
            if (_allNotifications == null) _allNotifications = ImmutableList<NotificationDTO>.Empty;

            if (CurrentPage < 1) _currentPage = 1;
            if (CurrentPage > PageCount) _currentPage = PageCount;

            var skip = (CurrentPage - 1) * PageSize;
            var pageItems = _allNotifications.Skip(skip).Take(PageSize).ToList();
            PagedNotifications = new ObservableCollection<NotificationDTO>(pageItems);

            OnProperyChanged(nameof(DisplayedCount));
            OnProperyChanged(nameof(ShowingText));
        }

        public void NextPage()
        {
            if (CurrentPage < PageCount)
            {
                CurrentPage++;
            }
        }

        public void PrevPage()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
            }
        }

        public void DeleteNotificationById(int id)
        {
            // call service to delete and then refresh list for current user
            _notificationService.DeleteNotificationById(id);
            // reload all notifications for user
            _allNotifications = _notification_service_wrapper();
            Notifications = new ObservableCollection<NotificationDTO>(_allNotifications);

            // ensure current page is valid
            if (CurrentPage > PageCount) _currentPage = PageCount;
            UpdatePaging();

            OnProperyChanged(nameof(TotalCount));
            OnProperyChanged(nameof(PageCount));
            OnProperyChanged(nameof(ShowingText));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnProperyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void OnCompleted()
        {
            Console.WriteLine("Notification observable completed");
        }

        public void OnError(Exception error)
        {
            Console.WriteLine($"Notification observable error: {error.Message}");
        }

        public void OnNext(NotificationDTO value)
        {
            // Trigger an update from the service
            LoadNotificationsForUser(CurrentUserId == 0 ? 1 : CurrentUserId);
        }
    }
}
