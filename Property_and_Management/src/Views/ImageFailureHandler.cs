using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Property_and_Management.Src.Views
{
    internal static class ImageFailureHandler
    {
        private const string DefaultGameImageKey = "DefaultGameImage";
        private const string DefaultGameImageAssetUri = "ms-appx:///Assets/default-game-placeholder.jpg";
        private const string DefaultGameImageAssetSuffix = "/Assets/default-game-placeholder.jpg";

        public static void HandleFailure(Image failedImage, ResourceDictionary pageResources)
        {
            if (failedImage == null)
            {
                return;
            }

            if (failedImage.Source is BitmapImage current &&
                current.UriSource != null &&
                current.UriSource.AbsoluteUri.EndsWith(DefaultGameImageAssetSuffix, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (pageResources != null &&
                pageResources.TryGetValue(DefaultGameImageKey, out var localResource) &&
                localResource is BitmapImage localImage)
            {
                failedImage.Source = localImage;
                return;
            }

            if (Application.Current.Resources.TryGetValue(DefaultGameImageKey, out var appResource) &&
                appResource is BitmapImage appImage)
            {
                failedImage.Source = appImage;
                return;
            }

            failedImage.Source = new BitmapImage(new Uri(DefaultGameImageAssetUri));
        }
    }
}