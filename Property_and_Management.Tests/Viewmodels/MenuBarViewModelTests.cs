using NUnit.Framework;
using Property_and_Management.Src.Viewmodels;

namespace Property_and_Management.Tests.Viewmodels
{
    [TestFixture]
    public sealed class MenuBarViewModelTests
    {
        private MenuBarViewModel viewModel = null!;
        private AppPage? capturedNavigationTarget;
        private bool navigationWasTriggered;
        private int navigationTriggerCount;

        [SetUp]
        public void SetUp()
        {
            viewModel = new MenuBarViewModel();
            capturedNavigationTarget = null;
            navigationWasTriggered = false;
            navigationTriggerCount = 0;
        }

        [Test]
        public void Constructor_WhenViewModelIsCreated_RegistersAllSixMenuEntries()
        {
            var registeredMenuLabels = viewModel.NavigationActionsByMenuLabel.Keys;

            Assert.That(registeredMenuLabels, Does.Contain("My Games"));
            Assert.That(registeredMenuLabels, Does.Contain("Others' Requests"));
            Assert.That(registeredMenuLabels, Does.Contain("Others' Rentals"));
            Assert.That(registeredMenuLabels, Does.Contain("My Requests"));
            Assert.That(registeredMenuLabels, Does.Contain("My Rentals"));
            Assert.That(registeredMenuLabels, Does.Contain("Notifications"));
        }

        [Test]
        public void SelectedPageName_MyGames_FiresListingsNavigation()
        {
            viewModel.RequestNavigation += CaptureNavigationTarget;
            viewModel.SelectedPageName = "My Games";
            Assert.That(capturedNavigationTarget, Is.EqualTo(AppPage.Listings));
        }

        [Test]
        public void SelectedPageName_Notifications_FiresNotificationsNavigation()
        {
            viewModel.RequestNavigation += CaptureNavigationTarget;
            viewModel.SelectedPageName = "Notifications";
            Assert.That(capturedNavigationTarget, Is.EqualTo(AppPage.Notifications));
        }

        [Test]
        public void SelectedPageName_MyRentals_FiresRentalsFromOthersNavigation()
        {
            viewModel.RequestNavigation += CaptureNavigationTarget;
            viewModel.SelectedPageName = "My Rentals";
            Assert.That(capturedNavigationTarget, Is.EqualTo(AppPage.RentalsFromOthers));
        }

        [Test]
        public void SelectedPageName_OthersRentals_FiresRentalsToOthersNavigation()
        {
            viewModel.RequestNavigation += CaptureNavigationTarget;
            viewModel.SelectedPageName = "Others' Rentals";
            Assert.That(capturedNavigationTarget, Is.EqualTo(AppPage.RentalsToOthers));
        }

        [Test]
        public void SelectedPageName_UnrecognisedLabel_DoesNotFireNavigation()
        {
            viewModel.RequestNavigation += MarkNavigationAsTriggered;
            viewModel.SelectedPageName = "Unknown page";
            Assert.That(navigationWasTriggered, Is.False);
        }

        [Test]
        public void SelectedPageName_SetToSameValueTwice_FiresNavigationOnlyOnce()
        {
            viewModel.RequestNavigation += IncrementNavigationTriggerCount;
            viewModel.SelectedPageName = "My Rentals";
            viewModel.SelectedPageName = "My Rentals";
            Assert.That(navigationTriggerCount, Is.EqualTo(1));
        }

        private void CaptureNavigationTarget(AppPage selectedPage)
        {
            capturedNavigationTarget = selectedPage;
        }

        private void MarkNavigationAsTriggered(AppPage selectedPage)
        {
            _ = selectedPage;
            navigationWasTriggered = true;
        }

        private void IncrementNavigationTriggerCount(AppPage selectedPage)
        {
            _ = selectedPage;
            navigationTriggerCount++;
        }
    }
}
