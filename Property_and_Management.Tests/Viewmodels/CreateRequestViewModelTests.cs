using System;
using System.Collections.Immutable;
using System.Linq;
using Moq;
using NUnit.Framework;
using Property_and_Management.Src.DataTransferObjects;
using Property_and_Management.Src.Interface;
using Property_and_Management.Src.Viewmodels;

namespace Property_and_Management.Tests.Viewmodels
{
    [TestFixture]
    public sealed class CreateRequestViewModelTests
    {
        private const int CurrentUserId = 5;
        private const int OtherOwnerId = 50;
        private const int AvailableGameId = 300;

        private Mock<IGameService> mockGameService = null!;
        private Mock<IRequestService> mockRequestService = null!;
        private Mock<ICurrentUserContext> mockUserContext = null!;

        [SetUp]
        public void SetUp()
        {
            mockGameService = new Mock<IGameService>();
            mockRequestService = new Mock<IRequestService>();
            mockUserContext = new Mock<ICurrentUserContext>();

            mockUserContext.SetupGet(ctx => ctx.CurrentUserId).Returns(CurrentUserId);

           
            mockGameService
                .Setup(svc => svc.GetAllGames())
                .Returns(ImmutableList.Create(BuildOtherUsersGame(AvailableGameId)));
        }

        private CreateRequestViewModel BuildViewModel()
        {
            return new CreateRequestViewModel(
                mockGameService.Object,
                mockRequestService.Object,
                mockUserContext.Object);
        }

      

        [Test]
        public void Constructor_LoadsActiveGamesOwnedByOtherUsers()
        {
            var viewModel = BuildViewModel();

            Assert.That(viewModel.AvailableGamesToRequest, Has.Count.EqualTo(1));
            Assert.That(viewModel.AvailableGamesToRequest[0].Id, Is.EqualTo(AvailableGameId));
        }

        [Test]
        public void Constructor_ExcludesGamesOwnedByCurrentUser()
        {
            var ownGame = new GameDTO
            {
                Id = 999,
                Owner = new UserDTO { Id = CurrentUserId },
                Name = "My Own Game",
                IsActive = true
            };

            mockGameService
                .Setup(svc => svc.GetAllGames())
                .Returns(ImmutableList.Create(BuildOtherUsersGame(AvailableGameId), ownGame));

            var viewModel = BuildViewModel();

            Assert.That(viewModel.AvailableGamesToRequest, Has.Count.EqualTo(1));
            Assert.That(viewModel.AvailableGamesToRequest.Any(game => game.Id == 999), Is.False);
        }

        [Test]
        public void Constructor_ExcludesInactiveGames()
        {
            var inactiveGame = BuildOtherUsersGame(400);
            inactiveGame.IsActive = false;

            mockGameService
                .Setup(svc => svc.GetAllGames())
                .Returns(ImmutableList.Create(BuildOtherUsersGame(AvailableGameId), inactiveGame));

            var viewModel = BuildViewModel();

            Assert.That(viewModel.AvailableGamesToRequest, Has.Count.EqualTo(1));
        }

        [Test]
        public void CurrentUserId_DelegatesToUserContext()
        {
            var viewModel = BuildViewModel();

            Assert.That(viewModel.CurrentUserId, Is.EqualTo(CurrentUserId));
        }

       

        [Test]
        public void LoadAvailableGames_RefreshesCollection()
        {
            var viewModel = BuildViewModel();
            Assert.That(viewModel.AvailableGamesToRequest, Has.Count.EqualTo(1));

            
            mockGameService
                .Setup(svc => svc.GetAllGames())
                .Returns(ImmutableList.Create(
                    BuildOtherUsersGame(AvailableGameId),
                    BuildOtherUsersGame(401)));

            viewModel.LoadAvailableGames();

            Assert.That(viewModel.AvailableGamesToRequest, Has.Count.EqualTo(2));
        }

    

        [Test]
        public void ValidateRequestInputs_AllFieldsValid_ReturnsTrue()
        {
            var viewModel = BuildViewModel();
            PopulateWithValidSelections(viewModel);

            Assert.That(viewModel.ValidateRequestInputs(), Is.True);
        }

        [Test]
        public void ValidateRequestInputs_NoGameSelected_ReturnsFalse()
        {
            var viewModel = BuildViewModel();
            PopulateWithValidSelections(viewModel);
            viewModel.SelectedGame = null;

            Assert.That(viewModel.ValidateRequestInputs(), Is.False);
        }

        [Test]
        public void ValidateRequestInputs_StartDateNull_ReturnsFalse()
        {
            var viewModel = BuildViewModel();
            PopulateWithValidSelections(viewModel);
            viewModel.StartDate = null;

            Assert.That(viewModel.ValidateRequestInputs(), Is.False);
        }

        [Test]
        public void ValidateRequestInputs_EndDateNull_ReturnsFalse()
        {
            var viewModel = BuildViewModel();
            PopulateWithValidSelections(viewModel);
            viewModel.EndDate = null;

            Assert.That(viewModel.ValidateRequestInputs(), Is.False);
        }

        [Test]
        public void ValidateRequestInputs_EndDateBeforeStartDate_ReturnsFalse()
        {
            var viewModel = BuildViewModel();
            PopulateWithValidSelections(viewModel);
            viewModel.EndDate = viewModel.StartDate.Value.AddDays(-1);

            Assert.That(viewModel.ValidateRequestInputs(), Is.False);
        }

        [Test]
        public void ValidateRequestInputs_StartDateInPast_ReturnsFalse()
        {
            var viewModel = BuildViewModel();
            PopulateWithValidSelections(viewModel);
            viewModel.StartDate = DateTimeOffset.Now.AddDays(-3);
            viewModel.EndDate = DateTimeOffset.Now.AddDays(-1);

            Assert.That(viewModel.ValidateRequestInputs(), Is.False);
        }

   
        [Test]
        public void SubmitRequest_ValidInputsAndServiceSucceeds_ReturnsSuccess()
        {
            var viewModel = BuildViewModel();
            PopulateWithValidSelections(viewModel);

            mockRequestService
                .Setup(svc => svc.CreateRequest(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Result<int, CreateRequestError>.Success(1));

            ViewOperationResult result = viewModel.SubmitRequest();

            Assert.That(result.IsSuccess, Is.True);
        }

        [Test]
        public void SubmitRequest_ValidationFails_ReturnsFailureWithValidationTitle()
        {
            var viewModel = BuildViewModel();
            

            ViewOperationResult result = viewModel.SubmitRequest();

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.DialogTitle, Is.EqualTo("Validation Error"));
        }

        [Test]
        public void SubmitRequest_ValidationFails_DoesNotCallService()
        {
            var viewModel = BuildViewModel();

            viewModel.SubmitRequest();

            mockRequestService.Verify(
                svc => svc.CreateRequest(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<DateTime>(), It.IsAny<DateTime>()),
                Times.Never);
        }

        [Test]
        public void SubmitRequest_ServiceReturnsOwnerCannotRent_ReturnsFailureWithOwnGameMessage()
        {
            var viewModel = BuildViewModel();
            PopulateWithValidSelections(viewModel);

            mockRequestService
                .Setup(svc => svc.CreateRequest(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Result<int, CreateRequestError>.Failure(CreateRequestError.OwnerCannotRent));

            ViewOperationResult result = viewModel.SubmitRequest();

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.DialogTitle, Is.EqualTo("Request Failed"));
            Assert.That(result.DialogMessage, Does.Contain("own game"));
        }

        [Test]
        public void SubmitRequest_ServiceReturnsDatesUnavailable_ReturnsFailureWithDatesMessage()
        {
            var viewModel = BuildViewModel();
            PopulateWithValidSelections(viewModel);

            mockRequestService
                .Setup(svc => svc.CreateRequest(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Result<int, CreateRequestError>.Failure(CreateRequestError.DatesUnavailable));

            ViewOperationResult result = viewModel.SubmitRequest();

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.DialogMessage, Does.Contain("not available"));
        }

        [Test]
        public void SubmitRequest_ServiceReturnsGameDoesNotExist_ReturnsFailureWithMissingGameMessage()
        {
            var viewModel = BuildViewModel();
            PopulateWithValidSelections(viewModel);

            mockRequestService
                .Setup(svc => svc.CreateRequest(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Result<int, CreateRequestError>.Failure(CreateRequestError.GameDoesNotExist));

            ViewOperationResult result = viewModel.SubmitRequest();

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.DialogMessage, Does.Contain("no longer exists"));
        }

     

        [Test]
        public void TrySubmitRequest_SuccessfulSubmission_ReturnsNull()
        {
            var viewModel = BuildViewModel();
            PopulateWithValidSelections(viewModel);

            mockRequestService
                .Setup(svc => svc.CreateRequest(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(Result<int, CreateRequestError>.Success(1));

            string? error = viewModel.TrySubmitRequest();

            Assert.That(error, Is.Null);
        }

        [Test]
        public void TrySubmitRequest_FailedSubmission_ReturnsDialogMessage()
        {
            var viewModel = BuildViewModel();
          

            string? error = viewModel.TrySubmitRequest();

            Assert.That(error, Is.Not.Null);
            Assert.That(error, Is.Not.Empty);
        }

    

        [Test]
        public void SelectedGame_WhenSet_RaisesPropertyChanged()
        {
            var viewModel = BuildViewModel();
            string? changedProperty = null;
            viewModel.PropertyChanged += (_, args) => changedProperty = args.PropertyName;

            viewModel.SelectedGame = BuildOtherUsersGame(888);

            Assert.That(changedProperty, Is.EqualTo(nameof(viewModel.SelectedGame)));
        }

        [Test]
        public void StartDate_WhenSet_RaisesPropertyChanged()
        {
            var viewModel = BuildViewModel();
            string? changedProperty = null;
            viewModel.PropertyChanged += (_, args) => changedProperty = args.PropertyName;

            viewModel.StartDate = DateTimeOffset.Now.AddDays(2);

            Assert.That(changedProperty, Is.EqualTo(nameof(viewModel.StartDate)));
        }

        [Test]
        public void EndDate_WhenSet_RaisesPropertyChanged()
        {
            var viewModel = BuildViewModel();
            string? changedProperty = null;
            viewModel.PropertyChanged += (_, args) => changedProperty = args.PropertyName;

            viewModel.EndDate = DateTimeOffset.Now.AddDays(10);

            Assert.That(changedProperty, Is.EqualTo(nameof(viewModel.EndDate)));
        }

        

        private static void PopulateWithValidSelections(CreateRequestViewModel viewModel)
        {
            viewModel.SelectedGame = BuildOtherUsersGame(AvailableGameId);
            viewModel.StartDate = DateTimeOffset.Now.AddDays(1);
            viewModel.EndDate = DateTimeOffset.Now.AddDays(7);
        }

        private static GameDTO BuildOtherUsersGame(int gameId)
        {
            return new GameDTO
            {
                Id = gameId,
                Owner = new UserDTO { Id = OtherOwnerId },
                Name = "Board Game " + gameId,
                Price = 12m,
                IsActive = true
            };
        }
    }
}
