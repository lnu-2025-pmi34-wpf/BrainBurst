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

    /// <summary>
    /// Реалізація репозиторію для роботи з сутностями <see cref="User"/>.
    /// Цей репозиторій використовує <see cref="IServiceProvider"/> для створення
    /// короткоживучих "scopes" (областей видимості) DbContext для кожного методу.
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserRepository"/> class.
        /// </summary>
        /// <param name="serviceProvider">Постачальник служб (DI) для створення "scopes".</param>
        public UserRepository(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Асинхронно додає нового користувача до бази даних.
        /// </summary>
        /// <param name="user">Сутність <see cref="User"/> для додавання.</param>
        /// <param name="ct">Токен скасування операції.</param>
        /// <returns>Додана сутність <see cref="User"/>.</returns>
        public async Task<User> AddAsync(User user, CancellationToken ct)
        {
            using (var scope = this._serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                context.Users.Add(user);
                await context.SaveChangesAsync(ct);
                return user;
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
            using (var scope = this._serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                return await context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email == email, ct);
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
            using (var scope = this._serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var user = await context.Users
                   .AsNoTracking()
                   .FirstOrDefaultAsync(u => u.UserId == userId, ct);

                return user ?? throw new KeyNotFoundException($"User with ID {userId} not found.");
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
            using (var scope = this._serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                context.Attach(user).State = EntityState.Modified;

                context.Entry(user).Property(u => u.PasswordHash).IsModified = false;

                await context.SaveChangesAsync(ct);
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
            using (var scope = this._serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                return await context.Users
                    .OrderByDescending(u => u.Points)
                    .Take(take)
                    .AsNoTracking()
                    .ToListAsync(ct);
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
            using (var scope = this._serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var user = await context.Users.FirstOrDefaultAsync(u => u.UserId == id, ct);
                if (user == null)
                {
                    throw new KeyNotFoundException($"User with ID {id} not found.");
                }

                context.Users.Remove(user);
                await context.SaveChangesAsync(ct);
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