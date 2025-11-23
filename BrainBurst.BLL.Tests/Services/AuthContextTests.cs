namespace BrainBurst.BLL.Tests.Services
{
    using System;
    using BrainBurst.BLL.DTO;
    using BrainBurst.BLL.Services;
    using Xunit;

    /// <summary>
    /// Містить юніт-тести для <see cref="AuthContext"/>.
    /// </summary>
    public class AuthContextTests
    {
        private readonly AuthContext _auth;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthContextTests"/> class.
        /// створюючи чистий екземпляр <see cref="AuthContext"/> перед кожним тестом.
        /// </summary>
        public AuthContextTests()
        {
            this._auth = new AuthContext();
        }

        /// <summary>
        /// Тест: Перевіряє, що початковий стан контексту є порожнім (CurrentUser is null та CurrentUserId is 0).
        /// </summary>
        [Fact]
        public void CurrentUser_IsNull_Initially()
        {
            Assert.Null(this._auth.CurrentUser);
            Assert.Equal(0, this._auth.CurrentUserId);
        }

        /// <summary>
        /// Тест: SetCurrentUser коректно встановлює користувача та його ID.
        /// </summary>
        [Fact]
        public void SetCurrentUser_SetsUserCorrectly()
        {
            var user = new UserDTO
            {
                Id = 10,
                Email = "test@example.com",
                FullName = "Tester",
            };

            this._auth.SetCurrentUser(user);

            Assert.Equal(user, this._auth.CurrentUser);
            Assert.Equal(10, this._auth.CurrentUserId);
        }

        /// <summary>
        /// Тест: SetCurrentUser кидає ArgumentNullException при спробі встановити null.
        /// </summary>
        [Fact]
        public void SetCurrentUser_Null_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => this._auth.SetCurrentUser(null!));
        }

        /// <summary>
        /// Тест: ClearContext коректно очищає раніше встановленого користувача.
        /// </summary>
        [Fact]
        public void ClearContext_RemovesCurrentUser()
        {
            var user = new UserDTO { Id = 7 };
            this._auth.SetCurrentUser(user);

            this._auth.ClearContext();

            Assert.Null(this._auth.CurrentUser);
            Assert.Equal(0, this._auth.CurrentUserId);
        }

        /// <summary>
        /// Тест: SetCurrentUser перезаписує попереднього користувача новим.
        /// </summary>
        [Fact]
        public void SetCurrentUser_OverridesPreviousUser()
        {
            var first = new UserDTO { Id = 1 };
            var second = new UserDTO { Id = 2 };

            this._auth.SetCurrentUser(first);
            this._auth.SetCurrentUser(second);

            Assert.Equal(second, this._auth.CurrentUser);
            Assert.Equal(2, this._auth.CurrentUserId);
        }

        /// <summary>
        /// Тест: CurrentUserId повертає 0, якщо користувач не встановлений.
        /// </summary>
        [Fact]
        public void CurrentUserId_ReturnsZero_WhenNoUser()
        {
            Assert.Equal(0, this._auth.CurrentUserId);
        }

        /// <summary>
        /// Тест: CurrentUserId коректно повертає ID встановленого користувача.
        /// </summary>
        [Fact]
        public void CurrentUserId_ReturnsUserId_WhenUserPresent()
        {
            var user = new UserDTO { Id = 12345 };
            this._auth.SetCurrentUser(user);

            Assert.Equal(12345, this._auth.CurrentUserId);
        }

        /// <summary>
        /// Тест: CurrentUser містить коректний DTO з усіма полями після SetCurrentUser.
        /// </summary>
        [Fact]
        public void CurrentUser_ContainsCorrectUserDTO()
        {
            var dto = new UserDTO
            {
                Id = 99,
                Email = "u@mail.com",
                FullName = "User X",
                Points = 150,
                RankLabel = "Майстер",
            };

            this._auth.SetCurrentUser(dto);

            var result = this._auth.CurrentUser;

            Assert.Equal("u@mail.com", result!.Email);
            Assert.Equal("User X", result.FullName);
            Assert.Equal(150, result.Points);
            Assert.Equal("Майстер", result.RankLabel);
        }

        /// <summary>
        /// Тест: ClearContext після SetCurrentUser встановлює CurrentUser у null.
        /// </summary>
        [Fact]
        public void ClearContext_AfterSetCurrentUser_SetsUserToNull()
        {
            this._auth.SetCurrentUser(new UserDTO { Id = 555 });

            this._auth.ClearContext();

            Assert.Null(this._auth.CurrentUser);
        }

        /// <summary>
        /// Тест: Повторний виклик ClearContext не викликає помилок.
        /// </summary>
        [Fact]
        public void ClearContext_CalledTwice_DoesNotThrow()
        {
            this._auth.ClearContext();
            this._auth.ClearContext();

            Assert.Null(this._auth.CurrentUser);
            Assert.Equal(0, this._auth.CurrentUserId);
        }
    }
}
