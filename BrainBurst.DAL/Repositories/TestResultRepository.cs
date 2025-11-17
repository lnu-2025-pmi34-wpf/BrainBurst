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
    /// Реалізація репозиторію для роботи з сутностями <see cref="TestResult"/>.
    /// </summary>
    public class TestResultRepository : ITestResultRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestResultRepository"/> class.
        /// </summary>
        /// <param name="context">Контекст бази даних, що буде використовуватися для операцій.</param>
        public TestResultRepository(ApplicationDbContext context)
        {
            this._context = context;
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
            this._context.TestResults.Add(tr);

            foreach (var q in qr)
            {
                tr.QuestionResults.Add(q);
            }

            await this._context.SaveChangesAsync(ct);
            return tr;
        }

        /// <summary>
        /// Асинхронно отримує всі результати тестів для конкретного користувача.
        /// </summary>
        /// <param name="userId">Ідентифікатор користувача, чиї результати потрібно знайти.</param>
        /// <param name="ct">Токен скасування операції.</param>
        /// <returns>Список <see cref="TestResult"/>, доступний лише для читання, включаючи пов'язані відповіді.</returns>
        public async Task<IReadOnlyList<TestResult>> GetByUserAsync(int userId, CancellationToken ct)
        {
            // Отримуємо всі результати тестів для конкретного користувача,
            // включаючи пов'язані результати питань, для відображення архіву.
            return await this._context.TestResults
                                 .Where(tr => tr.UserId == userId)
                                 .Include(tr => tr.QuestionResults)
                                 .OrderByDescending(tr => tr.TestDate)
                                 .AsNoTracking()
                                 .ToListAsync(ct);
        }
    }
}