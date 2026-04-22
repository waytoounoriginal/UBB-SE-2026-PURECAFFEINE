// Tudor
using System;
using System.Collections.Immutable;
using Moq;
using NUnit.Framework;
using Property_and_Management.Src.DataTransferObjects;
using Property_and_Management.Src.Interface;
using Property_and_Management.Src.Model;
using Property_and_Management.Src.Service;

namespace Property_and_Management.Tests.Service
{
    [TestFixture]
    public class RequestServiceTests
    {
        private Mock<IRequestRepository> requestRepositoryMock;
        private Mock<IRentalRepository> rentalRepositoryMock;
        private Mock<IGameRepository> gameRepositoryMock;
        private Mock<INotificationService> notifications;
        private Mock<IMapper<Request, RequestDTO>> requestMapperMock;
        private RequestService requestService;

        [SetUp]
        public void Setup()
        {
            requestRepositoryMock = new Mock<IRequestRepository>();
            rentalRepositoryMock = new Mock<IRentalRepository>();
            gameRepositoryMock = new Mock<IGameRepository>();
            notifications = new Mock<INotificationService>();
            requestMapperMock = new Mock<IMapper<Request, RequestDTO>>();

            requestService = new RequestService(
                requestRepositoryMock.Object,
                rentalRepositoryMock.Object,
                gameRepositoryMock.Object,
                notifications.Object,
                requestMapperMock.Object);
        }

        [Test]
        public void CreateRequest_WhenRenterIsOwner_ReturnsOwnerCannotRent()
        {
            //Arrange

            //Act
            var result = requestService.CreateRequest(
                gameId: 10,
                renterUserId: 1,
                ownerUserId: 1,
                proposedStartDate: DateTime.UtcNow.AddDays(2),
                proposedEndDate: DateTime.UtcNow.AddDays(4));

            //Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Is.EqualTo(CreateRequestError.OwnerCannotRent));
        }

        [Test]
        public void CancelRequest_AsRenter_DeletesRequestAndNotifications()
        {
            //Arrange
            var existingOpenRequest = new Request
            {
                Id = 100,
                Renter = new User(1, "Renter"),
                Owner = new User(2, "Owner"),
                Game = new Game { Id = 10, Name = "Game" },
                StartDate = DateTime.UtcNow.AddDays(3),
                EndDate = DateTime.UtcNow.AddDays(5),
                Status = RequestStatus.Open
            };
            requestRepositoryMock.Setup(repo => repo.Get(100)).Returns(existingOpenRequest);

            //Act
            var result = requestService.CancelRequest(100, 1);

            //Assert
            Assert.That(result, Is.EqualTo(100));
            requestRepositoryMock.Verify(repo => repo.Delete(100), Times.Once);
            notifications.Verify(notifSvc => notifSvc.DeleteNotificationsLinkedToRequest(100), Times.Once);
        }
    }
}
