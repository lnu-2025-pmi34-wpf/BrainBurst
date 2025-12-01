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
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Реалізація репозиторію для роботи з сутностями <see cref="Test"/>.
    /// </summary>
    public class TestRepository : ITestRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TestRepository> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestRepository"/> class.
        /// </summary>
        /// <param name="context">Контекст бази даних, що буде використовуватися для операцій.</param>
        /// <param name="logger">Логер для запису подій.</param>
        public TestRepository(ApplicationDbContext context, ILogger<TestRepository> logger)
        {
            this._context = context;
            this._logger = logger;

            this._logger.LogDebug("TestRepository: Репозиторій тестів ініціалізовано.");
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
            var cardCount = flashcardIds?.Count() ?? 0;
            this._logger.LogInformation("CreateFromFlashcardsAsync: Створення тесту для {CreatorId} з {Count} карток.", creatorId, cardCount);
            try
            {
                var test = new Test
                {
                    CreatorId = creatorId,
                };

                this._context.Tests.Add(test);
                await this._context.SaveChangesAsync(ct);

                this._logger.LogInformation("CreateFromFlashcardsAsync: Запис тесту {TestId} успішно створено.", test.TestId);

                return test;
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "CreateFromFlashcardsAsync: Критична помилка БД при створенні тесту для CreatorId: {CreatorId}", creatorId);
                throw;
            }
        }

        /// <summary>
        /// Асинхронно отримує один тест за його ID.
        /// </summary>
        /// <param name="id">Ідентифікатор тесту.</param>
        /// <param name="ct">Токен скасування операції.</param>
        /// <returns>Знайдений <see cref="Test"/> або null.</returns>
        public async Task<Test?> GetAsync(int id, CancellationToken ct)
        {
            this._logger.LogDebug("GetAsync: Запит тесту за ID: {TestId}.", id);

            try
            {
                var test = await this._context.Tests
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.TestId == id, ct);

                if (test == null)
                {
                    this._logger.LogDebug("GetAsync: Тест {TestId} не знайдено.", id);
                }

                return test;
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "GetAsync: Критична помилка БД при отриманні тесту {TestId}.", id);
                throw;
            }
        }
    }
}