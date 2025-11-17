using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BrainBurst.BLL.DTO;
using BrainBurst.BLL.Enums;
using BrainBurst.BLL.Interfaces;
using BrainBurst.BLL.Services;
using BrainBurst.DAL.Abstractions;
using BrainBurst.DAL.Entities;
using Moq;
using Xunit;

namespace BrainBurst.BLL.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _usersMock;
        private readonly Mock<IRatingService> _ratingMock;
        private readonly AuthService _service;

        public AuthServiceTests()
        {
            _usersMock  = new Mock<IUserRepository>(MockBehavior.Strict);
            _ratingMock = new Mock<IRatingService>(MockBehavior.Strict);

            _service = new AuthService(_usersMock.Object, _ratingMock.Object);
        }

        // RegisterAsync

        [Fact]
        public async Task RegisterAsync_EmailAlreadyExists_ThrowsArgumentException_AndDoesNotAdd()
        {
            var ct = CancellationToken.None;
            string email = "user@example.com";
            string password = "ValidPass1!";
            string fullName = "User Name";

            _usersMock
                .Setup(r => r.GetByEmailAsync(email, ct))
                .ReturnsAsync(new User { Email = email });

            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.RegisterAsync(email, password, fullName, ct));

            Assert.Contains("Користувач з таким email вже існує", ex.Message);
            _usersMock.Verify(r => r.GetByEmailAsync(email, ct), Times.Once);
            _usersMock.Verify(r => r.AddAsync(It.IsAny<User>(), ct), Times.Never);
        }

        [Fact]
        public async Task RegisterAsync_NewEmail_AddsUserWithHashedPasswordAndZeroPoints_AndReturnsDto()
        {
            var ct = CancellationToken.None;
            string email = "newuser@example.com";
            string password = "ValidPass1!";
            string fullName = "New User";

            _usersMock
                .Setup(r => r.GetByEmailAsync(email, ct))
                .ReturnsAsync((User?)null);

            User? capturedUser = null;

            var savedUser = new User
            {
                UserId = 42,
                Email = email,
                FullName = fullName,
                Points = 0
            };

            _usersMock
                .Setup(r => r.AddAsync(It.IsAny<User>(), ct))
                .Callback<User, CancellationToken>((u, _) =>
                {
                    capturedUser = u;
                })
                .ReturnsAsync(savedUser);

            _ratingMock
                .Setup(r => r.GetRank(It.IsAny<int>()))
                .Returns((int pts) => UserRank.Newbie);

            _ratingMock
                .Setup(r => r.GetRankLabel(It.IsAny<UserRank>()))
                .Returns((UserRank _) => "Початківець 👶");

            var utcBefore = DateTime.UtcNow;

            var dto = await _service.RegisterAsync(email, password, fullName, ct);

            var utcAfter = DateTime.UtcNow;

            // репозиторій викликано як треба
            _usersMock.Verify(r => r.GetByEmailAsync(email, ct), Times.Once);
            _usersMock.Verify(r => r.AddAsync(It.IsAny<User>(), ct), Times.Once);

            Assert.NotNull(capturedUser);
            Assert.Equal(email, capturedUser!.Email);
            Assert.Equal(fullName, capturedUser.FullName);
            Assert.Equal(0, capturedUser.Points);

            // пароль захешовано
            Assert.NotNull(capturedUser.PasswordHash);
            Assert.NotEqual(password, capturedUser.PasswordHash);
            Assert.True(PasswordHelper.VerifyPassword(password, capturedUser.PasswordHash));

            // CreatedAt в адекватних межах
            Assert.True(capturedUser.CreatedAt >= utcBefore &&
                        capturedUser.CreatedAt <= utcAfter);

            // перевіряємо повернений DTO
            Assert.Equal(savedUser.UserId, dto.Id);
            Assert.Equal(email, dto.Email);
            Assert.Equal(fullName, dto.FullName);
            Assert.Equal(0, dto.Points);
            Assert.Equal("Початківець 👶", dto.RankLabel);
        }

        // LoginAsync

        [Fact]
        public async Task LoginAsync_UserNotFound_ThrowsKeyNotFoundException()
        {
            var ct = CancellationToken.None;
            string email = "missing@example.com";
            string password = "ValidPass1!";

            _usersMock
                .Setup(r => r.GetByEmailAsync(email, ct))
                .ReturnsAsync((User?)null);

            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.LoginAsync(email, password, ct));

            Assert.Contains("Некоректний email або пароль", ex.Message);
            _usersMock.Verify(r => r.GetByEmailAsync(email, ct), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_WrongPassword_ThrowsKeyNotFoundException()
        {
            var ct = CancellationToken.None;
            string email = "user@example.com";
            string correctPassword = "CorrectPass1!";
            string wrongPassword = "WrongPass1!";

            var storedUser = new User
            {
                UserId = 7,
                Email = email,
                FullName = "User",
                PasswordHash = PasswordHelper.HashPassword(correctPassword),
                Points = 10
            };

            _usersMock
                .Setup(r => r.GetByEmailAsync(email, ct))
                .ReturnsAsync(storedUser);

            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.LoginAsync(email, wrongPassword, ct));

            Assert.Contains("Некоректний email або пароль", ex.Message);
            _usersMock.Verify(r => r.GetByEmailAsync(email, ct), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_CorrectCredentials_ReturnsMappedDto()
        {
            var ct = CancellationToken.None;
            string email = "user@example.com";
            string password = "CorrectPass1!";

            var storedUser = new User
            {
                UserId = 99,
                Email = email,
                FullName = "Login User",
                PasswordHash = PasswordHelper.HashPassword(password),
                Points = 123
            };

            _usersMock
                .Setup(r => r.GetByEmailAsync(email, ct))
                .ReturnsAsync(storedUser);

            _ratingMock
                .Setup(r => r.GetRank(It.IsAny<int>()))
                .Returns((int pts) =>
                {
                    Assert.Equal(storedUser.Points, pts);
                    return UserRank.Expert;
                });

            _ratingMock
                .Setup(r => r.GetRankLabel(It.IsAny<UserRank>()))
                .Returns((UserRank rank) =>
                {
                    Assert.Equal(UserRank.Expert, rank);
                    return "Експерт ⭐";
                });

            var dto = await _service.LoginAsync(email, password, ct);

            _usersMock.Verify(r => r.GetByEmailAsync(email, ct), Times.Once);
            _ratingMock.Verify(r => r.GetRank(It.IsAny<int>()), Times.AtLeastOnce);
            _ratingMock.Verify(r => r.GetRankLabel(It.IsAny<UserRank>()), Times.AtLeastOnce);

            Assert.Equal(storedUser.UserId, dto.Id);
            Assert.Equal(storedUser.Email, dto.Email);
            Assert.Equal(storedUser.FullName, dto.FullName);
            Assert.Equal(storedUser.Points, dto.Points);
            Assert.Equal("Експерт ⭐", dto.RankLabel);
        }

        [Fact]
        public async Task LoginAsync_CallsRepositoryWithSameEmailAndToken()
        {
            var cts = new CancellationTokenSource();
            var ct = cts.Token;
            string email = "check@example.com";
            string password = "ValidPass1!";

            var storedUser = new User
            {
                UserId = 1,
                Email = email,
                FullName = "Check User",
                PasswordHash = PasswordHelper.HashPassword(password),
                Points = 0
            };

            _usersMock
                .Setup(r => r.GetByEmailAsync(email, ct))
                .ReturnsAsync(storedUser);

            _ratingMock
                .Setup(r => r.GetRank(It.IsAny<int>()))
                .Returns(UserRank.Newbie);
            _ratingMock
                .Setup(r => r.GetRankLabel(It.IsAny<UserRank>()))
                .Returns("Початківець 👶");

            var dto = await _service.LoginAsync(email, password, ct);

            _usersMock.Verify(r => r.GetByEmailAsync(email, ct), Times.Once);
        }
    }
}
