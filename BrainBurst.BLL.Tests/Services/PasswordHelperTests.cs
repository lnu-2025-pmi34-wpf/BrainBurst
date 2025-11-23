namespace BrainBurst.BLL.Tests.Services
{
    using System;
    using BrainBurst.BLL.Services;
    using Xunit;

    /// <summary>
    /// Містить юніт-тести для <see cref="PasswordHelper"/>.
    /// </summary>
    public class PasswordHelperTests
    {
        /// <summary>
        /// Тест: HashPassword повертає валідний (не порожній) Base64 рядок.
        /// </summary>
        [Fact]
        public void HashPassword_ReturnsBase64String()
        {
            string pwd = "MySecurePassword123";

            string hash = PasswordHelper.HashPassword(pwd);

            Assert.False(string.IsNullOrWhiteSpace(hash));

#pragma warning disable SA1011
            byte[]? bytes = null;
#pragma warning restore SA1011
            var exception = Record.Exception(() => bytes = Convert.FromBase64String(hash));

            Assert.Null(exception);
            Assert.NotNull(bytes);
        }

        /// <summary>
        /// Тест: HashPassword повертає однаковий хеш для однакових вхідних даних (оскільки "сіль" не використовується).
        /// </summary>
        [Fact]
        public void HashPassword_SameInput_ReturnsSameHash()
        {
            string pwd = "test123";

            string h1 = PasswordHelper.HashPassword(pwd);
            string h2 = PasswordHelper.HashPassword(pwd);

            Assert.Equal(h1, h2);
        }

        /// <summary>
        /// Тест: HashPassword повертає різні хеші для різних паролів.
        /// </summary>
        [Fact]
        public void HashPassword_DifferentPasswords_ReturnDifferentHashes()
        {
            string h1 = PasswordHelper.HashPassword("abc1");
            string h2 = PasswordHelper.HashPassword("abc2");

            Assert.NotEqual(h1, h2);
        }

        /// <summary>
        /// Тест: VerifyPassword повертає true, якщо пароль та хеш співпадають.
        /// </summary>
        [Fact]
        public void VerifyPassword_CorrectPassword_ReturnsTrue()
        {
            string pwd = "SofiiaStrongPassword!";
            string hash = PasswordHelper.HashPassword(pwd);

            bool result = PasswordHelper.VerifyPassword(pwd, hash);

            Assert.True(result);
        }

        /// <summary>
        /// Тест: VerifyPassword повертає false, якщо пароль не співпадає з хешем.
        /// </summary>
        [Fact]
        public void VerifyPassword_WrongPassword_ReturnsFalse()
        {
            string hash = PasswordHelper.HashPassword("correct");

            bool result = PasswordHelper.VerifyPassword("wrong", hash);

            Assert.False(result);
        }

        /// <summary>
        /// Тест: VerifyPassword повертає false, якщо пароль порожній.
        /// </summary>
        [Fact]
        public void VerifyPassword_EmptyPassword_ReturnsFalse()
        {
            string hash = PasswordHelper.HashPassword("qqq");

            bool result = PasswordHelper.VerifyPassword(string.Empty, hash);

            Assert.False(result);
        }

        /// <summary>
        /// Тест: VerifyPassword повертає false, якщо хеш порожній.
        /// </summary>
        [Fact]
        public void VerifyPassword_EmptyHash_ReturnsFalse()
        {
            bool result = PasswordHelper.VerifyPassword("pwd", string.Empty);

            Assert.False(result);
        }

        /// <summary>
        /// Тест: VerifyPassword повертає false, якщо хеш є null.
        /// </summary>
        [Fact]
        public void VerifyPassword_NullHash_ReturnsFalse()
        {
            bool result = PasswordHelper.VerifyPassword("pwd", hash: null!);

            Assert.False(result);
        }
    }
}
