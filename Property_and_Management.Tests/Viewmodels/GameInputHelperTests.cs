using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Property_and_Management.Src.Viewmodels;

namespace Property_and_Management.Tests.Viewmodels
{
    [TestFixture]
    public sealed class GameInputHelperTests
    {
        [Test]
        public void BuildValidationErrors_WithAllValidInputs_ReturnsEmptyErrorList()
        {

            var testGameName = "Catan";
            var testGamePrice = 19.99m;
            var minimumPlayerCount = 3;
            var maximumPlayerCount = 4;
            var gameDescription = "Colonize the island";
            var minimumNameLength = 3;
            var maximumNameLength = 50;
            var minimumAllowedPrice = 0.01m;
            var absoluteMinimumPlayerCount = 2;
            var minimumDescriptionLength = 10;
            var maximumDescriptionLength = 200;
            
            
            var validationErrors = GameInputHelper.BuildValidationErrors(
                testGameName,
                testGamePrice,
                minimumPlayerCount,
                maximumPlayerCount,
                gameDescription,
                minimumNameLength,
                maximumNameLength,
                minimumAllowedPrice,
                absoluteMinimumPlayerCount,
                minimumDescriptionLength,
                maximumDescriptionLength);

           
            Assert.That(validationErrors, Is.Empty);
        }

        [Test]
        public void BuildValidationErrors_WithLowPriceAndShortDescription_ReturnsPriceAndDescriptionErrors()
        {

            var testGameName = "Saboteur";
            var testGamePrice = 2.0m;
            var minimumPlayerCount = 2;
            var maximumPlayerCount = 12;
            var gameDescription = "Find the gold";
            var minimumNameLength = 3;
            var maximumNameLength = 50;
            var minimumAllowedPrice = 20.01m;
            var absoluteMinimumPlayerCount = 2;
            var minimumDescriptionLength = 100;
            var maximumDescriptionLength = 200;

           
            var validationErrors = GameInputHelper.BuildValidationErrors(
                testGameName,
                testGamePrice,
                minimumPlayerCount,
                maximumPlayerCount,
                gameDescription,
                minimumNameLength,
                maximumNameLength,
                minimumAllowedPrice,
                absoluteMinimumPlayerCount,
                minimumDescriptionLength,
                maximumDescriptionLength);

            
            Assert.That(validationErrors, Does.Contain(Constants.ValidationMessages.PriceMinimum(minimumAllowedPrice)));
            Assert.That(validationErrors, Does.Contain(Constants.ValidationMessages.DescriptionLengthRange(minimumDescriptionLength, maximumDescriptionLength)));
        }

        [Test]
        public void BuildValidationErrors_WithEmptyNameAndInvalidPlayerCounts_ReturnsNameAndPlayerCountErrors()
        {
            

            var testGameName = "";
            var testGamePrice = 30.0m;
            var minimumPlayerCount = 11;
            var maximumPlayerCount = 10;
            var gameDescription = "Find the gold";
            var minimumNameLength = 3;
            var maximumNameLength = 50;
            var minimumAllowedPrice = 20.01m;
            var absoluteMinimumPlayerCount = 20;
            var minimumDescriptionLength = 1;
            var maximumDescriptionLength = 200;

            
            var validationErrors = GameInputHelper.BuildValidationErrors(
                testGameName,
                testGamePrice,
                minimumPlayerCount,
                maximumPlayerCount,
                gameDescription,
                minimumNameLength,
                maximumNameLength,
                minimumAllowedPrice,
                absoluteMinimumPlayerCount,
                minimumDescriptionLength,
                maximumDescriptionLength);

            
            Assert.That(validationErrors, Does.Contain(Constants.ValidationMessages.NameLengthRange(minimumNameLength, maximumNameLength)));
            Assert.That(validationErrors, Does.Contain(Constants.ValidationMessages.MinimumPlayerCount(absoluteMinimumPlayerCount)));
            Assert.That(validationErrors, Does.Contain(Constants.ValidationMessages.MaximumPlayerCountComparedToMinimum));
        }

        [Test]
        public void EnsureImageOrDefault_WithValidImage_ReturnsSameImage()
        {
            var gameImage = new byte[] { 1, 2, 3 };
            var baseDir = "SomeDirectory";

            var result = GameInputHelper.EnsureImageOrDefault(gameImage, baseDir);

            Assert.That(result, Is.SameAs(gameImage));
        }

        [Test]
        public void EnsureImageOrDefault_WithEmptyImageAndInvalidPath_ReturnsEmptyArray()
        {
            var gameImage = Array.Empty<byte>();
            var baseDir = "InvalidDirectory123456789";

            var returnedImage = GameInputHelper.EnsureImageOrDefault(gameImage, baseDir);

            Assert.That(returnedImage, Is.Empty);
        }
    }
}
