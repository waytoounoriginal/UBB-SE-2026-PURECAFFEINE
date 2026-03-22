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
        private NotificationService _notificationService;

        public int CurrentUserId { get; private set; }

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
            var list = _notificationService.GetNotificationsForUser(userId);
            Notifications = new ObservableCollection<NotificationDTO>(list);
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
            LoadNotificationsForUser(1);
        }
    }
}
