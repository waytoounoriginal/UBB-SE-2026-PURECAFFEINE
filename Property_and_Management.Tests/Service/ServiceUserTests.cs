using Moq;
using NUnit.Framework;
using Property_and_Management.Src.DataTransferObjects;
using Property_and_Management.Src.Interface;
using Property_and_Management.Src.Model;
using Property_and_Management.Src.Service;
using System.Collections.Immutable;
using System.Linq;

namespace Property_and_Management.Tests.Service
{
    [TestFixture]
    public sealed class ServiceUserTests
    {
        private const int MeId = 1;
        private const int SecondId = 2;
        private const int ThirdId = 3;

        private Mock<IUserRepository> RepositoryMock = null!;
        private Mock<IMapper<User, UserDTO>> MapperMock = null!;
        private UserService Service = null!;

        [SetUp]
        public void SetUp()
        {
            RepositoryMock = new Mock<IUserRepository>();
            MapperMock = new Mock<IMapper<User, UserDTO>>();
            MapperMock
                .Setup(mapper => mapper.ToDTO(It.IsAny<User>()))
                .Returns<User>(user => new UserDTO { Id = user.Id, DisplayName = user.DisplayName });

            Service = new UserService(RepositoryMock.Object, MapperMock.Object);
        }


        [Test]
        public void GetUserExcept_WithMultipleUsers_ReturnsAllUsersBesidesTheCurrentOne()
        {

            var allUsers = ImmutableList.Create(
                new User(SecondId, "Maria"),
                new User(ThirdId, "GABI")
            );
            RepositoryMock.Setup(db => db.GetAll()).Returns(allUsers);

            var result = Service.GetUsersExcept(MeId);
            Assert.That(result.Any(user => user.Id == SecondId && user.DisplayName == "Maria"), Is.True);
            Assert.That(result.Any(user => user.Id == ThirdId && user.DisplayName == "GABI"), Is.True);
        }


        [Test]
        public void GetUsersExcept_WhenNoOtherUsersExist_ReturnsEmptyList()
        {

            

            RepositoryMock.Setup(db => db.GetAll()).Returns(ImmutableList.Create(new User(MeId, "Me")));
            var numberOfUsersWhenOnlyCurrentUserExists = Service.GetUsersExcept(MeId);
            Assert.That(numberOfUsersWhenOnlyCurrentUserExists, Has.Count.EqualTo(0));
        }

        [Test]
        public void GetUsersExcept_WhenThereAreNoUsers_ReturnsEmptyList()
        {

            RepositoryMock.Setup(db => db.GetAll()).Returns(ImmutableList<User>.Empty);
            var result = Service.GetUsersExcept(MeId);
            Assert.That(result, Is.Empty);
        }


        [Test]
        public void GetUsersExcept_WithMultipleUsers_ReturnsTheCorrectNumberOfUsersExcludingCurrentOne()
        {
            var allUsers = ImmutableList.Create(
                new User(MeId, "Me"),
                new User(SecondId, "Alice"),
                new User(ThirdId, "Bob")
            );
            RepositoryMock.Setup(db => db.GetAll()).Returns(allUsers);


            var result = Service.GetUsersExcept(MeId);

            Assert.That(result.Select(user => user.Id), Does.Not.Contain(MeId));
            Assert.That(result, Has.Count.EqualTo(2));

        }

        
    }
}