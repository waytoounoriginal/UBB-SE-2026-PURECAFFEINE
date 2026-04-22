// Tudor
using System;
using Moq;
using NUnit.Framework;
using Property_and_Management.Src.DataTransferObjects;
using Property_and_Management.Src.Interface;
using Property_and_Management.Src.Model;
using Property_and_Management.Src.Service;

namespace Property_and_Management.Tests.Service
{
    [TestFixture]
    public class NotificationServiceTests
    {
        private Mock<INotificationRepository> notificationRepo;
        private Mock<IMapper<Notification, NotificationDTO>> notificationMapperMock;
        private Mock<IServerClient> serverClient;
        private Mock<ICurrentUserContext> userContext;
        private Mock<IToastNotificationService> toastNotificationServiceMock;
        private NotificationService notificationService;

        [SetUp]
        public void Setup()
        {
            notificationRepo = new Mock<INotificationRepository>();
            notificationMapperMock = new Mock<IMapper<Notification, NotificationDTO>>();
            serverClient = new Mock<IServerClient>();
            userContext = new Mock<ICurrentUserContext>();
            toastNotificationServiceMock = new Mock<IToastNotificationService>();

            userContext.SetupGet(ctx => ctx.CurrentUserId).Returns(1);
            serverClient
                .Setup(client => client.Subscribe(It.IsAny<IObserver<IncomingNotification>>()))
                .Returns(Mock.Of<IDisposable>());

            notificationService = new NotificationService(
                notificationRepo.Object,
                notificationMapperMock.Object,
                serverClient.Object,
                userContext.Object,
                toastNotificationServiceMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            notificationService?.Dispose();
        }

        [Test]
        public void SendNotificationToUser_SavesAndForwardsToServer()
        {
            //Arrange
            var notificationDto = new NotificationDTO
            {
                User = new UserDTO { Id = 2 },
                Title = "Hello",
                Body = "World"
            };

            //Act
            notificationService.SendNotificationToUser(2, notificationDto);

            //Assert
            notificationRepo.Verify(repo => repo.Add(It.IsAny<Notification>()), Times.Once);
            serverClient.Verify(client => client.SendNotification(2, "Hello", "World"), Times.Once);
        }

        [Test]
        public void DeleteNotificationsLinkedToRequest_CallsRepository()
        {
            //Arrange

            //Act
            notificationService.DeleteNotificationsLinkedToRequest(42);

            //Assert
            notificationRepo.Verify(repo => repo.DeleteNotificationsLinkedToRequest(42), Times.Once);
        }
    }
}
