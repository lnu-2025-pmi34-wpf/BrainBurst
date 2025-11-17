namespace BrainBurst.DAL.Repositories
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BrainBurst.DAL.Abstractions;
    using BrainBurst.DAL.Data;
    using BrainBurst.DAL.Entities;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Реалізація репозиторію для роботи з сутностями <see cref="Test"/>.
    /// </summary>
    public class TestRepository : ITestRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestRepository"/> class.
        /// </summary>
        /// <param name="context">Контекст бази даних, що буде використовуватися для операцій.</param>
        public TestRepository(ApplicationDbContext context)
        {
            this._context = context;
        }

        /// <summary>
        /// Асинхронно створює новий запис про тест у базі даних.
        /// </summary>
        /// <param name="creatorId">Ідентифікатор користувача, який створює тест.</param>
        /// <param name="flashcardIds">Список ідентифікаторів флеш-карток (використовується BLL, а не DAL).</param>
        /// <param name="ct">Токен скасування операції.</param>
        /// <returns>Створена сутність <see cref="Test"/>.</returns>
        public async Task<Test> CreateFromFlashcardsAsync(int creatorId, IEnumerable<int> flashcardIds, CancellationToken ct)
        {
            var test = new Test
            {
                CreatorId = creatorId,
            };

            this._context.Tests.Add(test);
            await this._context.SaveChangesAsync(ct);
            return test;
        }

        /// <summary>
        /// Асинхронно отримує один тест за його ID.
        /// </summary>
        /// <param name="id">Ідентифікатор тесту.</param>
        /// <param name="ct">Токен скасування операції.</param>
        /// <returns>Знайдений <see cref="Test"/> або null.</returns>
        public async Task<Test?> GetAsync(int id, CancellationToken ct)
        {
            // Отримання сутності Test за її ID.
            return await this._context.Tests
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync(t => t.TestId == id, ct);
        }
    }
}