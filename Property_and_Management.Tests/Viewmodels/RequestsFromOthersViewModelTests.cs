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
    public sealed class RequestsFromOthersViewModelTests
    {
        private const int SampleOwnerIdentifier = 1;
        private const int SampleRequestIdentifier = 42;

        private Mock<IRequestService> requestServiceMock = null!;
        private Mock<ICurrentUserContext> currentUserContextMock = null!;
        private RequestsFromOthersViewModel viewModel = null!;

        [SetUp]
        public void SetUp()
        {
            requestServiceMock = new Mock<IRequestService>();
            currentUserContextMock = new Mock<ICurrentUserContext>();
            currentUserContextMock
                .SetupGet(context => context.CurrentUserId)
                .Returns(SampleOwnerIdentifier);
            requestServiceMock
                .Setup(service => service.GetRequestsForOwner(SampleOwnerIdentifier))
                .Returns(ImmutableList<RequestDTO>.Empty);

            viewModel = new RequestsFromOthersViewModel(
                requestServiceMock.Object, currentUserContextMock.Object);
        }

        [Test]
        public void TryApproveRequest_WhenServiceSucceeds_ReturnsNull()
        {
            requestServiceMock
                .Setup(service => service.ApproveRequest(SampleRequestIdentifier, SampleOwnerIdentifier))
                .Returns(Result<int, ApproveRequestError>.Success(500));
            requestServiceMock.Invocations.Clear();

            var errorMessage = viewModel.TryApproveRequest(SampleRequestIdentifier);

            errorMessage.Should().BeNull();
        }

        [Test]
        public void TryDenyRequest_WhenServiceReturnsUnauthorized_ReturnsNonNullErrorMessage()
        {
            requestServiceMock
                .Setup(service => service.DenyRequest(
                    SampleRequestIdentifier, SampleOwnerIdentifier, It.IsAny<string>()))
                .Returns(Result<int, DenyRequestError>.Failure(DenyRequestError.Unauthorized));

            var errorMessage = viewModel.TryDenyRequest(SampleRequestIdentifier, "unavailable");

            errorMessage.Should().NotBeNull();
        }
    }
}
