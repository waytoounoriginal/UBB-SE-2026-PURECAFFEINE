using System;
using System.Collections.Immutable;
using System.Linq;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Property_and_Management.Src.DataTransferObjects;
using Property_and_Management.Src.Interface;
using Property_and_Management.Src.Model;
using Property_and_Management.Src.Service;

namespace Property_and_Management.Tests.Service
{
    [TestFixture]
    public sealed class ServiceRentalTests
    {

        private const int ActiveGameId = 10;
        private const int InactiveGameId = 20;
        private const int Owner_Id = 1;
        private const int Renter_Id = 2;
        private const int Fake_Owner_Id = 999;
        private  User ownerUser = new User(Owner_Id, "Gabi");

        private Mock<IRentalRepository> MockRentalRepo = null!;
        private Mock<IGameRepository> MockGameRepo = null!;
        private Mock<IMapper<Rental, RentalDTO>> MockMapper = null!;
        private RentalService RentalServiceToTest = null!;

        [SetUp]
        public void SetUp()
        {
            MockRentalRepo = new Mock<IRentalRepository>();
            MockGameRepo = new Mock<IGameRepository>();
            MockMapper = new Mock<IMapper<Rental, RentalDTO>>();
            var fakeActiveGameId= new Game
            {
                Id = ActiveGameId,
                Owner = ownerUser,
                IsActive = true
            };

            var fakeInactiveGameId = new Game
            {
                Id = InactiveGameId,
                Owner = ownerUser,
                IsActive = false
            };


            MockGameRepo.Setup(repo => repo.Get(ActiveGameId)).Returns(fakeActiveGameId);
            MockGameRepo.Setup(repo => repo.Get(InactiveGameId)).Returns(fakeInactiveGameId);

            MockRentalRepo.Setup(repo => repo.GetRentalsByGame(ActiveGameId))
                           .Returns(ImmutableList<Rental>.Empty);

            MockRentalRepo.Setup(repo => repo.GetRentalsByGame(InactiveGameId ))
                           .Returns(ImmutableList<Rental>.Empty);   

            RentalServiceToTest = new RentalService(
                MockRentalRepo.Object,
                MockGameRepo.Object,
                MockMapper.Object);
        }

        [Test]
        public void CreateConfirmedRental_WithCorrectOwner_CallsAddConfirmedForEachGame()
        {

            RentalServiceToTest.CreateConfirmedRental(InactiveGameId, Renter_Id, Owner_Id, DateTime.UtcNow, DateTime.UtcNow.AddDays(3));

            MockRentalRepo.Verify(repo => repo.AddConfirmed(It.IsAny<Rental>()), Times.Once);

            RentalServiceToTest.CreateConfirmedRental(ActiveGameId, Renter_Id, Owner_Id, DateTime.UtcNow, DateTime.UtcNow.AddDays(3));

            MockRentalRepo.Verify(repo => repo.AddConfirmed(It.IsAny<Rental>()), Times.Exactly(2));




        }





        [Test]
        public void CreateConfirmedRental_WithWrongOwnerId_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() =>
                 RentalServiceToTest.CreateConfirmedRental(
                     ActiveGameId, Renter_Id, Fake_Owner_Id,
                     DateTime.UtcNow, DateTime.UtcNow.AddDays(3)));
        }

        [Test]
        public void CreateConfirmedRental_OnOverlappingDates_ThrowsInvalidOperationExceptionOnlyForSameGame()
        {
            var existingRental = BuildFakeRental(startDate: DateTime.UtcNow.AddDays(1),
                                                 endDate: DateTime.UtcNow.AddDays(3));
            MockRentalRepo.Setup(repo => repo.GetRentalsByGame(ActiveGameId))
                           .Returns(ImmutableList.Create(existingRental));

 
            Assert.Throws<InvalidOperationException>(()=> RentalServiceToTest.CreateConfirmedRental(
                ActiveGameId, Renter_Id, Owner_Id,
                DateTime.UtcNow.AddDays(2), DateTime.UtcNow.AddDays(3)));


            Assert.DoesNotThrow(() => RentalServiceToTest.CreateConfirmedRental(
            InactiveGameId, Renter_Id, Owner_Id,DateTime.UtcNow.AddDays(2), DateTime.UtcNow.AddDays(3))
    );

        }

        [Test]
        public void IsSlotAvailable_DuringBufferPeriod_ReturnsFalsesOnlyForSameGame()
        {
            
            var existingRental = BuildFakeRental(startDate: DateTime.UtcNow.AddDays(1),
                                                 endDate: DateTime.UtcNow.AddDays(2));
            MockRentalRepo.Setup(repo => repo.GetRentalsByGame(ActiveGameId))
                           .Returns(ImmutableList.Create(existingRental));

            bool isAvailable = RentalServiceToTest.IsSlotAvailable(
                ActiveGameId, DateTime.UtcNow.AddDays(3), DateTime.UtcNow.AddDays(4));

            bool isAvailableForAnotherGame = RentalServiceToTest.IsSlotAvailable(
                InactiveGameId, DateTime.UtcNow.AddDays(3), DateTime.UtcNow.AddDays(4));


            Assert.That(isAvailable, Is.False);
            Assert.That(!isAvailableForAnotherGame, Is.False);


        }

       
        private static Rental BuildFakeRental(DateTime startDate, DateTime endDate)
        {
            return new Rental(
                id: 1,
                rentedGame: new Game { Id = ActiveGameId },
                renterUser: new User(Renter_Id, "Renter"),
                ownerUser: new User(Owner_Id, "Owner"),
                startDate: startDate,
                endDate: endDate);
        }
    }
}