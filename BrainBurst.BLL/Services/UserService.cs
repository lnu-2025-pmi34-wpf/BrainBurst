// Примітка: Клас Guard знаходиться в глобальному просторі імен і доступний автоматично.

namespace BrainBurst.BLL.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BrainBurst.BLL.DTO;
    using BrainBurst.BLL.Interfaces;
    using BrainBurst.BLL.Mapping;
    using BrainBurst.DAL.Abstractions;
    using BrainBurst.DAL.Entities;

    public class UserService : IUserService
    {

        private readonly IUserRepository _users;
        private readonly IRatingService _rating;

        // ... (в класі UserService)
        public async Task DeleteAccountAsync(int userId, CancellationToken ct)
{
    // Просто викликаємо видалення у репозиторії.
    await this._users.DeleteAsync(userId, ct);
}

        public async Task ChangePasswordAsync(int userId, string oldPassword, string newPassword, CancellationToken ct)
    {
        // 1. Валідація
        Guard.Password(oldPassword);
        Guard.Password(newPassword);

        if (oldPassword.Equals(newPassword, StringComparison.Ordinal))
        {
            throw new ArgumentException("Новий пароль повинен відрізнятися від старого.");
        }

        // 2. Отримання користувача з БД
        var user = await this._users.GetByIdAsync(userId, ct);

        // 3. Верифікація старого паролю
        if (!PasswordHelper.VerifyPassword(oldPassword, user.PasswordHash))
        {
            throw new ArgumentException("Неправильний старий пароль.");
        }

        // 4. Оновлення паролю та збереження змін
        user.PasswordHash = PasswordHelper.HashPassword(newPassword);

        await this._users.UpdateAsync(user, ct);
    }

        public UserService(IUserRepository users, IRatingService rating)
        {
            this._users = users;
            this._rating = rating;
        }

        public async Task<UserDTO> GetAsync(int id, CancellationToken ct)
        {
            // Отримуємо сутність користувача з DAL (викличе виняток, якщо не знайдено)
            var user = await this._users.GetByIdAsync(id, ct);

            // Конвертуємо у DTO, використовуючи IRatingService для отримання рангу.
            return user.ToDTO(this._rating);
        }

        public async Task<UserDTO> UpdateProfileAsync(int id, string fullName, CancellationToken ct)
        {
            // 1. Валідація
            Guard.Text(fullName, "Повне ім'я", max: 100);

            // 2. Отримання та оновлення сутності
            var user = await this._users.GetByIdAsync(id, ct);

            if (user.FullName == fullName)
            {
                return user.ToDTO(this._rating);
            }

            user.FullName = fullName;

            // 3. Збереження змін
            await this._users.UpdateAsync(user, ct);

            // 4. Повернення оновленого DTO
            return user.ToDTO(this._rating);
        }

        public async Task<IReadOnlyList<RankingEntryDTO>> GetLeaderboardAsync(int top, CancellationToken ct)
        {
            // 1. Отримуємо топових користувачів з DAL, відсортованих за балами
            var topUsers = await this._users.GetTopAsync(top, ct);

            var leaderboard = new List<RankingEntryDTO>();

            // 2. Формуємо DTO для рейтингу
            foreach (var user in topUsers)
            {
                leaderboard.Add(new RankingEntryDTO
                {
                    UserId = user.UserId,
                    // Для відображення в рейтингу використовуємо FullName або Email.
                    FullName = user.FullName ?? user.Email,
                    Points = user.Points,
                    // Отримуємо текстове представлення рангу (напр., "Легенда ✨")
                    Rank = this._rating.GetRankLabel(this._rating.GetRank(user.Points))
                });
            }

            return leaderboard;
        }
    }
}