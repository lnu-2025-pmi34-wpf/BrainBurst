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

namespace BrainBurst.BLL.Tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _usersMock;
        private readonly Mock<IRatingService> _ratingMock;
        private readonly UserService _service;

        public UserServiceTests()
        {
            _usersMock  = new Mock<IUserRepository>(MockBehavior.Strict);
            _ratingMock = new Mock<IRatingService>(MockBehavior.Strict);

            _service = new UserService(_usersMock.Object, _ratingMock.Object);
        }

        // DeleteAccountAsync

        [Fact]
        public async Task DeleteAccountAsync_CallsRepositoryWithSameIdAndToken()
        {
            int userId = 123;
            var ct = CancellationToken.None;

            _usersMock
                .Setup(r => r.DeleteAsync(userId, ct))
                .Returns(Task.CompletedTask);

            await _service.DeleteAccountAsync(userId, ct);

            _usersMock.Verify(r => r.DeleteAsync(userId, ct), Times.Once);
        }

        // ChangePasswordAsync

        [Fact]
        public async Task ChangePasswordAsync_SameOldAndNew_ThrowsArgumentException()
        {
            int userId = 1;
            string password = "OldPass123!";
            var ct = CancellationToken.None;

            // щоб до репозиторію навіть не дійшло — не робимо Setup на _users
            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.ChangePasswordAsync(userId, password, password, ct));

            Assert.Contains("Новий пароль повинен відрізнятися від старого", ex.Message);
        }

        [Fact]
        public async Task ChangePasswordAsync_WrongOldPassword_ThrowsArgumentExceptionAndDoesNotUpdate()
        {
            int userId = 2;
            string oldPassword = "OldPass123!";
            string newPassword = "NewPass456!";
            var ct = CancellationToken.None;

            // у БД лежить інший хеш → VerifyPassword поверне false
            var storedUser = new User
            {
                UserId = userId,
                PasswordHash = PasswordHelper.HashPassword("SomeOtherPassword123!")
            };

            _usersMock
                .Setup(r => r.GetByIdAsync(userId, ct))
                .ReturnsAsync(storedUser);

            // UpdateAsync не має викликатися
            _usersMock
                .Setup(r => r.UpdateAsync(It.IsAny<User>(), ct))
                .Returns(Task.CompletedTask);

            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.ChangePasswordAsync(userId, oldPassword, newPassword, ct));

            Assert.Contains("Неправильний старий пароль", ex.Message);

            _usersMock.Verify(r => r.GetByIdAsync(userId, ct), Times.Once);
            _usersMock.Verify(r => r.UpdateAsync(It.IsAny<User>(), ct), Times.Never);
        }

        [Fact]
        public async Task ChangePasswordAsync_CorrectOldPassword_UpdatesHashAndCallsUpdate()
        {
            int userId = 3;
            string oldPassword = "OldPass123!";
            string newPassword = "NewPass456!";
            var ct = CancellationToken.None;

            // хешуємо старий пароль так само, як це зробить прод-код
            var initialHash = PasswordHelper.HashPassword(oldPassword);

            var user = new User
            {
                UserId = userId,
                PasswordHash = initialHash
            };

            _usersMock
                .Setup(r => r.GetByIdAsync(userId, ct))
                .ReturnsAsync(user);

            User? updatedUser = null;

            _usersMock
                .Setup(r => r.UpdateAsync(It.IsAny<User>(), ct))
                .Callback<User, CancellationToken>((u, _) =>
                {
                    updatedUser = u;
                })
                .Returns(Task.CompletedTask);

            await _service.ChangePasswordAsync(userId, oldPassword, newPassword, ct);

            _usersMock.Verify(r => r.GetByIdAsync(userId, ct), Times.Once);
            _usersMock.Verify(r => r.UpdateAsync(It.IsAny<User>(), ct), Times.Once);

            Assert.NotNull(updatedUser);
            Assert.Equal(userId, updatedUser!.UserId);
            Assert.NotEqual(initialHash, updatedUser.PasswordHash);
            Assert.True(PasswordHelper.VerifyPassword(newPassword, updatedUser.PasswordHash));
        }

        // GetAsync

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
                Points = 1234
            };

            _usersMock
                .Setup(r => r.GetByIdAsync(userId, ct))
                .ReturnsAsync(user);

            _ratingMock
                .Setup(r => r.GetRank(It.IsAny<int>()))
                .Returns((int pts) =>
                {
                    // можемо додатково перевірити, що в ToDTO передаються саме user.Points
                    Assert.Equal(user.Points, pts);
                    return UserRank.Expert;
                });

            _ratingMock
                .Setup(r => r.GetRankLabel(It.IsAny<UserRank>()))
                .Returns((UserRank rank) =>
                {
                    // і тут перевіримо, що label береться для того самого рангу
                    Assert.Equal(UserRank.Expert, rank);
                    return "Експерт ⭐";
                });

            var dto = await _service.GetAsync(userId, ct);

            Assert.NotNull(dto);
            Assert.Equal(userId, dto.Id);
            Assert.Equal(user.Email, dto.Email);
            Assert.Equal(user.FullName, dto.FullName);
            Assert.Equal(user.Points, dto.Points);
            Assert.Equal("Експерт ⭐", dto.RankLabel);

            _usersMock.Verify(r => r.GetByIdAsync(userId, ct), Times.Once);
            _ratingMock.Verify(r => r.GetRank(It.IsAny<int>()), Times.AtLeastOnce);
            _ratingMock.Verify(r => r.GetRankLabel(It.IsAny<UserRank>()), Times.AtLeastOnce);
        }

        // UpdateProfileAsync

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
                Points = 200
            };

            _usersMock
                .Setup(r => r.GetByIdAsync(userId, ct))
                .ReturnsAsync(user);

            _ratingMock
                .Setup(r => r.GetRank(user.Points))
                .Returns(UserRank.Enthusiast);

            _ratingMock
                .Setup(r => r.GetRankLabel(UserRank.Enthusiast))
                .Returns("Ентузіаст 👍");

            // UpdateAsync не повинен викликатися
            _usersMock
                .Setup(r => r.UpdateAsync(It.IsAny<User>(), ct))
                .Returns(Task.CompletedTask);

            var dto = await _service.UpdateProfileAsync(userId, fullName, ct);

            Assert.Equal(fullName, dto.FullName);
            Assert.Equal(user.Points, dto.Points);
            Assert.Equal("Ентузіаст 👍", dto.RankLabel);

            _usersMock.Verify(r => r.GetByIdAsync(userId, ct), Times.Once);
            _usersMock.Verify(r => r.UpdateAsync(It.IsAny<User>(), ct), Times.Never);
        }

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
                Points = 500
            };

            _usersMock
                .Setup(r => r.GetByIdAsync(userId, ct))
                .ReturnsAsync(user);

            _usersMock
                .Setup(r => r.UpdateAsync(It.IsAny<User>(), ct))
                .Returns(Task.CompletedTask);

            _ratingMock
                .Setup(r => r.GetRank(user.Points))
                .Returns(UserRank.Specialist);

            _ratingMock
                .Setup(r => r.GetRankLabel(UserRank.Specialist))
                .Returns("Спеціаліст 🛠");

            var dto = await _service.UpdateProfileAsync(userId, newName, ct);

            _usersMock.Verify(r => r.GetByIdAsync(userId, ct), Times.Once);
            _usersMock.Verify(r => r.UpdateAsync(
                It.Is<User>(u => u.UserId == userId && u.FullName == newName), ct), Times.Once);

            Assert.Equal(newName, dto.FullName);
            Assert.Equal("Спеціаліст 🛠", dto.RankLabel);
        }

        // GetLeaderboardAsync

        [Fact]
        public async Task GetLeaderboardAsync_EmptyList_ReturnsEmpty()
        {
            int top = 5;
            var ct = CancellationToken.None;

            _usersMock
                .Setup(r => r.GetTopAsync(top, ct))
                .ReturnsAsync(new List<User>());

            var result = await _service.GetLeaderboardAsync(top, ct);

            Assert.NotNull(result);
            Assert.Empty(result);

            _usersMock.Verify(r => r.GetTopAsync(top, ct), Times.Once);
        }

        [Fact]
        public async Task GetLeaderboardAsync_MapsUsersToRankingEntries_UsesFullNameOrEmailAndRating()
        {
            int top = 3;
            var ct = CancellationToken.None;

            var users = new List<User>
            {
                new User { UserId = 1, FullName = "User One", Email = "one@example.com", Points = 100 },
                new User { UserId = 2, FullName = null,       Email = "two@example.com", Points = 1000 }
            };

            _usersMock
                .Setup(r => r.GetTopAsync(top, ct))
                .ReturnsAsync(users);

            _ratingMock
                .Setup(r => r.GetRank(100))
                .Returns(UserRank.Enthusiast);
            _ratingMock
                .Setup(r => r.GetRankLabel(UserRank.Enthusiast))
                .Returns("Ентузіаст 👍");

            _ratingMock
                .Setup(r => r.GetRank(1000))
                .Returns(UserRank.Expert);
            _ratingMock
                .Setup(r => r.GetRankLabel(UserRank.Expert))
                .Returns("Експерт ⭐");

            var leaderboard = await _service.GetLeaderboardAsync(top, ct);

            Assert.Equal(2, leaderboard.Count);

            var e1 = leaderboard[0];
            Assert.Equal(1, e1.UserId);
            Assert.Equal("User One", e1.FullName); // було FullName
            Assert.Equal(100, e1.Points);
            Assert.Equal("Ентузіаст 👍", e1.Rank);

            var e2 = leaderboard[1];
            Assert.Equal(2, e2.UserId);
            Assert.Equal("two@example.com", e2.FullName); // FullName null -> Email
            Assert.Equal(1000, e2.Points);
            Assert.Equal("Експерт ⭐", e2.Rank);

            _usersMock.Verify(r => r.GetTopAsync(top, ct), Times.Once);
            _ratingMock.Verify(r => r.GetRank(100), Times.Once);
            _ratingMock.Verify(r => r.GetRank(1000), Times.Once);
        }
    }
}
