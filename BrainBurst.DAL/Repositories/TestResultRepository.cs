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
    /// Реалізація репозиторію для роботи з сутностями <see cref="TestResult"/>.
    /// </summary>
    public class TestResultRepository : ITestResultRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TestResultRepository> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestResultRepository"/> class.
        /// </summary>
        /// <param name="context">Контекст бази даних, що буде використовуватися для операцій.</param>
        /// <param name="logger">Логер для запису подій.</param>
        public TestResultRepository(ApplicationDbContext context, ILogger<TestResultRepository> logger)
        {
            this._context = context;
            this._logger = logger;

            this._logger.LogDebug("TestResultRepository: Репозиторій результатів тесту ініціалізовано.");
        }

        /// <summary>
        /// Асинхронно додає новий результат тесту та пов'язані з ним відповіді на питання.
        /// </summary>
        /// <param name="tr">Сутність <see cref="TestResult"/> для додавання.</param>
        /// <param name="qr">Список сутностей <see cref="QuestionResult"/>, пов'язаних з цим результатом.</param>
        /// <param name="ct">Токен скасування операції.</param>
        /// <returns>Додана сутність <see cref="TestResult"/>.</returns>
        public async Task<TestResult> AddAsync(TestResult tr, IEnumerable<QuestionResult> qr, CancellationToken ct)
        {
            var answerCount = qr.Count();
            this._logger.LogInformation("AddAsync: Збереження результату тесту {TestId} для користувача {UserId} з {Count} відповідей.", tr.TestId, tr.UserId, answerCount);

            try
            {
                this._context.TestResults.Add(tr);

                foreach (var q in qr)
                {
                    tr.QuestionResults.Add(q);
                }

                await this._context.SaveChangesAsync(ct);

                this._logger.LogInformation("AddAsync: Результат тесту {TestId} успішно збережено. ResultId: {ResultId}.", tr.TestId, tr.TestResultId);
                return tr;
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "AddAsync: Критична помилка БД при збереженні результату тесту {TestId} для користувача {UserId}.", tr.TestId, tr.UserId);
                throw;
            }
        }

        /// <summary>
        /// Асинхронно отримує всі результати тестів для конкретного користувача.
        /// </summary>
        /// <param name="userId">Ідентифікатор користувача, чиї результати потрібно знайти.</param>
        /// <param name="ct">Токен скасування операції.</param>
        /// <returns>Список <see cref="TestResult"/>, доступний лише для читання, включаючи пов'язані відповіді.</returns>
        public async Task<IReadOnlyList<TestResult>> GetByUserAsync(int userId, CancellationToken ct)
        {
            this._logger.LogDebug("GetByUserAsync: Запит результатів тестів для користувача {UserId}.", userId);
            try
            {
                var results = await this._context.TestResults
                    .Where(tr => tr.UserId == userId)
                    .Include(tr => tr.QuestionResults)
                    .OrderByDescending(tr => tr.TestDate)
                    .AsNoTracking()
                    .ToListAsync(ct);

                this._logger.LogInformation("GetByUserAsync: Знайдено {Count} результатів тестів для користувача {UserId}.", results.Count, userId);
                return results;
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "GetByUserAsync: Критична помилка БД при отриманні результатів для користувача {UserId}.", userId);
                throw;
            }
        }
    }
}