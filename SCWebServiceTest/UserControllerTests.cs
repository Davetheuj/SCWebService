using Microsoft.AspNetCore.Mvc;
using Moq;
using SCWebService.Controllers;
using SCWebService.Models.UserService;
using SCWebService.Services;
using System.Threading.Tasks;
using Xunit;

namespace SCWebService.Tests.Controllers
{
    public class UserControllerTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly UserController _controller;

        public UserControllerTests()
        {
            _mockUserService = new Mock<IUserService>();
            _controller = new UserController(_mockUserService.Object);
        }

        #region Register Tests

        [Fact]
        public async Task Post_Register_WithNewUser_ReturnsCreatedAtAction()
        {
            // Arrange
            var newUser = new User
            {
                userName = "newuser",
                userPassword = "password123",
                userEmail = "test@example.com"
            };

            // Mock that no existing user is found
            _mockUserService.Setup(s => s.GetAsyncUnsecured(newUser.userName))
                           .ReturnsAsync((User)null);

            _mockUserService.Setup(s => s.CreateAsync(It.IsAny<User>()))
                           .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Post(newUser);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(_controller.Get), createdAtActionResult.ActionName);
            Assert.Equal(newUser, createdAtActionResult.Value);
        }

        [Fact]
        public async Task Post_Register_WithExistingUser_ReturnsJsonResultWithTaken()
        {
            // Arrange
            var existingUser = new User
            {
                userName = "existinguser",
                userPassword = "password123",
                userEmail = "existing@example.com"
            };

            var newUser = new User
            {
                userName = "existinguser", // Same username
                userPassword = "differentpassword",
                userEmail = "different@example.com"
            };

            _mockUserService.Setup(s => s.GetAsyncUnsecured(newUser.userName))
                           .ReturnsAsync(existingUser);

            // Act
            var result = await _controller.Post(newUser);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.Equal("taken", jsonResult.Value);
        }


        [Fact]
        public async Task Post_Register_CallsCreateAsyncWithCorrectUser()
        {
            // Arrange
            var newUser = new User
            {
                userName = "testuser",
                userPassword = "password123"
            };

            _mockUserService.Setup(s => s.GetAsyncUnsecured(newUser.userName))
                           .ReturnsAsync((User)null);

            _mockUserService.Setup(s => s.CreateAsync(newUser))
                           .Returns(Task.CompletedTask);

            // Act
            await _controller.Post(newUser);

            // Assert
            _mockUserService.Verify(s => s.CreateAsync(newUser), Times.Once);
        }

        [Fact]
        public async Task Post_Register_DoesNotCallCreateAsync_WhenUserExists()
        {
            // Arrange
            var existingUser = new User { userName = "existinguser" };
            var newUser = new User { userName = "existinguser" };

            _mockUserService.Setup(s => s.GetAsyncUnsecured(newUser.userName))
                           .ReturnsAsync(existingUser);

            // Act
            await _controller.Post(newUser);

            // Assert
            _mockUserService.Verify(s => s.CreateAsync(It.IsAny<User>()), Times.Never);
        }

        #endregion

        #region Login Tests

        [Fact]
        public async Task Get_Login_WithValidCredentials_ReturnsUser()
        {
            // Arrange
            var loginUser = new User
            {
                userName = "testuser",
                userPassword = "password123"
            };

            var authenticatedUser = new User
            {
                _id = "user123",
                userName = "testuser",
                userEmail = "test@example.com",
                gems = 100,
                userMMR = 1500
            };

            _mockUserService.Setup(s => s.GetAsyncSecure(loginUser))
                           .ReturnsAsync(authenticatedUser);

            // Act
            var result = await _controller.Get(loginUser);

            // Assert
            var actionResult = Assert.IsType<ActionResult<User>>(result);
            Assert.Equal(authenticatedUser, actionResult.Value);
        }

        [Fact]
        public async Task Get_Login_WithInvalidCredentials_ReturnsNotFound()
        {
            // Arrange
            var loginUser = new User
            {
                userName = "testuser",
                userPassword = "wrongpassword"
            };

            _mockUserService.Setup(s => s.GetAsyncSecure(loginUser))
                           .ReturnsAsync((User)null);

            // Act
            var result = await _controller.Get(loginUser);

            // Assert
            var actionResult = Assert.IsType<ActionResult<User>>(result);
            Assert.IsType<NotFoundResult>(actionResult.Result);
        }

        [Fact]
        public async Task Get_Login_CallsGetAsyncSecureWithCorrectUser()
        {
            // Arrange
            var loginUser = new User
            {
                userName = "testuser",
                userPassword = "password123"
            };

            _mockUserService.Setup(s => s.GetAsyncSecure(loginUser))
                           .ReturnsAsync((User)null);

            // Act
            await _controller.Get(loginUser);

            // Assert
            _mockUserService.Verify(s => s.GetAsyncSecure(loginUser), Times.Once);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task Get_Login_WithEmptyUsername_ReturnsNotFound(string username)
        {
            // Arrange
            var loginUser = new User
            {
                userName = username,
                userPassword = "password123"
            };

            _mockUserService.Setup(s => s.GetAsyncSecure(loginUser))
                           .ReturnsAsync((User)null);

            // Act
            var result = await _controller.Get(loginUser);

            // Assert
            var actionResult = Assert.IsType<ActionResult<User>>(result);
            Assert.IsType<NotFoundResult>(actionResult.Result);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task Get_Login_WithEmptyPassword_ReturnsNotFound(string password)
        {
            // Arrange
            var loginUser = new User
            {
                userName = "testuser",
                userPassword = password
            };

            _mockUserService.Setup(s => s.GetAsyncSecure(loginUser))
                           .ReturnsAsync((User)null);

            // Act
            var result = await _controller.Get(loginUser);

            // Assert
            var actionResult = Assert.IsType<ActionResult<User>>(result);
            Assert.IsType<NotFoundResult>(actionResult.Result);
        }

        #endregion

        #region Update Board Preset Tests

        [Fact]
        public async Task UpdateBoardPreset_WithValidUser_ReturnsAccepted()
        {
            // Arrange
            var updatedUser = new User
            {
                _id = "user123",
                userName = "testuser"
            };

            _mockUserService.Setup(s => s.UpdateAsyncSecure(updatedUser))
                           .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.UpdateBoardPreset(updatedUser);

            // Assert
            Assert.IsType<AcceptedResult>(result);
        }

        [Fact]
        public async Task UpdateBoardPreset_CallsUpdateAsyncSecureWithCorrectUser()
        {
            // Arrange
            var updatedUser = new User
            {
                _id = "user123",
                userName = "testuser"
            };

            _mockUserService.Setup(s => s.UpdateAsyncSecure(updatedUser))
                           .Returns(Task.CompletedTask);

            // Act
            await _controller.UpdateBoardPreset(updatedUser);

            // Assert
            _mockUserService.Verify(s => s.UpdateAsyncSecure(updatedUser), Times.Once);
        }

        [Fact]
        public async Task UpdateBoardPreset_WithNullUser_StillCallsService()
        {
            // Arrange
            User nullUser = null;

            _mockUserService.Setup(s => s.UpdateAsyncSecure(nullUser))
                           .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.UpdateBoardPreset(nullUser);

            // Assert
            Assert.IsType<AcceptedResult>(result);
            _mockUserService.Verify(s => s.UpdateAsyncSecure(nullUser), Times.Once);
        }

        [Fact]
        public async Task UpdateBoardPreset_ServiceThrowsException_ExceptionBubbles()
        {
            // Arrange
            var updatedUser = new User
            {
                _id = "user123",
                userName = "testuser"
            };

            _mockUserService.Setup(s => s.UpdateAsyncSecure(updatedUser))
                           .ThrowsAsync(new System.Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<System.Exception>(() =>
                _controller.UpdateBoardPreset(updatedUser));
        }

        #endregion

        #region Edge Case Tests

        [Fact]
        public async Task Post_Register_WithUserThatHasId_StillProcessesCorrectly()
        {
            // Arrange
            var newUser = new User
            {
                _id = "should-be-ignored", // ID should be set by database
                userName = "testuser"
            };

            _mockUserService.Setup(s => s.GetAsyncUnsecured(newUser.userName))
                           .ReturnsAsync((User)null);

            _mockUserService.Setup(s => s.CreateAsync(It.IsAny<User>()))
                           .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Post(newUser);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(newUser, createdAtActionResult.Value);
        }

        [Fact]
        public async Task Get_Login_ReturnsCorrectRouteValues()
        {
            // Arrange
            var loginUser = new User { userName = "test", userPassword = "pass" };
            var authenticatedUser = new User
            {
                _id = "user123",
                userName = "test"
            };

            _mockUserService.Setup(s => s.GetAsyncSecure(loginUser))
                           .ReturnsAsync(authenticatedUser);

            // Act
            var result = await _controller.Get(loginUser);

            // Assert
            var actionResult = Assert.IsType<ActionResult<User>>(result);
            Assert.Equal(authenticatedUser, actionResult.Value);
        }

        #endregion
    }
}