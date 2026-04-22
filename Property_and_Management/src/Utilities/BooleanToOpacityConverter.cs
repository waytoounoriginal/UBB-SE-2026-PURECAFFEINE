using System;
using Microsoft.UI.Xaml.Data;

namespace Property_and_Management.Src.Utilities
{
    public class BooleanToOpacityConverter : IValueConverter
    {
        private const double ExpiredItemOpacity = 0.5;
        private const double ActiveItemOpacity = 1.0;

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool isExpired && isExpired)
            {
                return ExpiredItemOpacity;
            }

            return ActiveItemOpacity;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
