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
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// Містить юніт-тести для <see cref="AuthService"/>.
    /// </summary>
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> userRepositoryMock;
        private readonly Mock<IRatingService> ratingServiceMock;
        private readonly Mock<ILogger<AuthService>> loggerMock;
        private readonly AuthService service;
        private readonly CancellationToken ct = CancellationToken.None;

        /// <summary>
        /// Конструктор тестового класу. Налаштовує "моки" для залежностей <see cref="AuthService"/>.
        /// </summary>
        public AuthServiceTests()
        {
            this.userRepositoryMock = new Mock<IUserRepository>(MockBehavior.Strict);
            this.ratingServiceMock = new Mock<IRatingService>(MockBehavior.Strict);
            this.loggerMock = new Mock<ILogger<AuthService>>(MockBehavior.Loose);

            this.ratingServiceMock
                .Setup(r => r.GetRank(It.IsAny<int>()))
                .Returns(UserRank.Newbie);

            this.ratingServiceMock
                .Setup(r => r.GetRankLabel(It.IsAny<UserRank>()))
                .Returns("Початківець");

            this.service = new AuthService(
                this.userRepositoryMock.Object,
                this.ratingServiceMock.Object,
                this.loggerMock.Object);
        }

        /// <summary>
        /// Тест: RegisterAsync при валідних даних створює нового користувача
        /// і повертає коректний <see cref="UserDTO"/>.
        /// </summary>
        [Fact]
        public async Task RegisterAsync_ValidData_CreatesUserAndReturnsDto()
        {
            // arrange
            string email = "test@example.com";
            string password = "StrongPass123!";
            string fullName = "Test User";

            this.userRepositoryMock
                .Setup(r => r.GetByEmailAsync(email, this.ct))
                .ReturnsAsync((User?)null);

            this.userRepositoryMock
                .Setup(r => r.AddAsync(It.IsAny<User>(), this.ct))
                .ReturnsAsync((User u, CancellationToken _) =>
                {
                    u.UserId = 42;
                    u.Points = 0;
                    u.CreatedAt = DateTime.UtcNow;
                    return u;
                });

            // act
            UserDTO result = await this.service.RegisterAsync(email, password, fullName, this.ct);

            // assert
            Assert.NotNull(result);
            Assert.Equal(42, result.Id);
            Assert.Equal(email, result.Email);
            Assert.Equal(fullName, result.FullName);
            Assert.Equal(0, result.Points);

            this.userRepositoryMock.Verify(r => r.GetByEmailAsync(email, this.ct), Times.Once);
            this.userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>(), this.ct), Times.Once);
        }

        /// <summary>
        /// Тест: RegisterAsync кидає <see cref="ArgumentException"/>,
        /// якщо користувач з таким email вже існує.
        /// </summary>
        [Fact]
        public async Task RegisterAsync_EmailAlreadyExists_ThrowsArgumentException()
        {
            // arrange
            string email = "exists@example.com";
            string password = "StrongPass123!";
            string fullName = "Existing User";

            this.userRepositoryMock
                .Setup(r => r.GetByEmailAsync(email, this.ct))
                .ReturnsAsync(new User { UserId = 1, Email = email });

            // act & assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                this.service.RegisterAsync(email, password, fullName, this.ct));

            this.userRepositoryMock.Verify(r => r.GetByEmailAsync(email, this.ct), Times.Once);
            this.userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>(), this.ct), Times.Never);
        }

        /// <summary>
        /// Тест: RegisterAsync кидає <see cref="ArgumentException"/>,
        /// якщо email має некоректний формат (валідація Guard.Email).
        /// </summary>
        [Fact]
        public async Task RegisterAsync_InvalidEmail_ThrowsArgumentException()
        {
            // arrange
            string email = "not-an-email";
            string password = "StrongPass123!";
            string fullName = "User";

            // act & assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                this.service.RegisterAsync(email, password, fullName, this.ct));

            this.userRepositoryMock.Verify(
                r => r.GetByEmailAsync(It.IsAny<string>(), this.ct),
                Times.Never);
            this.userRepositoryMock.Verify(
                r => r.AddAsync(It.IsAny<User>(), this.ct),
                Times.Never);
        }

        /// <summary>
        /// Тест: LoginAsync при валідних облікових даних повертає <see cref="UserDTO"/>.
        /// </summary>
        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsUserDto()
        {
            // arrange
            string email = "user@example.com";
            string password = "StrongPass123!";
            string hash = PasswordHelper.HashPassword(password);

            var userEntity = new User
            {
                UserId = 10,
                Email = email,
                FullName = "User Name",
                PasswordHash = hash,
                Points = 150,
                CreatedAt = DateTime.UtcNow,
            };

            this.userRepositoryMock
                .Setup(r => r.GetByEmailAsync(email, this.ct))
                .ReturnsAsync(userEntity);

            // act
            UserDTO result = await this.service.LoginAsync(email, password, this.ct);

            // assert
            Assert.NotNull(result);
            Assert.Equal(10, result.Id);
            Assert.Equal(email, result.Email);
            Assert.Equal("User Name", result.FullName);

            this.userRepositoryMock.Verify(r => r.GetByEmailAsync(email, this.ct), Times.Once);
        }

        /// <summary>
        /// Тест: LoginAsync кидає <see cref="KeyNotFoundException"/>,
        /// якщо користувача з таким email не знайдено.
        /// </summary>
        [Fact]
        public async Task LoginAsync_UserNotFound_ThrowsKeyNotFoundException()
        {
            string email = "missing@example.com";
            string password = "SomePass123!";

            this.userRepositoryMock
                .Setup(r => r.GetByEmailAsync(email, this.ct))
                .ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                this.service.LoginAsync(email, password, this.ct));

            this.userRepositoryMock.Verify(r => r.GetByEmailAsync(email, this.ct), Times.Once);
        }

        /// <summary>
        /// Тест: LoginAsync кидає <see cref="KeyNotFoundException"/>,
        /// якщо пароль некоректний.
        /// </summary>
        [Fact]
        public async Task LoginAsync_InvalidPassword_ThrowsKeyNotFoundException()
        {
            // arrange
            string email = "user@example.com";
            string correctPassword = "Correct123!";
            string wrongPassword = "Wrong999!";
            string hash = PasswordHelper.HashPassword(correctPassword);

            var userEntity = new User
            {
                UserId = 11,
                Email = email,
                FullName = "User Name",
                PasswordHash = hash,
                Points = 0,
                CreatedAt = DateTime.UtcNow,
            };

            this.userRepositoryMock
                .Setup(r => r.GetByEmailAsync(email, this.ct))
                .ReturnsAsync(userEntity);

            // act & assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                this.service.LoginAsync(email, wrongPassword, this.ct));

            this.userRepositoryMock.Verify(r => r.GetByEmailAsync(email, this.ct), Times.Once);
        }
    }
}