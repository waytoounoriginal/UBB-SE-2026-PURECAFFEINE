using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Property_and_Management.Src.Utilities
{
    public class ByteArrayToImageConverter : IValueConverter
    {
        private const int EmptyByteArrayLength = 0;

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is byte[] imageBytes && imageBytes.Length > EmptyByteArrayLength)
            {
                try
                {
                    var imageByteStream = new MemoryStream(imageBytes);
                    var gameImageBitmap = new BitmapImage();
                    gameImageBitmap.SetSource(imageByteStream.AsRandomAccessStream());
                    return gameImageBitmap;
                }
                catch
                {
                }
            }

            return new BitmapImage(new Uri("ms-appx:///Assets/default-game-placeholder.jpg"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}