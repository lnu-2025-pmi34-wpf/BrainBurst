namespace BrainBurst.BLL.Tests.Services
{
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

    /// <summary>
    /// Містить юніт-тести для <see cref="AuthService"/>.
    /// </summary>
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _usersMock;
        private readonly Mock<IRatingService> _ratingMock;
        private readonly AuthService _service;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthServiceTests"/> class.
        /// налаштовуючи "моки" (заглушки) для <see cref="IUserRepository"/> та <see cref="IRatingService"/>.
        /// </summary>
        public AuthServiceTests()
        {
            this._usersMock = new Mock<IUserRepository>(MockBehavior.Strict);
            this._ratingMock = new Mock<IRatingService>(MockBehavior.Strict);

            this._service = new AuthService(this._usersMock.Object, this._ratingMock.Object);
        }

        /// <summary>
        /// Тест: RegisterAsync кидає ArgumentException, якщо email вже існує, і не додає нового користувача.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task RegisterAsync_EmailAlreadyExists_ThrowsArgumentException_AndDoesNotAdd()
        {
            var ct = CancellationToken.None;
            string email = "user@example.com";
            string password = "ValidPass1!";
            string fullName = "User Name";

            this._usersMock
                .Setup(r => r.GetByEmailAsync(email, ct))
                .ReturnsAsync(new User { Email = email });

            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                this._service.RegisterAsync(email, password, fullName, ct));

            Assert.Contains("Користувач з таким email вже існує", ex.Message);
            this._usersMock.Verify(r => r.GetByEmailAsync(email, ct), Times.Once);
            this._usersMock.Verify(r => r.AddAsync(It.IsAny<User>(), ct), Times.Never);
        }

        /// <summary>
        /// Тест: RegisterAsync коректно створює нового користувача з захешованим паролем та нульовими балами, і повертає DTO.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task RegisterAsync_NewEmail_AddsUserWithHashedPasswordAndZeroPoints_AndReturnsDto()
        {
            var ct = CancellationToken.None;
            string email = "newuser@example.com";
            string password = "ValidPass1!";
            string fullName = "New User";

            this._usersMock
                .Setup(r => r.GetByEmailAsync(email, ct))
                .ReturnsAsync((User?)null);

            User? capturedUser = null;

            var savedUser = new User
            {
                UserId = 42,
                Email = email,
                FullName = fullName,
                Points = 0,
            };

            this._usersMock
                .Setup(r => r.AddAsync(It.IsAny<User>(), ct))
                .Callback<User, CancellationToken>((u, _) =>
                {
                    capturedUser = u;
                })
                .ReturnsAsync(savedUser);

            this._ratingMock
                .Setup(r => r.GetRank(It.IsAny<int>()))
                .Returns((int pts) => UserRank.Newbie);

            this._ratingMock
                .Setup(r => r.GetRankLabel(It.IsAny<UserRank>()))
                .Returns((UserRank _) => "Початківець 👶");

            var utcBefore = DateTime.UtcNow;

            var dto = await this._service.RegisterAsync(email, password, fullName, ct);

            var utcAfter = DateTime.UtcNow;

            this._usersMock.Verify(r => r.GetByEmailAsync(email, ct), Times.Once);
            this._usersMock.Verify(r => r.AddAsync(It.IsAny<User>(), ct), Times.Once);

            Assert.NotNull(capturedUser);
            Assert.Equal(email, capturedUser!.Email);
            Assert.Equal(fullName, capturedUser.FullName);
            Assert.Equal(0, capturedUser.Points);

            Assert.NotNull(capturedUser.PasswordHash);
            Assert.NotEqual(password, capturedUser.PasswordHash);
            Assert.True(PasswordHelper.VerifyPassword(password, capturedUser.PasswordHash));

            Assert.True(capturedUser.CreatedAt >= utcBefore &&
                        capturedUser.CreatedAt <= utcAfter);

            Assert.Equal(savedUser.UserId, dto.Id);
            Assert.Equal(email, dto.Email);
            Assert.Equal(fullName, dto.FullName);
            Assert.Equal(0, dto.Points);
            Assert.Equal("Початківець 👶", dto.RankLabel);
        }

        /// <summary>
        /// Тест: LoginAsync кидає KeyNotFoundException, якщо користувача з таким email не знайдено.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task LoginAsync_UserNotFound_ThrowsKeyNotFoundException()
        {
            var ct = CancellationToken.None;
            string email = "missing@example.com";
            string password = "ValidPass1!";

            this._usersMock
                .Setup(r => r.GetByEmailAsync(email, ct))
                .ReturnsAsync((User?)null);

            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                this._service.LoginAsync(email, password, ct));

            Assert.Contains("Некоректний email або пароль", ex.Message);
            this._usersMock.Verify(r => r.GetByEmailAsync(email, ct), Times.Once);
        }

        /// <summary>
        /// Тест: LoginAsync кидає KeyNotFoundException, якщо пароль введено неправильно.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
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
                Points = 10,
            };

            this._usersMock
                .Setup(r => r.GetByEmailAsync(email, ct))
                .ReturnsAsync(storedUser);

            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                this._service.LoginAsync(email, wrongPassword, ct));

            Assert.Contains("Некоректний email або пароль", ex.Message);
            this._usersMock.Verify(r => r.GetByEmailAsync(email, ct), Times.Once);
        }

        /// <summary>
        /// Тест: LoginAsync повертає коректно замаплений DTO при правильних email та паролі.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
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
                Points = 123,
            };

            this._usersMock
                .Setup(r => r.GetByEmailAsync(email, ct))
                .ReturnsAsync(storedUser);

            this._ratingMock
                .Setup(r => r.GetRank(It.IsAny<int>()))
                .Returns((int pts) =>
                {
                    Assert.Equal(storedUser.Points, pts);
                    return UserRank.Expert;
                });

            this._ratingMock
                .Setup(r => r.GetRankLabel(It.IsAny<UserRank>()))
                .Returns((UserRank rank) =>
                {
                    Assert.Equal(UserRank.Expert, rank);
                    return "Експерт ⭐";
                });

            var dto = await this._service.LoginAsync(email, password, ct);

            this._usersMock.Verify(r => r.GetByEmailAsync(email, ct), Times.Once);
            this._ratingMock.Verify(r => r.GetRank(It.IsAny<int>()), Times.AtLeastOnce);
            this._ratingMock.Verify(r => r.GetRankLabel(It.IsAny<UserRank>()), Times.AtLeastOnce);

            Assert.Equal(storedUser.UserId, dto.Id);
            Assert.Equal(storedUser.Email, dto.Email);
            Assert.Equal(storedUser.FullName, dto.FullName);
            Assert.Equal(storedUser.Points, dto.Points);
            Assert.Equal("Експерт ⭐", dto.RankLabel);
        }

        /// <summary>
        /// Тест: LoginAsync викликає репозиторій з тими ж email та CancellationToken, що були передані.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
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
                Points = 0,
            };

            this._usersMock
                .Setup(r => r.GetByEmailAsync(email, ct))
                .ReturnsAsync(storedUser);

            this._ratingMock
                .Setup(r => r.GetRank(It.IsAny<int>()))
                .Returns(UserRank.Newbie);
            this._ratingMock
                .Setup(r => r.GetRankLabel(It.IsAny<UserRank>()))
                .Returns("Початківець 👶");

            var dto = await this._service.LoginAsync(email, password, ct);

            this._usersMock.Verify(r => r.GetByEmailAsync(email, ct), Times.Once);
        }
    }
}
