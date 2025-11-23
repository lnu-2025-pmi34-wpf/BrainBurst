namespace BrainBurst.BLL.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
    /// Містить юніт-тести для <see cref="UserService"/>.
    /// </summary>
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _usersMock;
        private readonly Mock<IRatingService> _ratingMock;
        private readonly UserService _service;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserServiceTests"/> class.
        /// налаштовуючи "моки" (заглушки) для <see cref="IUserRepository"/> та <see cref="IRatingService"/>.
        /// </summary>
        public UserServiceTests()
        {
            this._usersMock = new Mock<IUserRepository>(MockBehavior.Strict);
            this._ratingMock = new Mock<IRatingService>(MockBehavior.Strict);

            this._service = new UserService(this._usersMock.Object, this._ratingMock.Object);
        }

        /// <summary>
        /// Тест: DeleteAccountAsync викликає метод DeleteAsync репозиторію з коректними ID та токеном.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DeleteAccountAsync_CallsRepositoryWithSameIdAndToken()
        {
            int userId = 123;
            var ct = CancellationToken.None;

            this._usersMock
                .Setup(r => r.DeleteAsync(userId, ct))
                .Returns(Task.CompletedTask);

            await this._service.DeleteAccountAsync(userId, ct);

            this._usersMock.Verify(r => r.DeleteAsync(userId, ct), Times.Once);
        }

        /// <summary>
        /// Тест: ChangePasswordAsync кидає ArgumentException, якщо старий та новий паролі однакові.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ChangePasswordAsync_SameOldAndNew_ThrowsArgumentException()
        {
            int userId = 1;
            string password = "OldPass123!";
            var ct = CancellationToken.None;

            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                this._service.ChangePasswordAsync(userId, password, password, ct));

            Assert.Contains("Новий пароль повинен відрізнятися від старого", ex.Message);
        }

        /// <summary>
        /// Тест: ChangePasswordAsync кидає ArgumentException і не викликає UpdateAsync, якщо старий пароль невірний.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ChangePasswordAsync_WrongOldPassword_ThrowsArgumentExceptionAndDoesNotUpdate()
        {
            int userId = 2;
            string oldPassword = "OldPass123!";
            string newPassword = "NewPass456!";
            var ct = CancellationToken.None;

            var storedUser = new User
            {
                UserId = userId,
                PasswordHash = PasswordHelper.HashPassword("SomeOtherPassword123!"),
            };

            this._usersMock
                .Setup(r => r.GetByIdAsync(userId, ct))
                .ReturnsAsync(storedUser);

            this._usersMock
                .Setup(r => r.UpdateAsync(It.IsAny<User>(), ct))
                .Returns(Task.CompletedTask);

            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                this._service.ChangePasswordAsync(userId, oldPassword, newPassword, ct));

            Assert.Contains("Неправильний старий пароль", ex.Message);

            this._usersMock.Verify(r => r.GetByIdAsync(userId, ct), Times.Once);
            this._usersMock.Verify(r => r.UpdateAsync(It.IsAny<User>(), ct), Times.Never);
        }

        /// <summary>
        /// Тест: ChangePasswordAsync при правильному старому паролі оновлює хеш та викликає UpdateAsync.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ChangePasswordAsync_CorrectOldPassword_UpdatesHashAndCallsUpdate()
        {
            int userId = 3;
            string oldPassword = "OldPass123!";
            string newPassword = "NewPass456!";
            var ct = CancellationToken.None;

            var initialHash = PasswordHelper.HashPassword(oldPassword);

            var user = new User
            {
                UserId = userId,
                PasswordHash = initialHash,
            };

            this._usersMock
                .Setup(r => r.GetByIdAsync(userId, ct))
                .ReturnsAsync(user);

            User? updatedUser = null;

            this._usersMock
                .Setup(r => r.UpdateAsync(It.IsAny<User>(), ct))
                .Callback<User, CancellationToken>((u, _) =>
                {
                    updatedUser = u;
                })
                .Returns(Task.CompletedTask);

            await this._service.ChangePasswordAsync(userId, oldPassword, newPassword, ct);

            this._usersMock.Verify(r => r.GetByIdAsync(userId, ct), Times.Once);
            this._usersMock.Verify(r => r.UpdateAsync(It.IsAny<User>(), ct), Times.Once);

            Assert.NotNull(updatedUser);
            Assert.Equal(userId, updatedUser!.UserId);
            Assert.NotEqual(initialHash, updatedUser.PasswordHash);
            Assert.True(PasswordHelper.VerifyPassword(newPassword, updatedUser.PasswordHash));
        }

        /// <summary>
        /// Тест: GetAsync повертає DTO користувача, коректно замаплений разом із рейтингом.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetAsync_ReturnsDtoMappedWithRating()
        {
            int userId = 10;
            var ct = CancellationToken.None;

            var user = new User
            {
                UserId = userId,
                Email = "user@example.com",
                FullName = "Test User",
                Points = 1234,
            };

            this._usersMock
                .Setup(r => r.GetByIdAsync(userId, ct))
                .ReturnsAsync(user);

            this._ratingMock
                .Setup(r => r.GetRank(It.IsAny<int>()))
                .Returns((int pts) =>
                {
                    Assert.Equal(user.Points, pts);
                    return UserRank.Expert;
                });

            this._ratingMock
                .Setup(r => r.GetRankLabel(It.IsAny<UserRank>()))
                .Returns((UserRank rank) =>
                {
                    Assert.Equal(UserRank.Expert, rank);
                    return "Експерт ⭐";
                });

            var dto = await this._service.GetAsync(userId, ct);

            Assert.NotNull(dto);
            Assert.Equal(userId, dto.Id);
            Assert.Equal(user.Email, dto.Email);
            Assert.Equal(user.FullName, dto.FullName);
            Assert.Equal(user.Points, dto.Points);
            Assert.Equal("Експерт ⭐", dto.RankLabel);

            this._usersMock.Verify(r => r.GetByIdAsync(userId, ct), Times.Once);
            this._ratingMock.Verify(r => r.GetRank(It.IsAny<int>()), Times.AtLeastOnce);
            this._ratingMock.Verify(r => r.GetRankLabel(It.IsAny<UserRank>()), Times.AtLeastOnce);
        }

        /// <summary>
        /// Тест: UpdateProfileAsync не викликає UpdateAsync, якщо ім'я користувача не змінилося.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task UpdateProfileAsync_FullNameSame_DoesNotCallUpdateAndReturnsCurrentDto()
        {
            int userId = 5;
            var ct = CancellationToken.None;
            string fullName = "Same Name";

            var user = new User
            {
                UserId = userId,
                Email = "same@example.com",
                FullName = fullName,
                Points = 200,
            };

            this._usersMock
                .Setup(r => r.GetByIdAsync(userId, ct))
                .ReturnsAsync(user);

            this._ratingMock
                .Setup(r => r.GetRank(user.Points))
                .Returns(UserRank.Enthusiast);

            this._ratingMock
                .Setup(r => r.GetRankLabel(UserRank.Enthusiast))
                .Returns("Ентузіаст 👍");

            this._usersMock
                .Setup(r => r.UpdateAsync(It.IsAny<User>(), ct))
                .Returns(Task.CompletedTask);

            var dto = await this._service.UpdateProfileAsync(userId, fullName, ct);

            Assert.Equal(fullName, dto.FullName);
            Assert.Equal(user.Points, dto.Points);
            Assert.Equal("Ентузіаст 👍", dto.RankLabel);

            this._usersMock.Verify(r => r.GetByIdAsync(userId, ct), Times.Once);
            this._usersMock.Verify(r => r.UpdateAsync(It.IsAny<User>(), ct), Times.Never);
        }

        /// <summary>
        /// Тест: UpdateProfileAsync викликає UpdateAsync та повертає оновлений DTO, якщо ім'я користувача змінилося.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task UpdateProfileAsync_FullNameChanged_UpdatesAndReturnsNewDto()
        {
            int userId = 6;
            var ct = CancellationToken.None;
            string oldName = "Old Name";
            string newName = "New Name";

            var user = new User
            {
                UserId = userId,
                Email = "user6@example.com",
                FullName = oldName,
                Points = 500,
            };

            this._usersMock
                .Setup(r => r.GetByIdAsync(userId, ct))
                .ReturnsAsync(user);

            this._usersMock
                .Setup(r => r.UpdateAsync(It.IsAny<User>(), ct))
                .Returns(Task.CompletedTask);

            this._ratingMock
                .Setup(r => r.GetRank(user.Points))
                .Returns(UserRank.Specialist);

            this._ratingMock
                .Setup(r => r.GetRankLabel(UserRank.Specialist))
                .Returns("Спеціаліст 🛠");

            var dto = await this._service.UpdateProfileAsync(userId, newName, ct);

            this._usersMock.Verify(r => r.GetByIdAsync(userId, ct), Times.Once);
            this._usersMock.Verify(
                r => r.UpdateAsync(
                It.Is<User>(u => u.UserId == userId && u.FullName == newName), ct), Times.Once);

            Assert.Equal(newName, dto.FullName);
            Assert.Equal("Спеціаліст 🛠", dto.RankLabel);
        }

        /// <summary>
        /// Тест: GetLeaderboardAsync повертає порожній список, якщо репозиторій не повертає користувачів.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetLeaderboardAsync_EmptyList_ReturnsEmpty()
        {
            int top = 5;
            var ct = CancellationToken.None;

            this._usersMock
                .Setup(r => r.GetTopAsync(top, ct))
                .ReturnsAsync(new List<User>());

            var result = await this._service.GetLeaderboardAsync(top, ct);

            Assert.NotNull(result);
            Assert.Empty(result);

            this._usersMock.Verify(r => r.GetTopAsync(top, ct), Times.Once);
        }

        /// <summary>
        /// Тест: GetLeaderboardAsync мапить користувачів у RankingEntries, використовуючи FullName або Email та рейтинг.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetLeaderboardAsync_MapsUsersToRankingEntries_UsesFullNameOrEmailAndRating()
        {
            int top = 3;
            var ct = CancellationToken.None;

            var users = new List<User>
            {
                new User { UserId = 1, FullName = "User One", Email = "one@example.com", Points = 100 },
                new User { UserId = 2, FullName = null,       Email = "two@example.com", Points = 1000 },
            };

            this._usersMock
                .Setup(r => r.GetTopAsync(top, ct))
                .ReturnsAsync(users);

            this._ratingMock
                .Setup(r => r.GetRank(100))
                .Returns(UserRank.Enthusiast);
            this._ratingMock
                .Setup(r => r.GetRankLabel(UserRank.Enthusiast))
                .Returns("Ентузіаст 👍");

            this._ratingMock
                .Setup(r => r.GetRank(1000))
                .Returns(UserRank.Expert);
            this._ratingMock
                .Setup(r => r.GetRankLabel(UserRank.Expert))
                .Returns("Експерт ⭐");

            var leaderboard = await this._service.GetLeaderboardAsync(top, ct);

            Assert.Equal(2, leaderboard.Count);

            var e1 = leaderboard[0];
            Assert.Equal(1, e1.UserId);
            Assert.Equal("User One", e1.FullName);
            Assert.Equal(100, e1.Points);
            Assert.Equal("Ентузіаст 👍", e1.Rank);

            var e2 = leaderboard[1];
            Assert.Equal(2, e2.UserId);
            Assert.Equal("two@example.com", e2.FullName);
            Assert.Equal(1000, e2.Points);
            Assert.Equal("Експерт ⭐", e2.Rank);

            this._usersMock.Verify(r => r.GetTopAsync(top, ct), Times.Once);
            this._ratingMock.Verify(r => r.GetRank(100), Times.Once);
            this._ratingMock.Verify(r => r.GetRank(1000), Times.Once);
        }
    }
}
