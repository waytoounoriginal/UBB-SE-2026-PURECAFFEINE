using System;
using System.Globalization;

namespace Property_and_Management.Src.Viewmodels
{
    internal static class PriceInputParser
    {
        private const double ZeroPriceAsParseDefault = 0;

        public static bool TryParsePriceInput(string rawPriceInput, out double parsedPriceAsDouble)
        {
            parsedPriceAsDouble = ZeroPriceAsParseDefault;
            var trimmedPriceText = rawPriceInput?.Trim();

            if (string.IsNullOrWhiteSpace(trimmedPriceText))
            {
                return false;
            }

            return double.TryParse(trimmedPriceText, NumberStyles.Float, CultureInfo.CurrentCulture, out parsedPriceAsDouble) ||
                   double.TryParse(trimmedPriceText, NumberStyles.Float, CultureInfo.InvariantCulture, out parsedPriceAsDouble);
        }
    }
}