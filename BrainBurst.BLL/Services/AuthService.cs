namespace BrainBurst.BLL.Services
{
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using System.Text;
    using BrainBurst.BLL.Mapping;
    using BrainBurst.BLL.Services;
    using BrainBurst.DAL.Abstractions;
    using BrainBurst.DAL.Entities;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Реалізація сервісу, що відповідає за автентифікацію та реєстрацію користувачів.
    /// </summary>
    public sealed class AuthService : IAuthService
    {
        private readonly IUserRepository _users;
        private readonly IRatingService _rating;
        private readonly ILogger<AuthService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthService"/> class.
        /// </summary>
        /// <param name="users">Репозиторій для доступу до даних користувачів.</param>
        /// <param name="rating">Сервіс для роботи з рейтингами.</param>
        /// <param name="logger">Логер для запису подій. </param>
        public AuthService(IUserRepository users, IRatingService rating, ILogger<AuthService> logger)
        {
            this._users = users;
            this._rating = rating;
            this._logger = logger;

            this._logger.LogDebug("AuthService: Сервіс автентифікації ініціалізовано.");
        }

        /// <summary>
        /// Асинхронно реєструє нового користувача в системі.
        /// </summary>
        /// <param name="email">Адреса електронної пошти.</param>
        /// <param name="password">Пароль (у відкритому вигляді).</param>
        /// <param name="fullName">Повне ім'я користувача.</param>
        /// <param name="ct">Токен скасування операції.</param>
        /// <returns>DTO створеного <see cref="UserDTO"/>.</returns>
        /// <exception cref="ArgumentException">Виникає, якщо email вже існує або дані невалідні.</exception>
        public async Task<UserDTO> RegisterAsync(string email, string password, string fullName, CancellationToken ct)
        {
            this._logger.LogDebug("RegisterAsync: Спроба реєстрації нового користувача з email: {Email}", email);

            try
            {
                Guard.Email(email);
                Guard.Password(password);
                Guard.Text(fullName, nameof(fullName), max: 100);

                if (await this._users.GetByEmailAsync(email, ct) != null)
                {
                    this._logger.LogWarning("RegisterAsync: Реєстрація невдала. Користувач з email {Email} вже існує.", email);
                    throw new ArgumentException("Користувач з таким email вже існує.");
                }

                var newUser = new User
                {
                    Email = email,
                    FullName = fullName,
                    PasswordHash = PasswordHelper.HashPassword(password),
                    Points = 0,
                    CreatedAt = DateTime.UtcNow,
                };

                var savedUser = await this._users.AddAsync(newUser, ct);

                this._logger.LogInformation("RegisterAsync: Користувач {Email} успішно зареєстрований. ID: {UserId}", email, savedUser.UserId);

                return savedUser.ToDTO(this._rating);
            }
            catch (ArgumentException ex)
            {
                this._logger.LogError(ex, "RegisterAsync: Помилка валідації даних для {Email}", email);
                throw;
            }
        }

        /// <summary>
        /// Асинхронно автентифікує користувача в системі.
        /// </summary>
        /// <param name="email">Адреса електронної пошти.</param>
        /// <param name="password">Пароль (у відкритому вигляді).</param>
        /// <param name="ct">Токен скасування операції.</param>
        /// <returns>DTO автентифікованого <see cref="UserDTO"/>.</returns>
        /// <exception cref="KeyNotFoundException">Виникає, якщо email або пароль некоректні.</exception>
        public async Task<UserDTO> LoginAsync(string email, string password, CancellationToken ct)
        {
            this._logger.LogDebug("LoginAsync: Спроба автентифікації для email: {Email}", email);

            Guard.Email(email);
            Guard.Password(password);

            var user = await this._users.GetByEmailAsync(email, ct);

            if (user == null)
            {
                this._logger.LogWarning("LoginAsync: Невдалий вхід. Користувача {Email} не знайдено.", email);
                throw new KeyNotFoundException("Некоректний email або пароль.");
            }

            if (!PasswordHelper.VerifyPassword(password, user.PasswordHash))
            {
                this._logger.LogWarning("LoginAsync: Невдалий вхід. Невірний пароль для користувача {Email}.", email);
                throw new KeyNotFoundException("Некоректний email або пароль.");
            }

            this._logger.LogInformation("LoginAsync: Користувач {Email} успішно автентифікований.", email);
            return user.ToDTO(this._rating);
        }
    }
}