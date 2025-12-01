namespace BrainBurst.DAL.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BrainBurst.DAL.Abstractions;
    using BrainBurst.DAL.Data;
    using BrainBurst.DAL.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Реалізація репозиторію для роботи з сутностями <see cref="User"/>.
    /// Цей репозиторій використовує <see cref="IServiceProvider"/> для створення
    /// короткоживучих "scopes" (областей видимості) DbContext для кожного методу.
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<UserRepository> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserRepository"/> class.
        /// </summary>
        /// <param name="serviceProvider">Постачальник служб (DI) для створення "scopes".</param>
        /// <param name="logger">Логер для запису подій.</param>
        public UserRepository(IServiceProvider serviceProvider, ILogger<UserRepository> logger)
        {
            this._serviceProvider = serviceProvider;
            this._logger = logger;

            this._logger.LogDebug("UserRepository: Репозиторій користувачів ініціалізовано.");
        }

        /// <summary>
        /// Асинхронно додає нового користувача до бази даних.
        /// </summary>
        /// <param name="user">Сутність <see cref="User"/> для додавання.</param>
        /// <param name="ct">Токен скасування операції.</param>
        /// <returns>Додана сутність <see cref="User"/>.</returns>
        public async Task<User> AddAsync(User user, CancellationToken ct)
        {
            this._logger.LogDebug("AddAsync: Спроба додати нового користувача.");
            try
            {
                using (var scope = this._serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    context.Users.Add(user);
                    await context.SaveChangesAsync(ct);

                    this._logger.LogInformation("AddAsync: Користувача {UserId} успішно додано.", user.UserId);
                    return user;
                }
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "AddAsync: Критична помилка при додаванні користувача.");
                throw;
            }
        }

        /// <summary>
        /// Асинхронно отримує користувача за його email.
        /// </summary>
        /// <param name="email">Email для пошуку.</param>
        /// <param name="ct">Токен скасування операції.</param>
        /// <returns>Знайдений <see cref="User"/> або null.</returns>
        public async Task<User?> GetByEmailAsync(string email, CancellationToken ct)
        {
            this._logger.LogDebug("GetByEmailAsync: Пошук користувача за email: {Email}", email);

            try
            {
                using (var scope = this._serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var user = await context.Users
                        .AsNoTracking()
                        .FirstOrDefaultAsync(u => u.Email == email, ct);

                    if (user != null)
                    {
                        this._logger.LogDebug("GetByEmailAsync: Користувача {Email} знайдено.", email);
                    }
                    else
                    {
                        this._logger.LogDebug("GetByEmailAsync: Користувача {Email} не знайдено.", email);
                    }

                    return user;
                }
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "GetByEmailAsync: Критична помилка при пошуку за email: {Email}", email);
                throw;
            }
        }

        /// <summary>
        /// Асинхронно отримує користувача за його ID.
        /// </summary>
        /// <param name="userId">ID користувача для пошуку.</param>
        /// <param name="ct">Токен скасування операції.</param>
        /// <returns>Знайдений <see cref="User"/>.</returns>
        /// <exception cref="KeyNotFoundException">Виникає, якщо користувача з таким ID не знайдено.</exception>
        public async Task<User> GetByIdAsync(int userId, CancellationToken ct)
        {
            this._logger.LogDebug("GetByIdAsync: Пошук користувача за ID: {UserId}", userId);

            try
            {
                using (var scope = this._serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var user = await context.Users
                       .AsNoTracking()
                       .FirstOrDefaultAsync(u => u.UserId == userId, ct);

                    if (user == null)
                    {
                        this._logger.LogWarning("GetByIdAsync: Користувача з ID {UserId} не знайдено.", userId);
                        throw new KeyNotFoundException($"User with ID {userId} not found.");
                    }

                    this._logger.LogDebug("GetByIdAsync: Користувача {UserId} знайдено.", userId);
                    return user;
                }
            }
            catch (Exception ex)
            {
                if (ex is not KeyNotFoundException)
                {
                    this._logger.LogError(ex, "GetByIdAsync: Критична помилка при пошуку за ID: {UserId}", userId);
                }

                throw;
            }
        }

        /// <summary>
        /// Асинхронно оновлює дані існуючого користувача.
        /// </summary>
        /// <param name="user">Сутність <see cref="User"/> з оновленими даними.</param>
        /// <param name="ct">Токен скасування операції.</param>
        /// <returns>A <see cref="Task"/>, що представляє асинхронну операцію.</returns>
        public async Task UpdateAsync(User user, CancellationToken ct)
        {
            this._logger.LogDebug("UpdateAsync: Спроба оновити дані користувача з ID: {UserId}", user.UserId);

            try
            {
                using (var scope = this._serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    context.Attach(user).State = EntityState.Modified;

                    context.Entry(user).Property(u => u.PasswordHash).IsModified = false;

                    await context.SaveChangesAsync(ct);

                    this._logger.LogInformation("UpdateAsync: Користувача {UserId} успішно оновлено.", user.UserId);
                }
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "UpdateAsync: Критична помилка при оновленні користувача з ID: {UserId}", user.UserId);
                throw;
            }
        }

        /// <summary>
        /// Асинхронно отримує список "топ" користувачів, відсортованих за балами.
        /// </summary>
        /// <param name="take">Кількість користувачів, яку потрібно повернути.</param>
        /// <param name="ct">Токен скасування операції.</param>
        /// <returns>Список <see cref="User"/>, доступний лише для читання.</returns>
        public async Task<IReadOnlyList<User>> GetTopAsync(int take, CancellationToken ct)
        {
            this._logger.LogDebug("GetTopAsync: Отримання топ-{Count} користувачів.", take);

            try
            {
                using (var scope = this._serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var users = await context.Users
                        .OrderByDescending(u => u.Points)
                        .Take(take)
                        .AsNoTracking()
                        .ToListAsync(ct);

                    this._logger.LogInformation("GetTopAsync: Успішно отримано {Count} топ користувачів.", users.Count);
                    return users;
                }
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "GetTopAsync: Критична помилка при отриманні топ-користувачів.");
                throw;
            }
        }

        /// <summary>
        /// Асинхронно видаляє користувача за ID.
        /// </summary>
        /// <param name="id">ID користувача для видалення.</param>
        /// <param name="ct">Токен скасування операції.</param>
        /// <returns>A <see cref="Task"/>, що представляє асинхронну операцію.</returns>
        /// <exception cref="KeyNotFoundException">Виникає, якщо користувача з таким ID не знайдено.</exception>
        public async Task DeleteAsync(int id, CancellationToken ct)
        {
            this._logger.LogDebug("DeleteAsync: Спроба видалити користувача з ID: {UserId}", id);
            try
            {
                using (var scope = this._serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var user = await context.Users.FirstOrDefaultAsync(u => u.UserId == id, ct);
                    if (user == null)
                    {
                        this._logger.LogWarning("DeleteAsync: Видалення невдале. Користувача з ID {UserId} не знайдено.", id);
                        throw new KeyNotFoundException($"User with ID {id} not found.");
                    }

                    context.Users.Remove(user);
                    await context.SaveChangesAsync(ct);

                    this._logger.LogInformation("DeleteAsync: Користувача з ID {UserId} успішно видалено.", id);
                }
            }
            catch (Exception ex)
            {
                if (ex is not KeyNotFoundException)
                {
                    this._logger.LogError(ex, "DeleteAsync: Критична помилка при видаленні користувача з ID {UserId}.", id);
                }

                throw;
            }
        }

        private (ApplicationDbContext Context, IServiceScope Scope) GetContextAndScope()
        {
            var scope = this._serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            return (Context: context, Scope: scope);
        }
    }
}