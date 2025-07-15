using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Moq;
using SCWebService.Controllers;
using SCWebService.Models.MatchmakingService;
using SCWebService.Models.UserService;
using SCWebService.Services.Matchmaking;
using SCWebService.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SCWebService.Tests.Controllers
{
    public class RankedMatchmakingControllerTests : IDisposable
    {
        private readonly Mock<IRankedMatchmakingService> _mockMatchmakingService;
        private readonly Mock<IUserService> _mockUserService;
        private readonly RankedMatchmakingController _controller;
        private readonly string _jwtSecretKey = "test-secret-key-that-is-long-enough-for-hmac-sha256-algorithm";

        public RankedMatchmakingControllerTests()
        {
            _mockMatchmakingService = new Mock<IRankedMatchmakingService>();
            _mockUserService = new Mock<IUserService>();
            _controller = new RankedMatchmakingController(_mockMatchmakingService.Object, _mockUserService.Object);

            // Set up environment variable for JWT secret
            Environment.SetEnvironmentVariable("JWT_SECRET_KEY", _jwtSecretKey);
        }

        public void Dispose()
        {
            // Clean up environment variable
            Environment.SetEnvironmentVariable("JWT_SECRET_KEY", null);
        }

        #region Find Match Tests

        [Fact]
        public async Task Get_FindMatch_WithValidHost_ReturnsOkWithMatchmakingUser()
        {
            // Arrange
            var inputUser = new RankedMatchmakingUser { UserName = "testuser", UserMMR = 1500, JoinCode = "Test94" };
            var expectedHost = new RankedMatchmakingUser { UserName = "hostuser", UserMMR = 1520, JoinCode = "Test94" };
            _mockMatchmakingService.Setup(s => s.FindValidHostAsync(inputUser.UserName, inputUser.UserMMR))
                                  .ReturnsAsync(expectedHost);

            // Act
            var result = await _controller.Get(inputUser);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.Equal(200, jsonResult.StatusCode);
            Assert.Equal(expectedHost, jsonResult.Value);
        }

        [Fact]
        public async Task Get_FindMatch_WithNoValidHost_ReturnsNoContent()
        {
            // Arrange
            var inputUser = new RankedMatchmakingUser { UserName = "testuser", UserMMR = 1500, JoinCode = "Test94" };
            _mockMatchmakingService.Setup(s => s.FindValidHostAsync(inputUser.UserName, inputUser.UserMMR))
                                  .ReturnsAsync((RankedMatchmakingUser)null);

            // Act
            var result = await _controller.Get(inputUser);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.Equal(204, jsonResult.StatusCode);
            Assert.Equal("", jsonResult.Value);
        }

        [Fact]
        public async Task Get_FindMatch_CallsMatchmakingServiceWithCorrectParameters()
        {
            // Arrange
            var inputUser = new RankedMatchmakingUser { UserName = "testuser", UserMMR = 1500, JoinCode = "Test94" };
            _mockMatchmakingService.Setup(s => s.FindValidHostAsync(It.IsAny<string>(), It.IsAny<int>()))
                                  .ReturnsAsync((RankedMatchmakingUser)null);

            // Act
            await _controller.Get(inputUser);

            // Assert
            _mockMatchmakingService.Verify(s => s.FindValidHostAsync(inputUser.UserName, inputUser.UserMMR), Times.Once);
        }

        #endregion

        #region Add Host Tests

        [Fact]
        public async Task Post_AddHost_CallsCreateAsyncAndReturnsAccepted()
        {
            // Arrange
            var mmUser = new RankedMatchmakingUser { UserName = "hostuser", UserMMR = 1600, JoinCode = "Test94" };
            _mockMatchmakingService.Setup(s => s.CreateAsync(mmUser))
                                  .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Post(mmUser);

            // Assert
            var acceptedResult = Assert.IsType<AcceptedResult>(result);
            _mockMatchmakingService.Verify(s => s.CreateAsync(mmUser), Times.Once);
        }

        #endregion

        #region Remove From Queue Tests

        [Fact]
        public async Task RemoveFromQueue_WithSuccessfulRemoval_ReturnsAccepted()
        {
            // Arrange
            var username = "testuser";
            _mockMatchmakingService.Setup(s => s.TryRemoveFromQueue(username))
                                  .ReturnsAsync(true);

            // Act
            var result = await _controller.RemoveFromQueue(username);

            // Assert
            Assert.IsType<AcceptedResult>(result);
        }

        [Fact]
        public async Task RemoveFromQueue_WithFailedRemoval_ReturnsServiceUnavailable()
        {
            // Arrange
            var username = "testuser";
            _mockMatchmakingService.Setup(s => s.TryRemoveFromQueue(username))
                                  .ReturnsAsync(false);

            // Act
            var result = await _controller.RemoveFromQueue(username);

            // Assert
            var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(503, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task RemoveFromQueue_CallsServiceWithCorrectUsername()
        {
            // Arrange
            var username = "testuser";
            _mockMatchmakingService.Setup(s => s.TryRemoveFromQueue(username))
                                  .ReturnsAsync(true);

            // Act
            await _controller.RemoveFromQueue(username);

            // Assert
            _mockMatchmakingService.Verify(s => s.TryRemoveFromQueue(username), Times.Once);
        }

        #endregion

        #region Start Match Tests

        [Fact]
        public void StartMatch_WithValidUserId_ReturnsOkWithJwtToken()
        {
            // Arrange
            var userId = "user123";

            // Act
            var result = _controller.StartMatch(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var token = Assert.IsType<string>(okResult.Value);
            Assert.NotEmpty(token);

            // Verify token structure
            var handler = new JwtSecurityTokenHandler();
            Assert.True(handler.CanReadToken(token));
        }

        [Fact]
        public void StartMatch_TokenContainsCorrectClaims()
        {
            // Arrange
            var userId = "user123";

            // Act
            var result = _controller.StartMatch(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var token = Assert.IsType<string>(okResult.Value);

            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(token);

            Assert.Equal(userId, jsonToken.Claims.First(c => c.Type == "userID").Value);
            Assert.Contains(jsonToken.Claims, c => c.Type == "start");
        }

        #endregion

        #region Submit Match Result Tests

        [Fact]
        public async Task PostMatchUpdate_WithValidTokenAndUser_ReturnsAcceptedWithGems()
        {
            // Arrange
            var userId = "user123";
            var user = new User { _id = userId, gems = 100, userMMR = 1500, wins = 5, losses = 3 };
            var validToken = CreateValidJwtToken(userId);
            var submission = new MatchSubmission
            {
                Token = validToken,
                Victory = true,
                Ranked = true,
                LocalMMR = 1500,
                OppositionMMR = 1520
            };

            _mockUserService.Setup(s => s.GetAsyncSecure(userId))
                           .ReturnsAsync(user);
            _mockUserService.Setup(s => s.UpdateAsyncSecure(It.IsAny<User>()))
                           .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.PostMatchUpdate(submission);

            // Assert
            var acceptedResult = Assert.IsType<AcceptedResult>(result);
            var gems = Assert.IsType<int>(acceptedResult.Value);
            Assert.True(gems > 0); // Victory should give gems
        }

        [Fact]
        public async Task PostMatchUpdate_WithInvalidToken_ReturnsUnauthorized()
        {
            // Arrange
            var submission = new MatchSubmission
            {
                Token = "invalid-token",
                Victory = true,
                Ranked = false,
                LocalMMR = 1500,
                OppositionMMR = 1520
            };

            // Act
            var result = await _controller.PostMatchUpdate(submission);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Invalid or expired token.", unauthorizedResult.Value);
        }

        [Fact]
        public async Task PostMatchUpdate_WithValidTokenButUserNotFound_ReturnsNotFound()
        {
            // Arrange
            var userId = "nonexistent-user";
            var validToken = CreateValidJwtToken(userId);
            var submission = new MatchSubmission
            {
                Token = validToken,
                Victory = true,
                Ranked = false,
                LocalMMR = 1500,
                OppositionMMR = 1520
            };

            _mockUserService.Setup(s => s.GetAsyncSecure(userId))
                           .ReturnsAsync((User)null);

            // Act
            var result = await _controller.PostMatchUpdate(submission);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task PostMatchUpdate_WithTooQuickMatch_ReturnsBadRequest()
        {
            // Arrange
            var userId = "user123";
            var recentToken = CreateValidJwtToken(userId, DateTime.UtcNow); // Token created now
            var submission = new MatchSubmission
            {
                Token = recentToken,
                Victory = true,
                Ranked = false,
                LocalMMR = 1500,
                OppositionMMR = 1520
            };

            // Act
            var result = await _controller.PostMatchUpdate(submission);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid match data", badRequestResult.Value);
        }

        [Fact]
        public async Task PostMatchUpdate_UpdatesUserStatsCorrectly_ForVictory()
        {
            // Arrange
            var userId = "user123";
            var user = new User { _id = userId, gems = 100, userMMR = 1500, wins = 5, losses = 3 };
            var validToken = CreateValidJwtToken(userId);
            var submission = new MatchSubmission
            {
                Token = validToken,
                Victory = true,
                Ranked = true,
                LocalMMR = 1500,
                OppositionMMR = 1520
            };

            _mockUserService.Setup(s => s.GetAsyncSecure(userId))
                           .ReturnsAsync(user);
            _mockUserService.Setup(s => s.UpdateAsyncSecure(It.IsAny<User>()))
                           .Returns(Task.CompletedTask);

            // Act
            await _controller.PostMatchUpdate(submission);

            // Assert
            _mockUserService.Verify(s => s.UpdateAsyncSecure(It.Is<User>(u =>
                u.wins == 6 && // Should increment wins
                u.losses == 3 && // Should not change losses
                u.gems > 100 && // Should increase gems
                u.userMMR > 1500 // Should increase MMR for ranked victory
            )), Times.Once);
        }

        [Fact]
        public async Task PostMatchUpdate_UpdatesUserStatsCorrectly_ForDefeat()
        {
            // Arrange
            var userId = "user123";
            var user = new User { _id = userId, gems = 100, userMMR = 1500, wins = 5, losses = 3 };
            var validToken = CreateValidJwtToken(userId);
            var submission = new MatchSubmission
            {
                Token = validToken,
                Victory = false,
                Ranked = true,
                LocalMMR = 1500,
                OppositionMMR = 1520
            };

            _mockUserService.Setup(s => s.GetAsyncSecure(userId))
                           .ReturnsAsync(user);
            _mockUserService.Setup(s => s.UpdateAsyncSecure(It.IsAny<User>()))
                           .Returns(Task.CompletedTask);

            // Act
            await _controller.PostMatchUpdate(submission);

            // Assert
            _mockUserService.Verify(s => s.UpdateAsyncSecure(It.Is<User>(u =>
                u.wins == 5 && // Should not change wins
                u.losses == 4 && // Should increment losses
                u.gems > 100 && // Should still get some gems
                u.userMMR < 1500 // Should decrease MMR for ranked defeat
            )), Times.Once);
        }

        [Fact]
        public async Task PostMatchUpdate_DoesNotUpdateMMR_ForUnrankedMatch()
        {
            // Arrange
            var userId = "user123";
            var user = new User { _id = userId, gems = 100, userMMR = 1500, wins = 5, losses = 3 };
            var validToken = CreateValidJwtToken(userId);
            var submission = new MatchSubmission
            {
                Token = validToken,
                Victory = true,
                Ranked = false, // Unranked match
                LocalMMR = 1500,
                OppositionMMR = 1520
            };

            _mockUserService.Setup(s => s.GetAsyncSecure(userId))
                           .ReturnsAsync(user);
            _mockUserService.Setup(s => s.UpdateAsyncSecure(It.IsAny<User>()))
                           .Returns(Task.CompletedTask);

            // Act
            await _controller.PostMatchUpdate(submission);

            // Assert
            _mockUserService.Verify(s => s.UpdateAsyncSecure(It.Is<User>(u =>
                u.userMMR == 1500 // MMR should not change for unranked
            )), Times.Once);
        }

        #endregion

        #region Helper Methods

        private string CreateValidJwtToken(string userId, DateTime? startTime = null)
        {
            var actualStartTime = startTime ?? DateTime.UtcNow.AddMinutes(-5); // Default to 5 minutes ago

            var claims = new[]
            {
                new Claim("userID", userId),
                new Claim("start", actualStartTime.ToString("O"))
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "SCWebService",
                audience: "SCClient",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(120),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        #endregion
    }
}