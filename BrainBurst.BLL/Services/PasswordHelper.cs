namespace BrainBurst.BLL.Services
{
    using System.Security.Cryptography;
    using System.Text;

    // Reusable utility for hashing and verification.
    // 🚨 УВАГА: У реальному додатку використовуйте BCrypt.Net або схожі криптографічні бібліотеки!
    public static class PasswordHelper
    {
        public static string HashPassword(string password)
        {
            // Проста імітація хешування для демонстрації.
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        public static bool VerifyPassword(string password, string hash)
        {
            // Перевірка на основі простої імітації.
            return HashPassword(password) == hash;
        }
    }
}