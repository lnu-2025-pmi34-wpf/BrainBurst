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

    /// <summary>
    /// Реалізація сервісу, що керує логікою, пов'язаною з профілями користувачів.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUserRepository _users;
        private readonly IRatingService _rating;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserService"/> class.
        /// </summary>
        /// <param name="users">Репозиторій для доступу до даних користувачів.</param>
        /// <param name="rating">Сервіс для роботи з рейтингами.</param>
        public UserService(IUserRepository users, IRatingService rating)
        {
            this._users = users;
            this._rating = rating;
        }

        /// <summary>
        /// Асинхронно видаляє обліковий запис користувача.
        /// </summary>
        /// <param name="userId">ID користувача для видалення.</param>
        /// <param name="ct">Токен скасування операції.</param>
        /// <returns>A <see cref="Task"/>, що представляє асинхронну операцію.</returns>
        public async Task DeleteAccountAsync(int userId, CancellationToken ct)
        {
            await this._users.DeleteAsync(userId, ct);
        }

        /// <summary>
        /// Асинхронно змінює пароль користувача.
        /// </summary>
        /// <param name="userId">ID користувача, який змінює пароль.</param>
        /// <param name="oldPassword">Поточний (старий) пароль.</param>
        /// <param name="newPassword">Новий пароль.</param>
        /// <param name="ct">Токен скасування операції.</param>
        /// <returns>A <see cref="Task"/>, що представляє асинхронну операцію.</returns>
        /// <exception cref="ArgumentException">Виникає, якщо старий пароль невірний або новий пароль співпадає зі старим.</exception>
        /// <exception cref="KeyNotFoundException">Виникає, якщо користувача не знайдено.</exception>
        public async Task ChangePasswordAsync(int userId, string oldPassword, string newPassword, CancellationToken ct)
        {
            Guard.Password(oldPassword);
            Guard.Password(newPassword);

            if (oldPassword.Equals(newPassword, StringComparison.Ordinal))
            {
                throw new ArgumentException("Новий пароль повинен відрізнятися від старого.");
            }

            var user = await this._users.GetByIdAsync(userId, ct);

            if (!PasswordHelper.VerifyPassword(oldPassword, user.PasswordHash))
            {
                throw new ArgumentException("Неправильний старий пароль.");
            }

            user.PasswordHash = PasswordHelper.HashPassword(newPassword);

            await this._users.UpdateAsync(user, ct);
        }

        /// <summary>
        /// Асинхронно отримує DTO користувача за його ID.
        /// </summary>
        /// <param name="id">ID користувача для отримання.</param>
        /// <param name="ct">Токен скасування операції.</param>
        /// <returns>DTO <see cref="UserDTO"/>.</returns>
        /// <exception cref="KeyNotFoundException">Виникає, якщо користувача не знайдено.</exception>
        public async Task<UserDTO> GetAsync(int id, CancellationToken ct)
        {
            var user = await this._users.GetByIdAsync(id, ct);

            return user.ToDTO(this._rating);
        }

        /// <summary>
        /// Асинхронно оновлює профіль користувача (наприклад, повне ім'я).
        /// </summary>
        /// <param name="id">ID користувача, чий профіль оновлюється.</param>
        /// <param name="fullName">Нове повне ім'я.</param>
        /// <param name="ct">Токен скасування операції.</param>
        /// <returns>Оновлений DTO <see cref="UserDTO"/>.</returns>
        /// <exception cref="KeyNotFoundException">Виникає, якщо користувача не знайдено.</exception>
        public async Task<UserDTO> UpdateProfileAsync(int id, string fullName, CancellationToken ct)
        {
            Guard.Text(fullName, "Повне ім'я", max: 100);

            var user = await this._users.GetByIdAsync(id, ct);

            if (user.FullName == fullName)
            {
                return user.ToDTO(this._rating);
            }

            user.FullName = fullName;

            await this._users.UpdateAsync(user, ct);

            return user.ToDTO(this._rating);
        }

        /// <summary>
        /// Асинхронно отримує таблицю лідерів (рейтинг).
        /// </summary>
        /// <param name="top">Кількість користувачів, яку потрібно повернути.</param>
        /// <param name="ct">Токен скасування операції.</param>
        /// <returns>Список <see cref="RankingEntryDTO"/>, доступний лише для читання.</returns>
        public async Task<IReadOnlyList<RankingEntryDTO>> GetLeaderboardAsync(int top, CancellationToken ct)
        {
            var topUsers = await this._users.GetTopAsync(top, ct);

            var leaderboard = new List<RankingEntryDTO>();

            foreach (var user in topUsers)
            {
                leaderboard.Add(new RankingEntryDTO
                {
                    UserId = user.UserId,
                    FullName = user.FullName ?? user.Email,
                    Points = user.Points,
                    Rank = this._rating.GetRankLabel(this._rating.GetRank(user.Points)),
                });
            }

            return leaderboard;
        }
    }
}