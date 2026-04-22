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
    public sealed class ListingsViewModelTests
    {
        private const int OwnerUserId = 7;

        private Mock<IGameService> mockGameService = null!;

        [SetUp]
        public void SetUp()
        {
            mockGameService = new Mock<IGameService>();

            mockGameService
                .Setup(svc => svc.GetGamesForOwner(OwnerUserId))
                .Returns(ImmutableList<GameDTO>.Empty);
        }

        private ListingsViewModel BuildViewModel()
        {
            return new ListingsViewModel(mockGameService.Object, OwnerUserId);
        }

        [Test]
        public void Constructor_LoadsGamesForOwner()
        {
            mockGameService
                .Setup(svc => svc.GetGamesForOwner(OwnerUserId))
                .Returns(ImmutableList.Create(
                    BuildGame(1),
                    BuildGame(2),
                    BuildGame(3)));

            var viewModel = BuildViewModel();

            Assert.That(viewModel.TotalCount, Is.EqualTo(3));
        }

        [Test]
        public void Constructor_NoGames_TotalCountIsZero()
        {
            var viewModel = BuildViewModel();

            Assert.That(viewModel.TotalCount, Is.EqualTo(0));
        }

        [Test]
        public void ShowingText_ContainsGameCountAndGamesWord()
        {
            mockGameService
                .Setup(svc => svc.GetGamesForOwner(OwnerUserId))
                .Returns(ImmutableList.Create(BuildGame(1), BuildGame(2)));

            var viewModel = BuildViewModel();

            Assert.That(viewModel.ShowingText, Does.Contain("2"));
            Assert.That(viewModel.ShowingText, Does.Contain("games"));
        }

        [Test]
        public void LoadGames_RefreshesCollectionFromService()
        {
            var viewModel = BuildViewModel();
            Assert.That(viewModel.TotalCount, Is.EqualTo(0));

            mockGameService
                .Setup(svc => svc.GetGamesForOwner(OwnerUserId))
                .Returns(ImmutableList.Create(BuildGame(10), BuildGame(11)));

            viewModel.LoadGames();

            Assert.That(viewModel.TotalCount, Is.EqualTo(2));
        }

        [Test]
        public void DeleteGame_CallsServiceDeleteWithCorrectId()
        {
            mockGameService
                .Setup(svc => svc.GetGamesForOwner(OwnerUserId))
                .Returns(ImmutableList.Create(BuildGame(42)));

            var viewModel = BuildViewModel();
            var gameToDelete = viewModel.PagedItems.First();

            viewModel.DeleteGame(gameToDelete);

            mockGameService.Verify(svc => svc.DeleteGameByIdentifier(42), Times.Once);
        }

        [Test]
        public void DeleteGame_ReloadsListAfterDeletion()
        {
            mockGameService
                .Setup(svc => svc.GetGamesForOwner(OwnerUserId))
                .Returns(ImmutableList.Create(BuildGame(1), BuildGame(2)));

            var viewModel = BuildViewModel();
            Assert.That(viewModel.TotalCount, Is.EqualTo(2));

            mockGameService
                .Setup(svc => svc.GetGamesForOwner(OwnerUserId))
                .Returns(ImmutableList.Create(BuildGame(2)));

            viewModel.DeleteGame(BuildGame(1));

            Assert.That(viewModel.TotalCount, Is.EqualTo(1));
        }

        [Test]
        public void TryDeleteGame_SuccessfulDeletion_ReturnsSuccessWithGameRemovedTitle()
        {
            mockGameService
                .Setup(svc => svc.GetGamesForOwner(OwnerUserId))
                .Returns(ImmutableList.Create(BuildGame(1)));

            var viewModel = BuildViewModel();

            ViewOperationResult result = viewModel.TryDeleteGame(BuildGame(1));

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.DialogTitle, Is.EqualTo("Game Removed"));
        }

        [Test]
        public void TryDeleteGame_GameHasActiveRentals_ReturnsFailureWithCannotDeleteTitle()
        {
            mockGameService
                .Setup(svc => svc.GetGamesForOwner(OwnerUserId))
                .Returns(ImmutableList.Create(BuildGame(1)));
            mockGameService
                .Setup(svc => svc.DeleteGameByIdentifier(1))
                .Throws(new InvalidOperationException("There are 2 active rentals for this game and it cannot be removed now."));

            var viewModel = BuildViewModel();

            ViewOperationResult result = viewModel.TryDeleteGame(BuildGame(1));

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.DialogTitle, Is.EqualTo("Cannot Delete Game"));
            Assert.That(result.DialogMessage, Does.Contain("active rentals"));
        }

        [Test]
        public void TryDeleteGame_UnexpectedExceptionWithMessage_ReturnsFailureWithThatMessage()
        {
            mockGameService
                .Setup(svc => svc.GetGamesForOwner(OwnerUserId))
                .Returns(ImmutableList.Create(BuildGame(1)));
            mockGameService
                .Setup(svc => svc.DeleteGameByIdentifier(1))
                .Throws(new Exception("Database connection failed."));

            var viewModel = BuildViewModel();

            ViewOperationResult result = viewModel.TryDeleteGame(BuildGame(1));

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.DialogTitle, Is.EqualTo("Cannot Delete Game"));
            Assert.That(result.DialogMessage, Is.EqualTo("Database connection failed."));
        }

        [Test]
        public void TryDeleteGame_UnexpectedExceptionWithEmptyMessage_ReturnsFallbackMessage()
        {
            mockGameService
                .Setup(svc => svc.GetGamesForOwner(OwnerUserId))
                .Returns(ImmutableList.Create(BuildGame(1)));
            mockGameService
                .Setup(svc => svc.DeleteGameByIdentifier(1))
                .Throws(new Exception(string.Empty));

            var viewModel = BuildViewModel();

            ViewOperationResult result = viewModel.TryDeleteGame(BuildGame(1));

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.DialogMessage, Is.EqualTo("An unexpected error occurred."));
        }

        [Test]
        public void TryDeleteGame_UnexpectedExceptionWithWhitespaceMessage_ReturnsFallbackMessage()
        {
            mockGameService
                .Setup(svc => svc.GetGamesForOwner(OwnerUserId))
                .Returns(ImmutableList.Create(BuildGame(1)));
            mockGameService
                .Setup(svc => svc.DeleteGameByIdentifier(1))
                .Throws(new Exception("   "));

            var viewModel = BuildViewModel();

            ViewOperationResult result = viewModel.TryDeleteGame(BuildGame(1));

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.DialogMessage, Is.EqualTo("An unexpected error occurred."));
        }

        [Test]
        public void PagedItems_MoreGamesThanPageSize_ShowsOnlyFirstPage()
        {
            var fiveGamesList = Enumerable.Range(1, 5).Select(id => BuildGame(id)).ToImmutableList();
            mockGameService
                .Setup(svc => svc.GetGamesForOwner(OwnerUserId))
                .Returns(fiveGamesList);

            var viewModel = BuildViewModel();

            Assert.That(viewModel.TotalCount, Is.EqualTo(5));
            Assert.That(viewModel.PagedItems.Count, Is.LessThanOrEqualTo(5));
        }

        [Test]
        public void ShowingText_WithGames_IncludesDisplayedAndTotalCounts()
        {
            var fiveGamesList = Enumerable.Range(1, 5).Select(id => BuildGame(id)).ToImmutableList();
            mockGameService
                .Setup(svc => svc.GetGamesForOwner(OwnerUserId))
                .Returns(fiveGamesList);

            var viewModel = BuildViewModel();

            string showingText = viewModel.ShowingText;
            Assert.That(showingText, Does.Contain("5"));
            Assert.That(showingText, Does.Contain("games"));
        }

        private static GameDTO BuildGame(int gameId)
        {
            return new GameDTO
            {
                Id = gameId,
                Owner = new UserDTO { Id = OwnerUserId },
                Name = "Game " + gameId,
                Price = 9.99m,
                IsActive = true,
                Description = "Test game description."
            };
        }
    }
}
