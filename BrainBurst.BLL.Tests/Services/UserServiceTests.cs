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
    /// Юніт-тести для UserService.
    /// </summary>
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _usersMock;
        private readonly Mock<IRatingService> _ratingMock;
        private readonly Mock<ILogger<UserService>> _loggerMock;

        private readonly UserService _service;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserServiceTests"/> class.
        /// </summary>
        public UserServiceTests()
        {
            this._usersMock = new Mock<IUserRepository>(MockBehavior.Strict);
            this._ratingMock = new Mock<IRatingService>(MockBehavior.Strict);
            this._loggerMock = new Mock<ILogger<UserService>>();

            this._service = new UserService(
                this._usersMock.Object,
                this._ratingMock.Object,
                this._loggerMock.Object);
        }

        // ============================================================================
        // GetAsync
        // ============================================================================

        /// <summary>
        /// GetAsync_Found_ReturnUserDto.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Fact]
        public async Task GetAsync_Found_ReturnsUserDto()
        {
            var ct = CancellationToken.None;

            var user = new User
            {
                UserId = 10,
                Email = "test@example.com",
                FullName = "John",
                Points = 100,
            };

            this._usersMock.Setup(r => r.GetByIdAsync(10, ct))
                      .ReturnsAsync(user);

            this._ratingMock.Setup(r => r.GetRank(user.Points))
                       .Returns(UserRank.Enthusiast);

            this._ratingMock.Setup(r => r.GetRankLabel(UserRank.Enthusiast))
                       .Returns("Ентузіаст");

            var dto = await this._service.GetAsync(10, ct);

            Assert.Equal(10, dto.Id);
            Assert.Equal("John", dto.FullName);
            Assert.Equal("Ентузіаст", dto.RankLabel);
        }

        // ============================================================================
        // UpdateProfileAsync
        // ============================================================================

        /// <summary>
        /// UpdateProfileAsync_UpdatesFullName.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Fact]
        public async Task UpdateProfileAsync_UpdatesFullName()
        {
            var ct = CancellationToken.None;

            var user = new User
            {
                UserId = 5,
                FullName = "Old Name",
                Points = 0,
            };

            this._usersMock.Setup(r => r.GetByIdAsync(5, ct))
                    .ReturnsAsync(user);

            this._usersMock.Setup(r => r.UpdateAsync(It.IsAny<User>(), ct))
                    .Returns(Task.CompletedTask);

            // ⭐ Додано — щоб ToDTO не впав
            this._ratingMock.Setup(r => r.GetRank(It.IsAny<int>()))
                    .Returns(UserRank.Enthusiast);

            this._ratingMock.Setup(r => r.GetRankLabel(UserRank.Enthusiast))
                    .Returns("Ентузіаст");

            var result = await this._service.UpdateProfileAsync(5, "New Name", ct);

            Assert.Equal("New Name", result.FullName);
        }

        /// <summary>
        /// UpdateProfileAsync_DoesNothing_WhenSameName.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Fact]
        public async Task UpdateProfileAsync_DoesNothing_WhenSameName()
        {
            var ct = CancellationToken.None;

            var user = new User
            {
                UserId = 5,
                FullName = "SameName",
                Points = 0,
            };

            this._usersMock.Setup(r => r.GetByIdAsync(5, ct))
                    .ReturnsAsync(user);

            // ⭐ Потрібно додати навіть якщо UpdateAsync не викликається
            this._ratingMock.Setup(r => r.GetRank(It.IsAny<int>()))
                    .Returns(UserRank.Enthusiast);

            this._ratingMock.Setup(r => r.GetRankLabel(UserRank.Enthusiast))
                    .Returns("Ентузіаст");

            var result = await this._service.UpdateProfileAsync(5, "SameName", ct);

            Assert.Equal("SameName", result.FullName);
            this._usersMock.Verify(r => r.UpdateAsync(It.IsAny<User>(), ct), Times.Never);
        }

        // ============================================================================
        // DeleteAccountAsync
        // ============================================================================

        /// <summary>
        /// DeleteAccountAsync_CallsRepository.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Fact]
        public async Task DeleteAccountAsync_CallsRepository()
        {
            var ct = CancellationToken.None;

            this._usersMock.Setup(r => r.DeleteAsync(3, ct))
                      .Returns(Task.CompletedTask);

            await this._service.DeleteAccountAsync(3, ct);

            this._usersMock.Verify(r => r.DeleteAsync(3, ct), Times.Once);
        }

        // ============================================================================
        // ChangePasswordAsync
        // ============================================================================

        /// <summary>
        /// ChangePasswordAsync_ValidOldPassword_ChangesPassword.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Fact]
        public async Task ChangePasswordAsync_ValidOldPassword_ChangesPassword()
        {
            var ct = CancellationToken.None;

            string oldPassword = "CorrectOld1";
            string newPassword = "NewPass123";

            var user = new User
            {
                UserId = 1,
                PasswordHash = PasswordHelper.HashPassword(oldPassword),
            };

            this._usersMock.Setup(r => r.GetByIdAsync(1, ct))
                      .ReturnsAsync(user);

            this._usersMock.Setup(r => r.UpdateAsync(It.IsAny<User>(), ct))
                      .Returns(Task.CompletedTask);

            await this._service.ChangePasswordAsync(1, oldPassword, newPassword, ct);

            Assert.True(PasswordHelper.VerifyPassword(newPassword, user.PasswordHash));
        }

        /// <summary>
        /// ChangePasswordAsync_NewEqualsOld_ThrowsArgumentException.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Fact]
        public async Task ChangePasswordAsync_NewEqualsOld_ThrowsArgumentException()
        {
            var ct = CancellationToken.None;

            string pass = "Password123";

            var user = new User
            {
                UserId = 2,
                PasswordHash = PasswordHelper.HashPassword(pass),
            };

            this._usersMock.Setup(r => r.GetByIdAsync(2, ct))
                      .ReturnsAsync(user);

            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                this._service.ChangePasswordAsync(2, pass, pass, ct));

            Assert.Contains("Новий пароль повинен відрізнятися", ex.Message);
        }

        /// <summary>
        /// ChangePasswordAsync_WrongOldPassword_ThrowsArgumentException.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Fact]
        public async Task ChangePasswordAsync_WrongOldPassword_ThrowsArgumentException()
        {
            var ct = CancellationToken.None;

            string correctPassword = "CorrectOld1";
            string wrongPassword = "WrongOld1";

            var user = new User
            {
                UserId = 3,
                PasswordHash = PasswordHelper.HashPassword(correctPassword),
            };

            this._usersMock.Setup(r => r.GetByIdAsync(3, ct))
                      .ReturnsAsync(user);

            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                this._service.ChangePasswordAsync(3, wrongPassword, "NewPass123", ct));

            Assert.Contains("Неправильний старий пароль", ex.Message);
        }

        // ============================================================================
        // GetLeaderboardAsync
        // ============================================================================

        /// <summary>
        /// GetLeaderboardAsync_ReturnsCorrectRanking.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Fact]
        public async Task GetLeaderboardAsync_ReturnsCorrectRanking()
        {
            var ct = CancellationToken.None;
            int top = 3;

            var users = new List<User>
            {
                new User { UserId = 1, FullName = "A", Points = 100 },
                new User { UserId = 2, FullName = "B", Points = 200 },
            };

            this._usersMock.Setup(r => r.GetTopAsync(top, ct))
                      .ReturnsAsync(users);

            this._ratingMock.Setup(r => r.GetRank(100)).Returns(UserRank.Enthusiast);
            this._ratingMock.Setup(r => r.GetRankLabel(UserRank.Enthusiast)).Returns("Ентузіаст 👍");

            this._ratingMock.Setup(r => r.GetRank(200)).Returns(UserRank.Expert);
            this._ratingMock.Setup(r => r.GetRankLabel(UserRank.Expert)).Returns("Експерт ⭐");

            var result = await this._service.GetLeaderboardAsync(top, ct);

            Assert.Equal(2, result.Count);

            Assert.Equal("A", result[0].FullName);
            Assert.Equal("Ентузіаст 👍", result[0].Rank);

            Assert.Equal("B", result[1].FullName);
            Assert.Equal("Експерт ⭐", result[1].Rank);
        }

        // ============================================================================
        // ДОДАТКОВІ ТЕСТИ ДЛЯ ПОКРИТТЯ ВСІХ ГІЛОК
        // ============================================================================

        // ---------- DeleteAccountAsync: catch (Exception) ----------

        /// <summary>
        /// DeleteAccountAsync_RepositoryThrows_RethrowsException.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Fact]
        public async Task DeleteAccountAsync_RepositoryThrows_RethrowsException()
        {
            var ct = CancellationToken.None;

            this._usersMock.Setup(r => r.DeleteAsync(5, ct))
                      .ThrowsAsync(new InvalidOperationException("DB error"));

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                this._service.DeleteAccountAsync(5, ct));

            this._usersMock.Verify(r => r.DeleteAsync(5, ct), Times.Once);
        }

        // ---------- ChangePasswordAsync: KeyNotFound + generic exception ----------

        /// <summary>
        /// ChangePasswordAsync_UserNotFound_ThrowsKeyNotFoundException.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Fact]
        public async Task ChangePasswordAsync_UserNotFound_ThrowsKeyNotFoundException()
        {
            var ct = CancellationToken.None;

            this._usersMock.Setup(r => r.GetByIdAsync(10, ct))
                      .ThrowsAsync(new KeyNotFoundException("not found"));

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                this._service.ChangePasswordAsync(10, "OldPass123", "NewPass123", ct));
        }

        /// <summary>
        /// ChangePasswordAsync_UpdateFails_LogsErrorAndRethrows.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Fact]
        public async Task ChangePasswordAsync_UpdateFails_LogsErrorAndRethrows()
        {
            var ct = CancellationToken.None;
            string oldPassword = "CorrectOld1";
            string newPassword = "NewPass123";

            var user = new User
            {
                UserId = 11,
                PasswordHash = PasswordHelper.HashPassword(oldPassword),
            };

            this._usersMock.Setup(r => r.GetByIdAsync(11, ct))
                      .ReturnsAsync(user);

            this._usersMock.Setup(r => r.UpdateAsync(user, ct))
                      .ThrowsAsync(new InvalidOperationException("DB update failed"));

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                this._service.ChangePasswordAsync(11, oldPassword, newPassword, ct));

            this._usersMock.Verify(r => r.GetByIdAsync(11, ct), Times.Once);
            this._usersMock.Verify(r => r.UpdateAsync(user, ct), Times.Once);
        }

        // ---------- GetAsync: KeyNotFound + generic exception ----------

        /// <summary>
        /// GetAsync_UserNotFound_ThrowsKeyNotFoundException.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Fact]
        public async Task GetAsync_UserNotFound_ThrowsKeyNotFoundException()
        {
            var ct = CancellationToken.None;

            this._usersMock.Setup(r => r.GetByIdAsync(20, ct))
                      .ThrowsAsync(new KeyNotFoundException("no user"));

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                this._service.GetAsync(20, ct));
        }

        /// <summary>
        /// GetAsync_RepositoryThrowsOtherException_Rethrows.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Fact]
        public async Task GetAsync_RepositoryThrowsOtherException_Rethrows()
        {
            var ct = CancellationToken.None;

            this._usersMock.Setup(r => r.GetByIdAsync(21, ct))
                      .ThrowsAsync(new InvalidOperationException("DB failure"));

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                this._service.GetAsync(21, ct));
        }

        // ---------- UpdateProfileAsync: Guard.Text + generic exception ----------

        /// <summary>
        /// UpdateProfileAsync_InvalidFullName_ThrowsArgumentException.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Fact]
        public async Task UpdateProfileAsync_InvalidFullName_ThrowsArgumentException()
        {
            var ct = CancellationToken.None;

            // некоректне ім'я → Guard.Text має впасти ще до repo
            await Assert.ThrowsAsync<ArgumentException>(() =>
                this._service.UpdateProfileAsync(30, string.Empty, ct));

            this._usersMock.Verify(
                r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        /// <summary>
        /// UpdateProfileAsync_RepositoryThrowsOtherException_Rethrows.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Fact]
        public async Task UpdateProfileAsync_RepositoryThrowsOtherException_Rethrows()
        {
            var ct = CancellationToken.None;

            this._usersMock.Setup(r => r.GetByIdAsync(31, ct))
                      .ThrowsAsync(new InvalidOperationException("DB failure"));

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                this._service.UpdateProfileAsync(31, "Valid Name", ct));

            this._usersMock.Verify(r => r.GetByIdAsync(31, ct), Times.Once);
        }

        // ---------- GetLeaderboardAsync: FullName ?? Email + catch(Exception) ----------

        /// <summary>
        /// GetLeaderboardAsync_UsesEmailWhenFullNameIsNull.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Fact]
        public async Task GetLeaderboardAsync_UsesEmailWhenFullNameIsNull()
        {
            var ct = CancellationToken.None;
            int top = 1;

            var users = new List<User>
            {
                new User
                {
                    UserId = 100,
                    FullName = null,
                    Email = "no-name@example.com",
                    Points = 50,
                },
            };

            this._usersMock.Setup(r => r.GetTopAsync(top, ct))
                      .ReturnsAsync(users);

            this._ratingMock.Setup(r => r.GetRank(50)).Returns(UserRank.Enthusiast);
            this._ratingMock.Setup(r => r.GetRankLabel(UserRank.Enthusiast)).Returns("Ентузіаст");

            var result = await this._service.GetLeaderboardAsync(top, ct);

            var entry = Assert.Single(result);
            Assert.Equal("no-name@example.com", entry.FullName);
            Assert.Equal("Ентузіаст", entry.Rank);
        }

        /// <summary>
        /// GetLeaderboardAsync_RepositoryThrows_Rethrows.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Fact]
        public async Task GetLeaderboardAsync_RepositoryThrows_Rethrows()
        {
            var ct = CancellationToken.None;
            int top = 5;

            this._usersMock.Setup(r => r.GetTopAsync(top, ct))
                      .ThrowsAsync(new InvalidOperationException("DB failure"));

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                this._service.GetLeaderboardAsync(top, ct));

            this._usersMock.Verify(r => r.GetTopAsync(top, ct), Times.Once);
        }
    }
}
