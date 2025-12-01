namespace BrainBurst.BLL.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BrainBurst.BLL.DTO;
    using BrainBurst.BLL.Interfaces;
    using BrainBurst.DAL.Abstractions;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Реалізація сервісу, що відповідає за отримання архіву тестів.
    /// </summary>
    public class ArchiveService : IArchiveService
    {
        private readonly ITestResultRepository _results;
        private readonly ILogger<ArchiveService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArchiveService"/> class.
        /// </summary>
        /// <param name="results">Репозиторій для доступу до результатів тестів.</param>
        /// /// <param name="logger">Логер для запису подій.</param>
        public ArchiveService(ITestResultRepository results, ILogger<ArchiveService> logger)
        {
            this._results = results;
            this._logger = logger;

            this._logger.LogDebug("ArchiveService: Сервіс архіву ініціалізовано.");
        }

        /// <summary>
        /// Асинхронно отримує архів пройдених тестів для конкретного користувача.
        /// </summary>
        /// <param name="userId">Ідентифікатор користувача, чий архів потрібно отримати.</param>
        /// <param name="ct">Токен скасування операції.</param>
        /// <returns>Список <see cref="ArchiveEntryDTO"/>, доступний лише для читання.</returns>
        public async Task<IReadOnlyList<ArchiveEntryDTO>> GetArchiveAsync(int userId, CancellationToken ct)
        {
            this._logger.LogInformation("GetArchiveAsync: Запит архіву результатів для користувача {UserId}.", userId);

            try
            {
                var testResults = await this._results.GetByUserAsync(userId, ct);

                this._logger.LogDebug("GetArchiveAsync: Отримано {Count} результатів тестів з репозиторію.", testResults.Count);

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

                this._logger.LogInformation("GetArchiveAsync: Успішно створено {Count} записів архіву.", archive.Count);

                return archive;
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "GetArchiveAsync: Критична помилка при отриманні архіву для користувача {UserId}.", userId);
                throw;
            }
        }
    }
}