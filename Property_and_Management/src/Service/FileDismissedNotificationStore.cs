using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Property_and_Management.Src.Interface;

namespace Property_and_Management.Src.Service
{
    public class FileDismissedNotificationStore : IDismissedNotificationStore
    {
        private const int InvalidNotificationId = -1;
        private const int MinimumValidNotificationId = 0;
        private const string ApplicationFolderName = "BoardRent";
        private const string DismissedFilePrefix = "dismissed-notifications-user-";
        private const string DismissedFileSuffix = ".txt";
        private const char TokenSeparator = ',';

        public HashSet<int> Load(int ownerUserId)
        {
            var storageFilePath = GetStoragePath(ownerUserId);
            if (!File.Exists(storageFilePath))
            {
                return new HashSet<int>();
            }

            var serializedContent = File.ReadAllText(storageFilePath);
            if (string.IsNullOrWhiteSpace(serializedContent))
            {
                return new HashSet<int>();
            }

            return serializedContent
                .Split(TokenSeparator, StringSplitOptions.RemoveEmptyEntries)
                .Select(token => int.TryParse(token, out var parsedNotificationId) ? parsedNotificationId : InvalidNotificationId)
                .Where(parsedNotificationId => parsedNotificationId > MinimumValidNotificationId)
                .ToHashSet();
        }

        public void Save(int ownerUserId, IEnumerable<int> dismissedNotificationIdentifiers)
        {
            if (dismissedNotificationIdentifiers == null)
            {
                throw new ArgumentNullException(nameof(dismissedNotificationIdentifiers));
            }

            var storageFilePath = GetStoragePath(ownerUserId);
            var serializedContent = string.Join(TokenSeparator, dismissedNotificationIdentifiers.OrderBy(notificationId => notificationId));
            File.WriteAllText(storageFilePath, serializedContent);
        }

        private static string GetStoragePath(int ownerUserId)
        {
            var appDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                ApplicationFolderName);
            Directory.CreateDirectory(appDataFolder);
            return Path.Combine(appDataFolder, $"{DismissedFilePrefix}{ownerUserId}{DismissedFileSuffix}");
        }
    }
}
