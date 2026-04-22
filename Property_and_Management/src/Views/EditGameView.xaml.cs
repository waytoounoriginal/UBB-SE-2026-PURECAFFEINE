using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Property_and_Management.Src.Viewmodels;

namespace Property_and_Management.Src.Views
{
    public sealed partial class EditGameView : Page
    {
        private const int EmptyImageLength = 0;

        public EditGameViewModel ViewModel { get; }

        public EditGameView()
        {
            this.InitializeComponent();

            ViewModel = App.Services.GetRequiredService<EditGameViewModel>();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs navigationEventArgs)
        {
            base.OnNavigatedTo(navigationEventArgs);

            if (navigationEventArgs.Parameter is int incomingGameId)
            {
                ViewModel.LoadGame(incomingGameId);
                this.Bindings.Update();
            }

            if (ViewModel.GameImage != null && ViewModel.GameImage.Length > EmptyImageLength)
            {
                using (var existingImageMemoryStream = new System.IO.MemoryStream(ViewModel.GameImage))
                {
                    var existingImageStream = existingImageMemoryStream.AsRandomAccessStream();
                    var existingGameImageBitmap = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage();
                    await existingGameImageBitmap.SetSourceAsync(existingImageStream);
                    ImagePreview.Source = existingGameImageBitmap;
                }
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            ViewModel.SetGamePriceFromText(PriceNumberBox.Text);

            var gameUpdateResult = ViewModel.SubmitGameUpdate();
            if (gameUpdateResult.IsSuccess)
            {
                if (Frame.CanGoBack)
                {
                    Frame.GoBack();
                }
                return;
            }

            await DialogHelper.ShowMessageAsync(
                this.XamlRoot,
                gameUpdateResult.DialogTitle,
                gameUpdateResult.DialogMessage);
        }

        private async void UploadImageButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            var imageFilePicker = new Windows.Storage.Pickers.FileOpenPicker();
            imageFilePicker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            imageFilePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            imageFilePicker.FileTypeFilter.Add(".jpg");
            imageFilePicker.FileTypeFilter.Add(".jpeg");
            imageFilePicker.FileTypeFilter.Add(".png");

            var mainWindowHandle = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(imageFilePicker, mainWindowHandle);

            var selectedImageFile = await imageFilePicker.PickSingleFileAsync();
            if (selectedImageFile == null)
            {
                return;
            }

            FileNameTextBlock.Text = selectedImageFile.Name;

            using (var fileReadStream = await selectedImageFile.OpenStreamForReadAsync())
            {
                using (var imageMemoryStream = new System.IO.MemoryStream())
                {
                    await fileReadStream.CopyToAsync(imageMemoryStream);
                    ViewModel.GameImage = imageMemoryStream.ToArray();
                }
            }

            var previewBitmapImage = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage();
            using (var imageRandomAccessStream = await selectedImageFile.OpenAsync(Windows.Storage.FileAccessMode.Read))
            {
                await previewBitmapImage.SetSourceAsync(imageRandomAccessStream);
            }
            ImagePreview.Source = previewBitmapImage;
        }
    }
}