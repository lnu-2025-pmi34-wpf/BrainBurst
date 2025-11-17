namespace BrainBurst.BLL.Services
{
    using System.Linq;
    using BrainBurst.BLL.DTO;
    using BrainBurst.BLL.Interfaces;
    using BrainBurst.DAL.Abstractions;

    public class ArchiveService : IArchiveService
    {
        private readonly ITestResultRepository _results;

        public ArchiveService(ITestResultRepository results)
        {
            this._results = results;
        }

        public async Task<IReadOnlyList<ArchiveEntryDTO>> GetArchiveAsync(int userId, CancellationToken ct)
        {
            // 1. Отримуємо всі результати тестів для користувача з DAL
            var testResults = await this._results.GetByUserAsync(userId, ct);

            var archive = new List<ArchiveEntryDTO>();

            // 2. Конвертуємо результати у DTO для відображення в архіві
            foreach (var tr in testResults)
            {
                // Примітка: Оскільки сутність Test у DAL не містить поля Title,
                // ми використовуємо умовну назву. У реальному проекті потрібне
                // додаткове поле або логіка визначення назви тесту.
                string title = $"Завершений Тест №{tr.TestId}";

                archive.Add(new ArchiveEntryDTO
                {
                    TestResultId = tr.TestResultId,
                    TestTitle = title,
                    CorrectAnswersPercent = (double)tr.CorrectAnswersPercent,
                    Points = tr.Points,
                    TestDate = tr.TestDate
                });
            }

            return archive;
        }
    }
}