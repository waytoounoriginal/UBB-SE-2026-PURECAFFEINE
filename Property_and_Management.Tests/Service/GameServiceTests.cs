using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Property_and_Management.Src.DataTransferObjects;
using Property_and_Management.Src.Interface;
using Property_and_Management.Src.Model;
using Property_and_Management.Src.Service;


namespace Property_and_Management.Tests.Service
{
    [TestFixture]
    public sealed class GameServiceTests
    {

        private const int SampleGameIdentifier = 42;
        private const int SampleOwnerIdentifier = 1;

        private Mock<IGameRepository> gameRepositoryMock = null!;
        private Mock<IRentalRepository> rentalRepositoryMock = null!;
        private Mock<IMapper<Game, GameDTO>> gameMapperMock = null!;
        private Mock<IRequestService> requestServiceMock = null!;
        private GameService gameService = null!;

        [SetUp]
        public void SetUp()
        {
            gameRepositoryMock = new Mock<IGameRepository>();
            rentalRepositoryMock = new Mock<IRentalRepository>();
            gameMapperMock = new Mock<IMapper<Game, GameDTO>>();
            requestServiceMock = new Mock<IRequestService>();

            rentalRepositoryMock
                .Setup(repository => repository.GetRentalsByGame(It.IsAny<int>()))
                .Returns(ImmutableList<Rental>.Empty);
            gameMapperMock
                .Setup(mapper => mapper.ToDTO(It.IsAny<Game>()))
                .Returns(new GameDTO { Id = SampleGameIdentifier });
            gameMapperMock
                .Setup(mapper => mapper.ToModel(It.IsAny<GameDTO>()))
                .Returns(new Game { Id = SampleGameIdentifier });

            gameService = new GameService(
                gameRepositoryMock.Object,
                rentalRepositoryMock.Object,
                gameMapperMock.Object,
                requestServiceMock.Object);
        }
        [Test]
        public void DeleteGameByIdentifier_WithOneActiveRental_ThrowsInvalidOperationException()
        {
            var activeRental = new Rental(
                1,
                rentedGame: new Game { Id = SampleGameIdentifier },
                renterUser: new User(2, "Madi"),
                ownerUser: new User(SampleOwnerIdentifier, "Beatrice"),
                startDate: DateTime.Now.AddDays(-1),
                endDate: DateTime.Now.AddDays(3));

            rentalRepositoryMock
                .Setup(repository => repo.GetRentalsByGame(SampleGameIdentifier))
                .Returns(ImmutableList.Create(activeRental));

            Action deleteAction = () => gameService.DeleteGameByIdentifier(SampleGameIdentifier);

            deleteAction.Should()
                .Throw<InvalidOperationException>()
                .WithMessage("*1 active rental*");
        }

        [Test]
        public void AddGame_WithValidDto_CallsRepositoryAddOnce()
        {
            var gameDto = new GameDTO { Id = SampleGameIdentifier };

            gameService.AddGame(gameDto);

            gameRepositoryMock.Verify(repository => repository.Add(It.IsAny<Game>()), Times.Once);
        }

        [Test]
        public void DeleteGameByIdentifier_WithMultipleActiveRentals_ExceptionMessageContainsRentalCount()
        {
            var rentalA = new Rental(
                1,
                rentedGame: new Game { Id = SampleGameIdentifier },
                renterUser: new User(2, "Madi"),
                ownerUser: new User(SampleOwnerIdentifier, "Beatrice"),
                startDate: DateTime.Now.AddDays(-1),
                endDate: DateTime.Now.AddDays(3));


            var rentalB = new Rental(
                2,
                rentedGame: new Game { Id = SampleGameIdentifier },
                renterUser: new User(2, "Madi"),
                ownerUser: new User(SampleOwnerIdentifier, "Beatrice"),
                startDate: DateTime.Now.AddDays(4),
                endDate: DateTime.Now.AddDays(6));

            rentalRepositoryMock
                .Setup(repository => repository.GetRentalsByGame(SampleGameIdentifier))
                .Returns(ImmutableList.Create(rentalA, rentalB));

            Action deleteAction = () => gameService.DeleteGameByIdentifier(SampleGameIdentifier);

            deleteAction.Should()
                .Throw<InvalidOperationException>()
                .WithMessage("*2 active rentals*");
        }

        [Test]
        public void GetGameByIdentifier_WithValidId_ReturnsGameDto()
        {
            gameRepositoryMock
                .Setup(repository => repository.Get(SampleGameIdentifier))
                .Returns(new Game { Id = SampleGameIdentifier });

            var retrievedGameDto = gameService.GetGameByIdentifier(SampleGameIdentifier);

            retrievedGameDto.Id.Should().Be(SampleGameIdentifier);
        }

        [Test]
        public void UpdateGameByIdentifier_WithValidDto_CallsRepositoryUpdateWithCorrectId()
        {
            var gameDto = new GameDTO { Id = SampleGameIdentifier };

            gameService.UpdateGameByIdentifier(SampleGameIdentifier, gameDto);

            gameRepositoryMock.Verify(
                repository => repository.Update(SampleGameIdentifier, It.IsAny<Game>()),
                Times.Once);
        }

        [Test]
        public void DeleteGameByIdentifier_WithNoActiveRentals_DeletesGameAndNotifiesRequestService()
        {
            rentalRepositoryMock
                .Setup(repository => repository.GetRentalsByGame(SampleGameIdentifier))
                .Returns(ImmutableList<Rental>.Empty);

            gameRepositoryMock
                .Setup(repository => repository.Delete(SampleGameIdentifier))
                .Returns(new Game { Id = SampleGameIdentifier });

            gameService.DeleteGameByIdentifier(SampleGameIdentifier);

            requestServiceMock.Verify(
                requestService => requestService.OnGameDeactivated(SampleGameIdentifier),
                Times.Once);

            gameRepositoryMock.Verify(
                repository => repository.Delete(SampleGameIdentifier),
                Times.Once);
        }
    }
}
