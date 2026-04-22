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
    public class NotificationsViewModelTests
    {
        private Mock<INotificationService> notificationService;
        private Mock<ICurrentUserContext> userContext;

        [SetUp]
        public void Setup()
        {
            notificationService = new Mock<INotificationService>();
            userContext = new Mock<ICurrentUserContext>();

            userContext.SetupGet(ctx => ctx.CurrentUserId).Returns(1);
            notificationService
                .Setup(svc => svc.Subscribe(It.IsAny<IObserver<NotificationDTO>>()))
                .Returns(Mock.Of<IDisposable>());
        }

        [Test]
        public void Constructor_LoadsNotificationsForCurrentUser()
        {
            //Arrange
            notificationService
                .Setup(svc => svc.GetNotificationsForUser(1))
                .Returns(ImmutableList.Create(
                    new NotificationDTO { Id = 1, User = new UserDTO { Id = 1 }, Title = "a", Body = "b" },
                    new NotificationDTO { Id = 2, User = new UserDTO { Id = 1 }, Title = "c", Body = "d" }));

            //Act
            var viewModel = new NotificationsViewModel(notificationService.Object, userContext.Object);

            //Assert
            Assert.That(viewModel.PagedItems.Count, Is.EqualTo(2));
        }

        [Test]
        public void DeleteNotificationByIdentifier_CallsServiceDelete()
        {
            //Arrange
            notificationService
                .Setup(svc => svc.GetNotificationsForUser(1))
                .Returns(ImmutableList<NotificationDTO>.Empty);
            var viewModel = new NotificationsViewModel(notificationService.Object, userContext.Object);

            //Act
            viewModel.DeleteNotificationByIdentifier(7);

            //Assert
            notificationService.Verify(svc => svc.DeleteNotificationByIdentifier(7), Times.Once);
        }
    }
}
