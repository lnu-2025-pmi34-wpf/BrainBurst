using BrainBurst.BLL.Services;
using System;
using Xunit;

namespace BrainBurst.BLL.Tests.Services
{
    public class PasswordHelperTests
    {
        // 1. переконуємось, що HashPassword повертає непорожній Base64
        [Fact]
        public void HashPassword_ReturnsBase64String()
        {
            string pwd = "MySecurePassword123";

            string hash = PasswordHelper.HashPassword(pwd);

            Assert.False(string.IsNullOrWhiteSpace(hash));

            byte[]? bytes = null;
            var exception = Record.Exception(() => bytes = Convert.FromBase64String(hash));

            Assert.Null(exception);
            Assert.NotNull(bytes);
        }

        // 2. один і той самий пароль -> однаковий хеш
        [Fact]
        public void HashPassword_SameInput_ReturnsSameHash()
        {
            string pwd = "test123";

            string h1 = PasswordHelper.HashPassword(pwd);
            string h2 = PasswordHelper.HashPassword(pwd);

            Assert.Equal(h1, h2);
        }

        // 3. різні паролі -> різні хеші
        [Fact]
        public void HashPassword_DifferentPasswords_ReturnDifferentHashes()
        {
            string h1 = PasswordHelper.HashPassword("abc1");
            string h2 = PasswordHelper.HashPassword("abc2");

            Assert.NotEqual(h1, h2);
        }

        // 4. VerifyPassword успішний
        [Fact]
        public void VerifyPassword_CorrectPassword_ReturnsTrue()
        {
            string pwd = "SofiiaStrongPassword!";
            string hash = PasswordHelper.HashPassword(pwd);

            bool result = PasswordHelper.VerifyPassword(pwd, hash);

            Assert.True(result);
        }

        // 5. VerifyPassword неправильний пароль -> false
        [Fact]
        public void VerifyPassword_WrongPassword_ReturnsFalse()
        {
            string hash = PasswordHelper.HashPassword("correct");

            bool result = PasswordHelper.VerifyPassword("wrong", hash);

            Assert.False(result);
        }

        // 6. VerifyPassword: порожній пароль
        [Fact]
        public void VerifyPassword_EmptyPassword_ReturnsFalse()
        {
            string hash = PasswordHelper.HashPassword("qqq");

            bool result = PasswordHelper.VerifyPassword("", hash);

            Assert.False(result);
        }

        // 7. VerifyPassword: порожній хеш → завжди false
        [Fact]
        public void VerifyPassword_EmptyHash_ReturnsFalse()
        {
            bool result = PasswordHelper.VerifyPassword("pwd", "");

            Assert.False(result);
        }

        // 8. VerifyPassword: null-хеш → NRE не кидається, але false
        [Fact]
        public void VerifyPassword_NullHash_ReturnsFalse()
        {
            bool result = PasswordHelper.VerifyPassword("pwd", hash: null);

            Assert.False(result);
        }
    }
}
