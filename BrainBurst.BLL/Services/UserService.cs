namespace BrainBurst.BLL.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BrainBurst.BLL.DTO;
    using BrainBurst.BLL.Interfaces;
    using BrainBurst.BLL.Mapping;
    using BrainBurst.DAL.Abstractions;
    using BrainBurst.DAL.Entities;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Реалізація сервісу, що керує логікою, пов'язаною з профілями користувачів.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUserRepository _users;
        private readonly IRatingService _rating;
        private readonly ILogger<UserService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserService"/> class.
        /// </summary>
        /// <param name="users">Репозиторій для доступу до даних користувачів.</param>
        /// <param name="rating">Сервіс для роботи з рейтингами.</param>
        /// <param name="logger">Логер для запису подій.</param>
        public UserService(IUserRepository users, IRatingService rating, ILogger<UserService> logger)
        {
            this._users = users;
            this._rating = rating;
            this._logger = logger;

            this._logger.LogDebug("UserService: Сервіс користувачів ініціалізовано.");
        }

        /// <summary>
        /// Асинхронно видаляє обліковий запис користувача.
        /// </summary>
        /// <param name="userId">ID користувача для видалення.</param>
        /// <param name="ct">Токен скасування операції.</param>
        /// <returns>A <see cref="Task"/>, що представляє асинхронну операцію.</returns>
        public async Task DeleteAccountAsync(int userId, CancellationToken ct)
        {
            this._logger.LogInformation("DeleteAccountAsync: Запит на видалення облікового запису користувача {UserId}.", userId);

            try
            {
                await this._users.DeleteAsync(userId, ct);
                this._logger.LogWarning("DeleteAccountAsync: Обліковий запис користувача {UserId} успішно ВИДАЛЕНО.", userId);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "DeleteAccountAsync: Критична помилка при видаленні облікового запису {UserId}.", userId);
                throw;
            }
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
            this._logger.LogInformation("ChangePasswordAsync: Спроба зміни пароля для користувача {UserId}.", userId);

            try
            {
                Guard.Password(oldPassword);
                Guard.Password(newPassword);

                if (oldPassword.Equals(newPassword, StringComparison.Ordinal))
                {
                    this._logger.LogWarning("ChangePasswordAsync: Відхилено. Новий пароль співпадає зі старим для {UserId}.", userId);
                    throw new ArgumentException("Новий пароль повинен відрізнятися від старого.");
                }

                var user = await this._users.GetByIdAsync(userId, ct);

                if (!PasswordHelper.VerifyPassword(oldPassword, user.PasswordHash))
                {
                    this._logger.LogError("ChangePasswordAsync: Відхилено. Неправильний старий пароль для {UserId}.", userId);
                    throw new ArgumentException("Неправильний старий пароль.");
                }

                user.PasswordHash = PasswordHelper.HashPassword(newPassword);

                await this._users.UpdateAsync(user, ct);

                this._logger.LogInformation("ChangePasswordAsync: Пароль для користувача {UserId} успішно змінено.", userId);
            }
            catch (Exception ex)
            {
                if (ex is not ArgumentException && ex is not KeyNotFoundException)
                {
                    this._logger.LogError(ex, "ChangePasswordAsync: Критична помилка при зміні пароля для {UserId}.", userId);
                }
                throw;
            }
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
            this._logger.LogDebug("GetAsync: Запит DTO користувача за ID: {UserId}", id);

            try
            {
                var user = await this._users.GetByIdAsync(id, ct);
                this._logger.LogDebug("GetAsync: DTO користувача {UserId} успішно отримано.", id);
                return user.ToDTO(this._rating);
            }
            catch (Exception ex)
            {
                if (ex is not KeyNotFoundException)
                {
                    this._logger.LogError(ex, "GetAsync: Критична помилка при отриманні DTO користувача {UserId}.", id);
                }

                throw;
            }
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
            this._logger.LogInformation("UpdateProfileAsync: Оновлення профілю {UserId} на FullName: {FullName}", id, fullName);

            try
            {
                Guard.Text(fullName, "Повне ім'я", max: 100);

                var user = await this._users.GetByIdAsync(id, ct);

                if (user.FullName == fullName)
                {
                    this._logger.LogInformation("UpdateProfileAsync: Профіль {UserId} не оновлено, оскільки дані не змінилися.", id);
                    return user.ToDTO(this._rating);
                }

                user.FullName = fullName;

                await this._users.UpdateAsync(user, ct);

                this._logger.LogInformation("UpdateProfileAsync: Профіль {UserId} успішно оновлено.", id);

                return user.ToDTO(this._rating);
            }
            catch (Exception ex)
            {
                if (ex is not ArgumentException && ex is not KeyNotFoundException)
                {
                    this._logger.LogError(ex, "UpdateProfileAsync: Критична помилка при оновленні профілю {UserId}.", id);
                }
                throw;
            }
        }

        /// <summary>
        /// Асинхронно отримує таблицю лідерів (рейтинг).
        /// </summary>
        /// <param name="top">Кількість користувачів, яку потрібно повернути.</param>
        /// <param name="ct">Токен скасування операції.</param>
        /// <returns>Список <see cref="RankingEntryDTO"/>, доступний лише для читання.</returns>
        public async Task<IReadOnlyList<RankingEntryDTO>> GetLeaderboardAsync(int top, CancellationToken ct)
        {
            this._logger.LogInformation("GetLeaderboardAsync: Запит таблиці лідерів (топ {Top}).", top);

            try
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

                this._logger.LogInformation("GetLeaderboardAsync: Успішно сформовано {Count} записів у таблиці лідерів.", topUsers.Count);
                return leaderboard;
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "GetLeaderboardAsync: Критична помилка при формуванні таблиці лідерів.");
                throw;
            }
        }
    }
}