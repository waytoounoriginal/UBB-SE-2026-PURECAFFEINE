using FluentAssertions;
using Moq;
using NUnit.Framework;
using Property_and_Management.Src.DataTransferObjects;
using Property_and_Management.Src.Interface;
using Property_and_Management.Src.Viewmodels;

namespace Property_and_Management.Tests.Viewmodels
{
    [TestFixture]
    public sealed class EditGameViewModelTests
    {
        private const int SampleGameIdentifier = 42;
        private const int SampleOwnerIdentifier = 1;

        private Mock<IGameService> gameServiceMock = null!;
        private EditGameViewModel viewModel = null!;

        [SetUp]
        public void SetUp()
        {
            gameServiceMock = new Mock<IGameService>();
            viewModel = new EditGameViewModel(gameServiceMock.Object);
        }

        [Test]
        public void LoadGame_PopulatesPropertiesFromService()
        {
            var existingGame = new GameDTO
            {
                Id = SampleGameIdentifier,
                Owner = new UserDTO { Id = SampleOwnerIdentifier },
                Name = "Existing Game",
                Price = 15m,
                MinimumPlayerNumber = 2,
                MaximumPlayerNumber = 5,
                Description = "A vewy very big long description for validation enough long.",
                IsActive = true,
            };
            gameServiceMock
                .Setup(service => service.GetGameByIdentifier(SampleGameIdentifier))
                .Returns(existingGame);

            viewModel.LoadGame(SampleGameIdentifier);

            viewModel.EditedGameId.Should().Be(SampleGameIdentifier);
            viewModel.GameName.Should().Be("Existing Game");
        }

        [Test]
        public void UpdateGame_ValidInputs_CallsUpdateWithCorrectIdentifier()
        {
            gameServiceMock
                .Setup(service => service.GetGameByIdentifier(SampleGameIdentifier))
                .Returns(new GameDTO
                {
                    Id = SampleGameIdentifier,
                    Owner = new UserDTO { Id = SampleOwnerIdentifier },
                    Name = "Valid Name",
                    Price = 10m,
                    MinimumPlayerNumber = 2,
                    MaximumPlayerNumber = 4,
                    Description = "This is the decritption that is long enough to pass validation",
                    IsActive = true,
                });
            viewModel.LoadGame(SampleGameIdentifier);

            viewModel.UpdateGame();

            gameServiceMock.Verify(
                service => service.UpdateGameByIdentifier(
                    SampleGameIdentifier, It.IsAny<GameDTO>()),
                Times.Once);
        }
    }
}
