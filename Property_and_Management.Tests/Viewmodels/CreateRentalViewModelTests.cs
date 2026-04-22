using System;
using System.Collections.Immutable;
using Moq;
using NUnit.Framework;
using Property_and_Management.Src.DataTransferObjects;
using Property_and_Management.Src.Interface;
using Property_and_Management.Src.Viewmodels;

namespace Property_and_Management.Tests.Viewmodels
{
    [TestFixture]
    public sealed class CreateRentalViewModelTests
    {
        private const int OwnerUserId = 10;
        private const int RenterUserId = 20;
        private const int GameId = 100;

        private Mock<IGameService> mockGameService = null!;
        private Mock<IRentalService> mockRentalService = null!;
        private Mock<IUserService> mockUserService = null!;
        private Mock<ICurrentUserContext> mockUserContext = null!;

        [SetUp]
        public void SetUp()
        {
            mockGameService = new Mock<IGameService>();
            mockRentalService = new Mock<IRentalService>();
            mockUserService = new Mock<IUserService>();
            mockUserContext = new Mock<ICurrentUserContext>();

            mockUserContext.SetupGet(ctx => ctx.CurrentUserId).Returns(OwnerUserId);

          
            mockGameService
                .Setup(svc => svc.GetGamesForOwner(OwnerUserId))
                .Returns(ImmutableList.Create(BuildActiveGame(GameId, OwnerUserId)));

            mockUserService
                .Setup(svc => svc.GetUsersExcept(OwnerUserId))
                .Returns(ImmutableList.Create(new UserDTO { Id = RenterUserId, DisplayName = "Renter" }));
        }

        private CreateRentalViewModel BuildViewModel()
        {
            return new CreateRentalViewModel(
                mockGameService.Object,
                mockRentalService.Object,
                mockUserService.Object,
                mockUserContext.Object);
        }


        [Test]
        public void Constructor_LoadsOwnedActiveGames()
        {
            var viewModel = BuildViewModel();

            Assert.That(viewModel.OwnedActiveGames, Has.Count.EqualTo(1));
            Assert.That(viewModel.OwnedActiveGames[0].Id, Is.EqualTo(GameId));
        }

        [Test]
        public void Constructor_ExcludesInactiveGamesFromList()
        {
            var inactiveGame = BuildActiveGame(200, OwnerUserId);
            inactiveGame.IsActive = false;

            mockGameService
                .Setup(svc => svc.GetGamesForOwner(OwnerUserId))
                .Returns(ImmutableList.Create(BuildActiveGame(GameId, OwnerUserId), inactiveGame));

            var viewModel = BuildViewModel();

            Assert.That(viewModel.OwnedActiveGames, Has.Count.EqualTo(1));
        }

        [Test]
        public void Constructor_LoadsAvailableRenters()
        {
            var viewModel = BuildViewModel();

            Assert.That(viewModel.AvailableRenters, Has.Count.EqualTo(1));
            Assert.That(viewModel.AvailableRenters[0].Id, Is.EqualTo(RenterUserId));
        }

        [Test]
        public void CurrentUserId_DelegatesToUserContext()
        {
            var viewModel = BuildViewModel();

            Assert.That(viewModel.CurrentUserId, Is.EqualTo(OwnerUserId));
        }



        [Test]
        public void LoadRentalFormData_RefreshesGamesAndRenters()
        {
            var viewModel = BuildViewModel();
            Assert.That(viewModel.OwnedActiveGames, Has.Count.EqualTo(1));

            mockGameService
                .Setup(svc => svc.GetGamesForOwner(OwnerUserId))
                .Returns(ImmutableList.Create(
                    BuildActiveGame(GameId, OwnerUserId),
                    BuildActiveGame(201, OwnerUserId)));

            viewModel.LoadRentalFormData();

            Assert.That(viewModel.OwnedActiveGames, Has.Count.EqualTo(2));
        }

  

        [Test]
        public void ValidateRentalInputs_AllFieldsValid_ReturnsTrue()
        {
            var viewModel = BuildViewModel();
            PopulateWithValidSelections(viewModel);

            Assert.That(viewModel.ValidateRentalInputs(), Is.True);
        }

        [Test]
        public void ValidateRentalInputs_NoGameSelected_ReturnsFalse()
        {
            var viewModel = BuildViewModel();
            PopulateWithValidSelections(viewModel);
            viewModel.SelectedGameToRent = null;

            Assert.That(viewModel.ValidateRentalInputs(), Is.False);
        }

        [Test]
        public void ValidateRentalInputs_NoRenterSelected_ReturnsFalse()
        {
            var viewModel = BuildViewModel();
            PopulateWithValidSelections(viewModel);
            viewModel.SelectedRenter = null;

            Assert.That(viewModel.ValidateRentalInputs(), Is.False);
        }

        [Test]
        public void ValidateRentalInputs_StartDateNull_ReturnsFalse()
        {
            var viewModel = BuildViewModel();
            PopulateWithValidSelections(viewModel);
            viewModel.StartDate = null;

            Assert.That(viewModel.ValidateRentalInputs(), Is.False);
        }

        [Test]
        public void ValidateRentalInputs_EndDateBeforeStartDate_ReturnsFalse()
        {
            var viewModel = BuildViewModel();
            PopulateWithValidSelections(viewModel);
            viewModel.EndDate = viewModel.StartDate.Value.AddDays(-1);

            Assert.That(viewModel.ValidateRentalInputs(), Is.False);
        }

        [Test]
        public void ValidateRentalInputs_StartDateInPast_ReturnsFalse()
        {
            var viewModel = BuildViewModel();
            PopulateWithValidSelections(viewModel);
            viewModel.StartDate = DateTimeOffset.Now.AddDays(-5);
            viewModel.EndDate = DateTimeOffset.Now.AddDays(-2);

            Assert.That(viewModel.ValidateRentalInputs(), Is.False);
        }


        [Test]
        public void CreateRental_ValidInputs_ReturnsSuccess()
        {
            var viewModel = BuildViewModel();
            PopulateWithValidSelections(viewModel);

            ViewOperationResult result = viewModel.CreateRental();

            Assert.That(result.IsSuccess, Is.True);
        }

        [Test]
        public void CreateRental_ValidInputs_CallsServiceWithCorrectArguments()
        {
            var viewModel = BuildViewModel();
            PopulateWithValidSelections(viewModel);

            viewModel.CreateRental();

            mockRentalService.Verify(svc => svc.CreateConfirmedRental(
                GameId,
                RenterUserId,
                OwnerUserId,
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>()), Times.Once);
        }

        [Test]
        public void CreateRental_InvalidInputs_ReturnsFailureWithValidationTitle()
        {
            var viewModel = BuildViewModel();

            ViewOperationResult result = viewModel.CreateRental();

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.DialogTitle, Is.EqualTo("Validation Error"));
        }

        [Test]
        public void CreateRental_InvalidInputs_DoesNotCallService()
        {
            var viewModel = BuildViewModel();

            viewModel.CreateRental();

            mockRentalService.Verify(
                svc => svc.CreateConfirmedRental(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<DateTime>(), It.IsAny<DateTime>()),
                Times.Never);
        }

        [Test]
        public void CreateRental_ServiceThrowsException_ReturnsFailureWithExceptionMessage()
        {
            var viewModel = BuildViewModel();
            PopulateWithValidSelections(viewModel);

            mockRentalService
                .Setup(svc => svc.CreateConfirmedRental(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Throws(new InvalidOperationException("Dates overlap with existing rental."));

            ViewOperationResult result = viewModel.CreateRental();

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.DialogTitle, Is.EqualTo("Rental Failed"));
            Assert.That(result.DialogMessage, Does.Contain("overlap"));
        }

       

        [Test]
        public void SaveRental_ValidInputs_ReturnsNull()
        {
            var viewModel = BuildViewModel();
            PopulateWithValidSelections(viewModel);

            string? errorMessage = viewModel.SaveRental();

            Assert.That(errorMessage, Is.Null);
        }

        [Test]
        public void SaveRental_ValidationFails_ReturnsValidationFailedMessage()
        {
            var viewModel = BuildViewModel();

            string? errorMessage = viewModel.SaveRental();

            Assert.That(errorMessage, Is.EqualTo("Validation failed."));
        }

        [Test]
        public void SaveRental_ServiceThrows_ReturnsServiceExceptionMessage()
        {
            var viewModel = BuildViewModel();
            PopulateWithValidSelections(viewModel);

            mockRentalService
                .Setup(svc => svc.CreateConfirmedRental(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Throws(new Exception("Database connection lost."));

            string? errorMessage = viewModel.SaveRental();

            Assert.That(errorMessage, Is.EqualTo("Database connection lost."));
        }


        [Test]
        public void SelectedGameToRent_WhenSet_RaisesPropertyChanged()
        {
            var viewModel = BuildViewModel();
            string? changedProperty = null;
            viewModel.PropertyChanged += (_, args) => changedProperty = args.PropertyName;

            viewModel.SelectedGameToRent = BuildActiveGame(999, OwnerUserId);

            Assert.That(changedProperty, Is.EqualTo(nameof(viewModel.SelectedGameToRent)));
        }

        [Test]
        public void SelectedRenter_WhenSet_RaisesPropertyChanged()
        {
            var viewModel = BuildViewModel();
            string? changedProperty = null;
            viewModel.PropertyChanged += (_, args) => changedProperty = args.PropertyName;

            viewModel.SelectedRenter = new UserDTO { Id = 99 };

            Assert.That(changedProperty, Is.EqualTo(nameof(viewModel.SelectedRenter)));
        }

        [Test]
        public void StartDate_WhenSet_RaisesPropertyChanged()
        {
            var viewModel = BuildViewModel();
            string? changedProperty = null;
            viewModel.PropertyChanged += (_, args) => changedProperty = args.PropertyName;

            viewModel.StartDate = DateTimeOffset.Now.AddDays(1);

            Assert.That(changedProperty, Is.EqualTo(nameof(viewModel.StartDate)));
        }

        [Test]
        public void EndDate_WhenSet_RaisesPropertyChanged()
        {
            var viewModel = BuildViewModel();
            string? changedProperty = null;
            viewModel.PropertyChanged += (_, args) => changedProperty = args.PropertyName;

            viewModel.EndDate = DateTimeOffset.Now.AddDays(5);

            Assert.That(changedProperty, Is.EqualTo(nameof(viewModel.EndDate)));
        }


        private static void PopulateWithValidSelections(CreateRentalViewModel viewModel)
        {
            viewModel.SelectedGameToRent = BuildActiveGame(GameId, OwnerUserId);
            viewModel.SelectedRenter = new UserDTO { Id = RenterUserId, DisplayName = "Renter" };
            viewModel.StartDate = DateTimeOffset.Now.AddDays(1);
            viewModel.EndDate = DateTimeOffset.Now.AddDays(7);
        }

        private static GameDTO BuildActiveGame(int gameId, int ownerId)
        {
            return new GameDTO
            {
                Id = gameId,
                Owner = new UserDTO { Id = ownerId },
                Name = "Test Game",
                Price = 10m,
                IsActive = true
            };
        }
    }
}
