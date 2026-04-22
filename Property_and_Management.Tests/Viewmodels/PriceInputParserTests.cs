using NUnit.Framework;
using Property_and_Management.Src.Viewmodels;

namespace Property_and_Management.Tests.Viewmodels
{
    [TestFixture]
    public sealed class PriceInputParserTests
    {
        [Test]
        public void TryParsePriceInput_NullString_ReturnsFalseAndZero()
        {
            var priceParsingSucceeded = PriceInputParser.TryParsePriceInput(null!, out var price);
            Assert.That(priceParsingSucceeded, Is.False);
            Assert.That(price, Is.EqualTo(0));
        }

        [Test]
        public void TryParsePriceInput_OnlyWhitespace_ReturnsFalseAndZero()
        {
            var input = "   ";
            var priceParsingSucceeded = PriceInputParser.TryParsePriceInput(input, out var price);
            Assert.That(priceParsingSucceeded, Is.False);
            Assert.That(price, Is.EqualTo(0));
        }

        [Test]
        public void TryParsePriceInput_WholeNumber_ParsesCorrectly()
        {
            var input = "42";
            var priceParsingSucceeded = PriceInputParser.TryParsePriceInput(input, out var price);
            Assert.That(priceParsingSucceeded, Is.True);
            Assert.That(price, Is.EqualTo(42));
        }

        [Test]
        public void TryParsePriceInput_DotDecimalSeparator_ParsesCorrectly()
        {
            var input = "12.50";
            var priceParsingSucceeded = PriceInputParser.TryParsePriceInput(input, out var price);
            Assert.That(priceParsingSucceeded, Is.True);
            Assert.That(price, Is.EqualTo(12.5));
        }

        [Test]
        public void TryParsePriceInput_NonNumericText_ReturnsFalseAndZero()
        {
            var input = "banana";
            var priceParsingSucceeded = PriceInputParser.TryParsePriceInput(input, out var price);
            Assert.That(priceParsingSucceeded, Is.False);
            Assert.That(price, Is.EqualTo(0));
        }
    }
}
