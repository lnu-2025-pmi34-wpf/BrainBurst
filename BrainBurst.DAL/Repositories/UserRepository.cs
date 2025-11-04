using BrainBurst.DAL.Abstractions;
using BrainBurst.DAL.Data;
using BrainBurst.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BrainBurst.DAL.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IServiceProvider _serviceProvider;

        public UserRepository(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        // Хелпер для отримання контексту та Scope, щоб ми могли його звільнити
        private (ApplicationDbContext Context, IServiceScope Scope) GetContextAndScope()
        {
            var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            return (Context: context, Scope: scope);
        }

        public async Task<User> AddAsync(User user, CancellationToken ct)
        {
            // Використовуємо using для гарантованого виклику Dispose() на Scope
            using (var scope = _serviceProvider.CreateScope()) 
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                
                context.Users.Add(user);
                await context.SaveChangesAsync(ct);
                return user; 
            }
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken ct)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                return await context.Users
                    .AsNoTracking() 
                    .FirstOrDefaultAsync(u => u.Email == email, ct);
            }
        }

        public async Task<User> GetByIdAsync(int userId, CancellationToken ct)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                 var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                 var user = await context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.UserId == userId, ct);
                
                return user ?? throw new KeyNotFoundException($"User with ID {userId} not found.");
            }
        }

        public async Task UpdateAsync(User user, CancellationToken ct)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Оновлення полів user.UserId = 0, тому потрібно прикріпити
                context.Attach(user).State = EntityState.Modified; 
                
                // Переконаємося, що хеш пароля не оновлюється випадково
                context.Entry(user).Property(u => u.PasswordHash).IsModified = false;
                
                await context.SaveChangesAsync(ct);
            }
        }

        public async Task<IReadOnlyList<User>> GetTopAsync(int take, CancellationToken ct)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                return await context.Users
                    .OrderByDescending(u => u.Points)
                    .Take(take)
                    .AsNoTracking()
                    .ToListAsync(ct);
            }
        }
        
        public async Task DeleteAsync(int id, CancellationToken ct)
        {
            using (var scope = _serviceProvider.CreateScope())
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
    }
}