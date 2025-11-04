using BrainBurst.DAL.Abstractions; // ВИПРАВЛЕНО
using BrainBurst.DAL.Entities;
using BrainBurst.BLL.Mapping;
using System.Security.Cryptography;
using System.Text;
using BrainBurst.BLL.Services;

namespace BrainBurst.BLL.Services
{
    // *** УВАГА: У реальному додатку використовуйте BCrypt.Net або схожі бібліотеки! ***
    // ВИПРАВЛЕНО: Прибрано 'private'
   
    // ********************************************************************************

    public sealed class AuthService : IAuthService
    {
        private readonly IUserRepository _users; // IUserRepository тепер з DAL.Abstractions
        private readonly IRatingService _rating;

        public AuthService(IUserRepository users, IRatingService rating)
        {
            _users = users;
            _rating = rating;
        }
        
        // ... (інші методи RegisterAsync та LoginAsync залишаються без змін)
        public async Task<UserDTO> RegisterAsync(string email, string password, string fullName, CancellationToken ct)
        {
            // 1. Валідація вхідних даних
            Guard.Email(email);
            Guard.Password(password);
            Guard.Text(fullName, nameof(fullName), max: 100);

            // 2. Перевірка наявності користувача
            if (await _users.GetByEmailAsync(email, ct) != null)
                throw new ArgumentException("Користувач з таким email вже існує.");

            // 3. Створення та збереження нового користувача
            var newUser = new User
            {
                Email = email,
                FullName = fullName,
                PasswordHash = PasswordHelper.HashPassword(password),
                Points = 0, // Початкові бали
                CreatedAt = DateTime.UtcNow
            };

            var savedUser = await _users.AddAsync(newUser, ct);
            
            // 4. Повернення DTO (використовуємо ToDTO з MappingExtensions)
            return savedUser.ToDTO(_rating);
        }

        public async Task<UserDTO> LoginAsync(string email, string password, CancellationToken ct)
        {
            // 1. Валідація
            Guard.Email(email);
            Guard.Password(password);

            // 2. Пошук користувача
            var user = await _users.GetByEmailAsync(email, ct);

            if (user == null)
                throw new KeyNotFoundException("Некоректний email або пароль.");

            // 3. Верифікація паролю
            if (!PasswordHelper.VerifyPassword(password, user.PasswordHash))
                throw new KeyNotFoundException("Некоректний email або пароль.");

            // 4. Повернення DTO
            return user.ToDTO(_rating);
        }
    }
}