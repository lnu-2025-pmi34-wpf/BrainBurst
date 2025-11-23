namespace BrainBurst.BLL.Services
{
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    /// Статичний клас-помічник для операцій, пов'язаних із паролями (хешування та верифікація).
    /// </summary>
    public static class PasswordHelper
    {
        /// <summary>
        /// Хешує пароль у відкритому вигляді.
        /// </summary>
        /// <remarks>
        /// Це проста імітація хешування для демонстрації.
        /// У реальному проекті слід використовувати bcrypt або Argon2 з "сіллю".
        /// </remarks>
        /// <param name="password">Пароль у відкритому вигляді.</param>
        /// <returns>Base64-представлення хешу SHA256.</returns>
        public static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Перевіряє, чи відповідає пароль у відкритому вигляді збереженому хешу.
        /// </summary>
        /// <param name="password">Пароль у відкритому вигляді, наданий користувачем.</param>
        /// <param name="hash">Збережений хеш пароля з бази даних.</param>
        /// <returns><c>true</c>, якщо пароль відповідає хешу; інакше <c>false</c>.</returns>
        public static bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }
    }
}