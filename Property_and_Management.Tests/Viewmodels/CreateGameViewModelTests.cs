using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Property_and_Management.Src.DataTransferObjects;
using Property_and_Management.Src.Interface;
using Property_and_Management.Src.Viewmodels;

namespace Property_and_Management.Tests.Viewmodels
{
    [TestFixture]
    public sealed class CreateGameViewModelTests
    {
        private const int TestUserId = 42;
        private const string ValidGameName = "Settlers of Catan";
        private const decimal ValidPrice = 15.99m;
        private const int ValidMinPlayers = 2;
        private const int ValidMaxPlayers = 6;
        private const string ValidDescription = "A classic resource-trading board game for families.";

        private Mock<IGameService> mockGameService = null!;
        private Mock<ICurrentUserContext> mockUserContext = null!;
        private CreateGameViewModel viewModel = null!;

        [SetUp]
        public void SetUp()
        {
            mockGameService = new Mock<IGameService>();
            mockUserContext = new Mock<ICurrentUserContext>();
            mockUserContext.SetupGet(ctx => ctx.CurrentUserId).Returns(TestUserId);

            viewModel = new CreateGameViewModel(mockGameService.Object, mockUserContext.Object);
        }


        [Test]
        public void CurrentUserId_ReturnsValueFromUserContext()
        {
            Assert.That(viewModel.CurrentUserId, Is.EqualTo(TestUserId));
        }


        [Test]
        public void ValidateGameInputs_AllFieldsValid_ReturnsNoErrors()
        {
            PopulateWithValidInputs();

            List<string> errors = viewModel.ValidateGameInputs();

            Assert.That(errors, Is.Empty);
        }

        [Test]
        public void ValidateGameInputs_NameTooShort_ReturnsNameError()
        {
            PopulateWithValidInputs();
            viewModel.GameName = "AB";

            List<string> errors = viewModel.ValidateGameInputs();

            Assert.That(errors, Has.Count.GreaterThanOrEqualTo(1));
            Assert.That(errors, Has.Some.Contain("Name"));
        }

        [Test]
        public void ValidateGameInputs_NameIsEmpty_ReturnsNameError()
        {
            PopulateWithValidInputs();
            viewModel.GameName = string.Empty;

            List<string> errors = viewModel.ValidateGameInputs();

            Assert.That(errors, Has.Some.Contain("Name"));
        }

        [Test]
        public void ValidateGameInputs_PriceBelowMinimum_ReturnsPriceError()
        {
            PopulateWithValidInputs();
            viewModel.GamePrice = 0m;

            List<string> errors = viewModel.ValidateGameInputs();

            Assert.That(errors, Has.Some.Contain("Price"));
        }

        [Test]
        public void ValidateGameInputs_MinPlayersZero_ReturnsPlayerCountError()
        {
            PopulateWithValidInputs();
            viewModel.MinimumPlayersRequired = 0;

            List<string> errors = viewModel.ValidateGameInputs();

            Assert.That(errors, Has.Some.Contain("player"));
        }

        [Test]
        public void ValidateGameInputs_MaxPlayersLessThanMin_ReturnsMaxPlayerError()
        {
            PopulateWithValidInputs();
            viewModel.MinimumPlayersRequired = 5;
            viewModel.MaximumPlayersAllowed = 2;

            List<string> errors = viewModel.ValidateGameInputs();

            Assert.That(errors, Has.Some.Contain("Maximum"));
        }

        [Test]
        public void ValidateGameInputs_DescriptionTooShort_ReturnsDescriptionError()
        {
            PopulateWithValidInputs();
            viewModel.GameDescription = "Short";

            List<string> errors = viewModel.ValidateGameInputs();

            Assert.That(errors, Has.Some.Contain("Description"));
        }

        [Test]
        public void ValidateGameInputs_MultipleInvalidFields_ReturnsMultipleErrors()
        {
            viewModel.GameName = "";
            viewModel.GamePrice = 0m;
            viewModel.GameDescription = "";

            List<string> errors = viewModel.ValidateGameInputs();

            Assert.That(errors.Count, Is.GreaterThanOrEqualTo(3));
        }

        [Test]
        public void SetGamePriceFromText_ValidNumericString_SetsGamePrice()
        {
            viewModel.SetGamePriceFromText("25.50");

            Assert.That(viewModel.GamePrice, Is.EqualTo(25.50m));
        }

        [Test]
        public void SetGamePriceFromText_EmptyString_SetsGamePriceToZero()
        {
            viewModel.GamePrice = 10m;

            viewModel.SetGamePriceFromText("");

            Assert.That(viewModel.GamePrice, Is.EqualTo(0m));
        }

        [Test]
        public void SetGamePriceFromText_NonNumericString_SetsGamePriceToZero()
        {
            viewModel.GamePrice = 10m;

            viewModel.SetGamePriceFromText("not-a-price");

            Assert.That(viewModel.GamePrice, Is.EqualTo(0m));
        }

        [Test]
        public void GamePriceAsDouble_RoundTrips_WithDecimalGamePrice()
        {
            viewModel.GamePriceAsDouble = 19.99;

            Assert.That(viewModel.GamePrice, Is.EqualTo(19.99m));
            Assert.That(viewModel.GamePriceAsDouble, Is.EqualTo(19.99).Within(0.001));
        }


        [Test]
        public void SubmitCreateGame_ValidInputs_ReturnsSuccessResult()
        {
            PopulateWithValidInputs();

            ViewOperationResult result = viewModel.SubmitCreateGame();

            Assert.That(result.IsSuccess, Is.True);
        }

        [Test]
        public void SubmitCreateGame_ValidInputs_InvokesServiceAddGame()
        {
            PopulateWithValidInputs();

            viewModel.SubmitCreateGame();

            mockGameService.Verify(svc => svc.AddGame(It.IsAny<GameDTO>()), Times.Once);
        }

        [Test]
        public void SubmitCreateGame_InvalidInputs_ReturnsFailureWithValidationTitle()
        {
            viewModel.GameName = "";

            ViewOperationResult result = viewModel.SubmitCreateGame();

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.DialogTitle, Is.EqualTo("Validation Error"));
        }

        [Test]
        public void SubmitCreateGame_InvalidInputs_DoesNotInvokeServiceAddGame()
        {
            viewModel.GameName = "";

            viewModel.SubmitCreateGame();

            mockGameService.Verify(svc => svc.AddGame(It.IsAny<GameDTO>()), Times.Never);
        }

        [Test]
        public void SaveGame_ValidInputs_ReturnsGameDtoWithCorrectOwner()
        {
            PopulateWithValidInputs();

            GameDTO savedGame = viewModel.SaveGame();

            Assert.That(savedGame, Is.Not.Null);
            Assert.That(savedGame.Owner.Id, Is.EqualTo(TestUserId));
        }

        [Test]
        public void SaveGame_ValidInputs_ReturnedDtoCarriesInputValues()
        {
            PopulateWithValidInputs();

            GameDTO savedGame = viewModel.SaveGame();

            Assert.That(savedGame.Name, Is.EqualTo(ValidGameName));
            Assert.That(savedGame.Price, Is.EqualTo(ValidPrice));
            Assert.That(savedGame.MinimumPlayerNumber, Is.EqualTo(ValidMinPlayers));
            Assert.That(savedGame.MaximumPlayerNumber, Is.EqualTo(ValidMaxPlayers));
        }

        [Test]
        public void SaveGame_InvalidInputs_ReturnsNull()
        {
            viewModel.GameName = "";

            GameDTO savedGame = viewModel.SaveGame();

            Assert.That(savedGame, Is.Null);
        }

        [Test]
        public void SaveGame_InvalidInputs_DoesNotCallService()
        {
            viewModel.GameName = "";

            viewModel.SaveGame();

            mockGameService.Verify(svc => svc.AddGame(It.IsAny<GameDTO>()), Times.Never);
        }

        [Test]
        public void Constructor_DefaultValues_AreSetCorrectly()
        {
            Assert.That(viewModel.GameName, Is.EqualTo(string.Empty));
            Assert.That(viewModel.GameDescription, Is.EqualTo(string.Empty));
            Assert.That(viewModel.IsGameActive, Is.True);
            Assert.That(viewModel.GameImage, Is.Null);
        }

        private void PopulateWithValidInputs()
        {
            viewModel.GameName = ValidGameName;
            viewModel.GamePrice = ValidPrice;
            viewModel.MinimumPlayersRequired = ValidMinPlayers;
            viewModel.MaximumPlayersAllowed = ValidMaxPlayers;
            viewModel.GameDescription = ValidDescription;
        }
    }
}
