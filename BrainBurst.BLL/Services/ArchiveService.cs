namespace BrainBurst.BLL.Services
{
    using System.Linq;
    using BrainBurst.BLL.DTO;
    using BrainBurst.BLL.Interfaces;
    using BrainBurst.DAL.Abstractions;

    /// <summary>
    /// Реалізація сервісу, що відповідає за отримання архіву тестів.
    /// </summary>
    public class ArchiveService : IArchiveService
    {
        private readonly ITestResultRepository _results;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArchiveService"/> class.
        /// </summary>
        /// <param name="results">Репозиторій для доступу до результатів тестів.</param>
        public ArchiveService(ITestResultRepository results)
        {
            this._results = results;
        }

        /// <summary>
        /// Асинхронно отримує архів пройдених тестів для конкретного користувача.
        /// </summary>
        /// <param name="userId">Ідентифікатор користувача, чий архів потрібно отримати.</param>
        /// <param name="ct">Токен скасування операції.</param>
        /// <returns>Список <see cref="ArchiveEntryDTO"/>, доступний лише для читання.</returns>
        public async Task<IReadOnlyList<ArchiveEntryDTO>> GetArchiveAsync(int userId, CancellationToken ct)
        {
            var testResults = await this._results.GetByUserAsync(userId, ct);

            var archive = new List<ArchiveEntryDTO>();

            foreach (var tr in testResults)
            {
                string title = $"Завершений Тест №{tr.TestId}";

                archive.Add(new ArchiveEntryDTO
                {
                    TestResultId = tr.TestResultId,
                    TestTitle = title,
                    CorrectAnswersPercent = (double)tr.CorrectAnswersPercent,
                    Points = tr.Points,
                    TestDate = tr.TestDate,
                });
            }

            return archive;
        }
    }
}