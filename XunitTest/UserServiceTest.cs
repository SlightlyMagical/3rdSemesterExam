using Application;
using Application.DTOs;
using Application.Interfaces;
using Application.Validators;
using AutoMapper;
using Domain;
using FluentValidation;
using Moq;

namespace XunitTest
{
    public class UserServiceTest
    {
        //Test 1.1
        [Fact]
        public void CreateUserServiceWithValidRepository()
        {
            //Arrange
            Mock<IUserRepository> mockRepository = new Mock<IUserRepository>();
            IUserRepository repository = mockRepository.Object;
            Mock<IMapper> mockMapper = new Mock<IMapper>();
            var registrationValidator = new UserRegistrationValidator();
            var loginValidator = new UserLoginValidator();

            //Act
            IUserService userService = new UserService(repository, mockMapper.Object, registrationValidator, loginValidator);

            //Assert
            Assert.NotNull(userService);
            Assert.True(userService is UserService);
        }

        //Test 1.2
        [Fact]
        public void CreateUserServiceWithInvalidRepository()
        {
            //Arrange
            IUserService userService = null;

            //Act + Assert
            var ex = Assert.Throws<ArgumentException>(() => userService = new UserService(null, null, null, null));

            Assert.Equal("Missing repository", ex.Message);
            Assert.Null(userService);
        }

        //Test 1.3 - Valid inputs
        [Theory]
        [InlineData("MartinK", "martink@yahoo.com", "hackme", "Client")]    //Valid client user
        [InlineData("Charlie", "penguinz0@yahoo.com", "hackme", "Coach")]   //Valid coach user
        public void CreateValidUser(string username, string email, string password, string usertype)
        {
            // Arrange
            Mock<IUserRepository> userRepositoryMock = new Mock<IUserRepository>();
            IUserRepository repository = userRepositoryMock.Object;

            RegisterUserDTO validUserDTO = new RegisterUserDTO { Username = username, Email = email, Password = password, Usertype = usertype };
            User validUser = new User { Username = username, Email = email, Password = password, Usertype = usertype };

            userRepositoryMock.Setup(x => x.CreateUser(validUser)).Returns(validUser);

            Mock<IMapper> mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<User>(validUserDTO)).Returns(validUser);

            var registrationValidator = new UserRegistrationValidator();
            var loginValidator = new UserLoginValidator();

            IUserService service = new UserService(repository, mockMapper.Object, registrationValidator, loginValidator);

            // Act
            User result = service.CreateUser(validUserDTO);

            // Assert
            Assert.Equal(validUser.Username, result.Username);
            userRepositoryMock.Verify(x => x.CreateUser(validUser), Times.Once);
        }

        //Test 1.4 - Invalid inputs
        [Theory]
        [InlineData("", "penguinz0@yahoo.com", "hackme", "Coach")]              //Username is an empty string
        [InlineData(null, "penguinz0@yahoo.com", "hackme", "Coach")]            //Username is null
        [InlineData("Charlie", "", "hackme", "Coach")]                          //Email is an empty string
        [InlineData("Charlie", null, "hackme", "Coach")]                        //Email is null
        [InlineData("Charlie", "penguinz0@yahoo.com", "", "Coach")]             //Password is an empty string
        [InlineData("Charlie", "penguinz0@yahoo.com", null, "Coach")]           //Password is null
        [InlineData("Charlie", "penguinz0@yahoo.com", "hackme", "")]            //UserType is an empty string
        [InlineData("Charlie", "penguinz0@yahoo.com", "hackme", null)]          //UserType is null
        [InlineData("Charlie", "penguinz0@yahoo.com", "hackme", "UserType")]    //UserType is not "Client" or "Coach"
        public void CreateInvalidUser(string username, string email, string password, string usertype)
        {
            // Arrange
            Mock<IUserRepository> userRepositoryMock = new Mock<IUserRepository>();
            IUserRepository repository = userRepositoryMock.Object;

            RegisterUserDTO invalidUserDTO = new RegisterUserDTO { Username = username, Email = email, Password = password, Usertype = usertype };
            User invalidUser = new User { Username = username, Email = email, Password = password, Usertype = usertype };

            userRepositoryMock.Setup(x => x.CreateUser(invalidUser)).Returns(invalidUser);

            Mock<IMapper> mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<User>(invalidUserDTO)).Returns(invalidUser);
            var registrationValidator = new UserRegistrationValidator();
            var loginValidator = new UserLoginValidator();

            IUserService service = new UserService(repository, mockMapper.Object, registrationValidator, loginValidator);

            //Act + Assert

            Assert.Throws<ValidationException>(() => service.CreateUser(invalidUserDTO));

            userRepositoryMock.Verify(x => x.CreateUser(invalidUser), Times.Never);
        }

        //Test 1.3 - Valid inputs
        [Theory]
        [InlineData("MartinK")]    //Valid client user
        [InlineData("Charlie")]   //Valid coach user
        public void GetValidUserByUsername(string username)
        {
            // Arrange
            Mock<IUserRepository> userRepositoryMock = new Mock<IUserRepository>();
            IUserRepository repository = userRepositoryMock.Object;

            User validUser = new User { Username = username };
            LoginUserDTO validUserDTO = new LoginUserDTO { Username = username, Password = "test" };

            userRepositoryMock.Setup(x => x.ReadUserByUsername(username)).Returns(validUser);

            Mock<IMapper> mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<User>(validUserDTO)).Returns(validUser);

            var registrationValidator = new UserRegistrationValidator();
            var loginValidator = new UserLoginValidator();

            IUserService service = new UserService(repository, mockMapper.Object, registrationValidator, loginValidator);

            // Act
            User result = service.GetUserByUsername(validUserDTO);

            // Assert
            Assert.Equal(validUser.Username, result.Username);
            userRepositoryMock.Verify(x => x.ReadUserByUsername(username), Times.Once);
        }

        [Theory]
        [InlineData(1, "", "penguinz0@yahoo.com", "hackme", "Coach")] // Id is valid and not 0
        [InlineData(2, "", "test@@yahoo.com", "hackme", "Client")] // Id is valid and not 0
        public void DeleteValidUser(int id, string username, string email, string password, string usertype)
        {
            // Arrange
            Mock<IUserRepository> mockRepo = new Mock<IUserRepository>();
            IUserRepository repository = mockRepo.Object;

            IUserService service = new UserService(repository, null, null, null);
            User fakeUser = new User { Id = id, Username = username, Email = email, Password = password, Usertype = usertype };

            mockRepo.Setup(r => r.DeleteUser(id)).Returns(fakeUser);

            // Act
            User result = service.DeleteUser(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(fakeUser, result);
            mockRepo.Verify(r => r.DeleteUser(id), Times.Once);
        }

        [Theory]
        [InlineData(-1, "", "test0@yahoo.com", "hackme", "Client")] // Id is invalid and not above 0
        [InlineData(0, "", "penguinz0@yahoo.com", "hackme", "Coach")] // Id is invalid and not above 0
        public void DeleteInvalidUser(int id, string username, string email, string password, string usertype)
        {
            // Arrange
            Mock<IUserRepository> mockRepo = new Mock<IUserRepository>();
            IUserRepository repository = mockRepo.Object;

            IUserService service = new UserService(repository, null, null, null);
            User fakeUser = new User { Id = id, Username = username, Email = email, Password = password, Usertype = usertype };

            mockRepo.Setup(r => r.DeleteUser(id)).Returns(fakeUser);

            // Act + assert
            Assert.Throws<ArgumentException>(() => service.DeleteUser(id));
            mockRepo.Verify(r => r.DeleteUser(id), Times.Never);
        }

        //[Theory]
        //[InlineData(1, "penguinz0", "penguinz0@yahoo.com", "hackme", "Coach")] // Id is valid and not 0
        //[InlineData(2, "test", "test@@yahoo.com", "hackme", "Client")] // Id is valid and not 0
        //public void GetValidUser(int id, string username, string email, string password, string usertype)
        //{
        //    // Arrange
        //    Mock<IUserRepository> mockRepo = new Mock<IUserRepository>();
        //    IUserRepository repository = mockRepo.Object;

        //    IUserService service = new UserService(repository, null, null);
        //    User fakeUser = new User { Id = id, Username = username, Email = email, Password = password, Usertype = usertype };

        //    mockRepo.Setup(r => r.GetUserById(id)).Returns(fakeUser);

        //    // Act
        //    User result = service.GetUser(id);

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.Equal(fakeUser, result);
        //    mockRepo.Verify(r => r.GetUserById(id), Times.Once);
        //}

        ////Test 1.4 - Invalid inputs
        //[Theory]
        //[InlineData("", "penguinz0@yahoo.com", "hackme", "Coach")]              //Username is an empty string
        //[InlineData(null, "penguinz0@yahoo.com", "hackme", "Coach")]            //Username is null
        //[InlineData("Charlie", "", "hackme", "Coach")]                          //Email is an empty string
        //[InlineData("Charlie", null, "hackme", "Coach")]                        //Email is null
        //[InlineData("Charlie", "penguinz0@yahoo.com", "", "Coach")]             //Password is an empty string
        //[InlineData("Charlie", "penguinz0@yahoo.com", null, "Coach")]           //Password is null
        //[InlineData("Charlie", "penguinz0@yahoo.com", "hackme", "")]            //UserType is an empty string
        //[InlineData("Charlie", "penguinz0@yahoo.com", "hackme", null)]          //UserType is null
        //[InlineData("Charlie", "penguinz0@yahoo.com", "hackme", "UserType")]    //UserType is not "Client" or "Coach"
        //public void GetInvalidUser(string username, string email, string password, string usertype)
        //{
        //    // Arrange
        //    Mock<IUserRepository> userRepositoryMock = new Mock<IUserRepository>();
        //    IUserRepository repository = userRepositoryMock.Object;

        //    LoginUserDTO invalidUserDTO = new LoginUserDTO { Username = username, Password = password };
        //    User invalidUser = new User { Username = username, Email = email, Password = password, Usertype = usertype };

        //    userRepositoryMock.Setup(x => x.CreateUser(invalidUser)).Returns(invalidUser);

        //    Mock<IMapper> mockMapper = new Mock<IMapper>();
        //    mockMapper.Setup(x => x.Map<User>(invalidUserDTO)).Returns(invalidUser);
        //    var validator = new UserValidator();

        //    IUserService service = new UserService(repository, mockMapper.Object, validator);

        //    //Act + Assert

        //    Assert.Throws<ValidationException>(() => service.GetUser(invalidUserDTO));

        //    userRepositoryMock.Verify(x => x.CreateUser(invalidUser), Times.Never);
        //}
    }
}