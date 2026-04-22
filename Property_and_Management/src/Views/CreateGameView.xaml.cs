using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Property_and_Management.Src.Viewmodels;

namespace Property_and_Management.Src.Views
{
    public sealed partial class CreateGameView : Page
    {
        public CreateGameViewModel ViewModel { get; }

        public CreateGameView()
        {
            this.InitializeComponent();

            ViewModel = App.Services.GetRequiredService<CreateGameViewModel>();
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            ViewModel.SetGamePriceFromText(PriceNumberBox.Text);

            var gameCreateResult = ViewModel.SubmitCreateGame();
            if (gameCreateResult.IsSuccess)
            {
                if (Frame.CanGoBack)
                {
                    Frame.GoBack();
                }
                return;
            }

            await DialogHelper.ShowMessageAsync(
                this.XamlRoot,
                gameCreateResult.DialogTitle,
                gameCreateResult.DialogMessage);
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