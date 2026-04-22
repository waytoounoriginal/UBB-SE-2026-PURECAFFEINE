using System;
using System.Collections.Immutable;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Property_and_Management.Src.DataTransferObjects;
using Property_and_Management.Src.Interface;
using Property_and_Management.Src.Viewmodels;

namespace Property_and_Management.Tests.Viewmodels
{
    [TestFixture]
    public sealed class RentalsFromOthersViewModelTests
    {
        private const int SampleRenterIdentifier = 1;

        private Mock<IRentalService> rentalServiceMock = null!;
        private Mock<ICurrentUserContext> currentUserContextMock = null!;

        [SetUp]
        public void SetUp()
        {
            rentalServiceMock = new Mock<IRentalService>();
            currentUserContextMock = new Mock<ICurrentUserContext>();
            currentUserContextMock
                .SetupGet(context => context.CurrentUserId)
                .Returns(SampleRenterIdentifier);
            rentalServiceMock
                .Setup(service => service.GetRentalsForRenter(SampleRenterIdentifier))
                .Returns(ImmutableList<RentalDTO>.Empty);
        }

        [Test]
        public void Constructor_LoadsRentalsForCurrentRenter()
        {
            rentalServiceMock
                .Setup(service => service.GetRentalsForRenter(SampleRenterIdentifier))
                .Returns(ImmutableList.Create(BuildRental(1), BuildRental(2)));

            var viewModel = new RentalsFromOthersViewModel(rentalServiceMock.Object, currentUserContextMock.Object);

            viewModel.TotalCount.Should().Be(2);
        }

        [Test]
        public void Reload_OrdersRentalsByStartDateDescending()
        {
            var olderRental = BuildRental(identifier: 1, startDate: DateTime.UtcNow.AddDays(2));
            var newerRental = BuildRental(identifier: 2, startDate: DateTime.UtcNow.AddDays(10));
            rentalServiceMock
                .Setup(service => service.GetRentalsForRenter(SampleRenterIdentifier))
                .Returns(ImmutableList.Create(olderRental, newerRental));

            var viewModel = new RentalsFromOthersViewModel(rentalServiceMock.Object, currentUserContextMock.Object);

            viewModel.PagedItems[0].Id.Should().Be(2);
        }

        private static RentalDTO BuildRental(int identifier, DateTime? startDate = null)
        {
            return new RentalDTO
            {
                Id = identifier,
                Game = new GameDTO { Id = 100 },
                Renter = new UserDTO { Id = SampleRenterIdentifier },
                Owner = new UserDTO { Id = 99 },
                StartDate = startDate ?? DateTime.UtcNow.AddDays(1),
                EndDate = (startDate ?? DateTime.UtcNow.AddDays(1)).AddDays(2),
            };
        }
    }
}
