using System;
using BrainBurst.BLL.DTO;
using BrainBurst.BLL.Services;
using Xunit;

namespace BrainBurst.BLL.Tests.Services
{
    public class AuthContextTests
    {
        private readonly AuthContext _auth;

        public AuthContextTests()
        {
            _auth = new AuthContext();
        }

        // 1. початково контекст порожній
        [Fact]
        public void CurrentUser_IsNull_Initially()
        {
            Assert.Null(_auth.CurrentUser);
            Assert.Equal(0, _auth.CurrentUserId);
        }

        // 2. SetCurrentUser встановлює користувача
        [Fact]
        public void SetCurrentUser_SetsUserCorrectly()
        {
            var user = new UserDTO
            {
                Id = 10,
                Email = "test@example.com",
                FullName = "Tester"
            };

            _auth.SetCurrentUser(user);

            Assert.Equal(user, _auth.CurrentUser);
            Assert.Equal(10, _auth.CurrentUserId);
        }

        // 3. SetCurrentUser(null) -> ArgumentNullException
        [Fact]
        public void SetCurrentUser_Null_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _auth.SetCurrentUser(null!));
        }

        // 4. ClearContext очищає користувача
        [Fact]
        public void ClearContext_RemovesCurrentUser()
        {
            var user = new UserDTO { Id = 7 };
            _auth.SetCurrentUser(user);

            _auth.ClearContext();

            Assert.Null(_auth.CurrentUser);
            Assert.Equal(0, _auth.CurrentUserId);
        }

        // 5. SetCurrentUser перезаписує попереднього
        [Fact]
        public void SetCurrentUser_OverridesPreviousUser()
        {
            var first = new UserDTO { Id = 1 };
            var second = new UserDTO { Id = 2 };

            _auth.SetCurrentUser(first);
            _auth.SetCurrentUser(second);

            Assert.Equal(second, _auth.CurrentUser);
            Assert.Equal(2, _auth.CurrentUserId);
        }

        // 6. CurrentUserId повертає 0, якщо користувача нема
        [Fact]
        public void CurrentUserId_ReturnsZero_WhenNoUser()
        {
            Assert.Equal(0, _auth.CurrentUserId);
        }

        // 7. CurrentUserId відображає справжній Id користувача
        [Fact]
        public void CurrentUserId_ReturnsUserId_WhenUserPresent()
        {
            var user = new UserDTO { Id = 12345 };
            _auth.SetCurrentUser(user);

            Assert.Equal(12345, _auth.CurrentUserId);
        }

        // 8. CurrentUser після SetCurrentUser містить DTO з усіма полями
        [Fact]
        public void CurrentUser_ContainsCorrectUserDTO()
        {
            var dto = new UserDTO
            {
                Id = 99,
                Email = "u@mail.com",
                FullName = "User X",
                Points = 150,
                RankLabel = "Майстер"
            };

            _auth.SetCurrentUser(dto);

            var result = _auth.CurrentUser;

            Assert.Equal("u@mail.com", result!.Email);
            Assert.Equal("User X", result.FullName);
            Assert.Equal(150, result.Points);
            Assert.Equal("Майстер", result.RankLabel);
        }

        // 9. ClearContext після SetCurrentUser встановлює CurrentUser у null
        [Fact]
        public void ClearContext_AfterSetCurrentUser_SetsUserToNull()
        {
            _auth.SetCurrentUser(new UserDTO { Id = 555 });

            _auth.ClearContext();

            Assert.Null(_auth.CurrentUser);
        }

        // 10. повторний ClearContext не ламається
        [Fact]
        public void ClearContext_CalledTwice_DoesNotThrow()
        {
            _auth.ClearContext();
            _auth.ClearContext(); // друга спроба

            Assert.Null(_auth.CurrentUser);
            Assert.Equal(0, _auth.CurrentUserId);
        }
    }
}
