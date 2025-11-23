namespace BrainBurst.BLL.Services
{
    using System.Security.Cryptography;
    using System.Text;
    using BrainBurst.BLL.Mapping;
    using BrainBurst.BLL.Services;
    using BrainBurst.DAL.Abstractions;
    using BrainBurst.DAL.Entities;

    /// <summary>
    /// Реалізація сервісу, що відповідає за автентифікацію та реєстрацію користувачів.
    /// </summary>
    public sealed class AuthService : IAuthService
    {
        private readonly IUserRepository _users;
        private readonly IRatingService _rating;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthService"/> class.
        /// </summary>
        /// <param name="users">Репозиторій для доступу до даних користувачів.</param>
        /// <param name="rating">Сервіс для роботи з рейтингами.</param>
        public AuthService(IUserRepository users, IRatingService rating)
        {
            this._users = users;
            this._rating = rating;
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
            Guard.Email(email);
            Guard.Password(password);
            Guard.Text(fullName, nameof(fullName), max: 100);

            if (await this._users.GetByEmailAsync(email, ct) != null)
            {
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

            return savedUser.ToDTO(this._rating);
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
            Guard.Email(email);
            Guard.Password(password);

            var user = await this._users.GetByEmailAsync(email, ct);

            if (user == null)
            {
                throw new KeyNotFoundException("Некоректний email або пароль.");
            }

            if (!PasswordHelper.VerifyPassword(password, user.PasswordHash))
            {
                throw new KeyNotFoundException("Некоректний email або пароль.");
            }

            return user.ToDTO(this._rating);
        }
    }
}