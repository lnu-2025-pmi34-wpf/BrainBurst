namespace BrainBurst.BLL.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BrainBurst.BLL.DTO;
    using BrainBurst.BLL.Services;
    using BrainBurst.DAL.Abstractions;
    using BrainBurst.DAL.Entities;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// Містить юніт-тести для <see cref="ArchiveService"/>.
    /// </summary>
    public class ArchiveServiceTests
    {
        private readonly Mock<ITestResultRepository> resultsMock;
        private readonly Mock<ILogger<ArchiveService>> loggerMock;
        private readonly ArchiveService service;
        private readonly CancellationToken ct = CancellationToken.None;

        /// <summary>
        /// Конструктор тестів: налаштовує "моки" для залежностей <see cref="ArchiveService"/>.
        /// </summary>
        public ArchiveServiceTests()
        {
            this.resultsMock = new Mock<ITestResultRepository>(MockBehavior.Strict);
            this.loggerMock = new Mock<ILogger<ArchiveService>>(MockBehavior.Loose);

            this.service = new ArchiveService(
                this.resultsMock.Object,
                this.loggerMock.Object);
        }

        /// <summary>
        /// Тест: GetArchiveAsync викликає репозиторій з коректними userId та CancellationToken.
        /// </summary>
        [Fact]
        public async Task GetArchiveAsync_CallsRepositoryWithUserIdAndCancellationToken()
        {
            int userId = 42;

            this.resultsMock
                .Setup(r => r.GetByUserAsync(userId, this.ct))
                .ReturnsAsync(new List<TestResult>());

            var result = await this.service.GetArchiveAsync(userId, this.ct);

            this.resultsMock.Verify(r => r.GetByUserAsync(userId, this.ct), Times.Once);
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        /// <summary>
        /// Тест: GetArchiveAsync повертає порожній список, якщо репозиторій не повертає результатів.
        /// </summary>
        [Fact]
        public async Task GetArchiveAsync_NoResults_ReturnsEmptyList()
        {
            int userId = 5;

            this.resultsMock
                .Setup(r => r.GetByUserAsync(userId, this.ct))
                .ReturnsAsync(new List<TestResult>());

            var archive = await this.service.GetArchiveAsync(userId, this.ct);

            Assert.NotNull(archive);
            Assert.Empty(archive);
        }

        /// <summary>
        /// Тест: GetArchiveAsync коректно мапить один TestResult в один ArchiveEntryDTO.
        /// </summary>
        [Fact]
        public async Task GetArchiveAsync_MapsSingleResultCorrectly()
        {
            int userId = 7;
            var testDate = new DateTime(2025, 1, 2, 3, 4, 5, DateTimeKind.Utc);

            var results = new List<TestResult>
            {
                new TestResult
                {
                    TestResultId = 100,
                    TestId = 200,
                    CorrectAnswersPercent = 87.5m,
                    Points = 35,
                    TestDate = testDate,
                },
            };

            this.resultsMock
                .Setup(r => r.GetByUserAsync(userId, this.ct))
                .ReturnsAsync(results);

            var archive = await this.service.GetArchiveAsync(userId, this.ct);

            var entry = Assert.Single(archive);

            Assert.Equal(100, entry.TestResultId);
            Assert.Equal("Завершений Тест №200", entry.TestTitle);
            Assert.Equal(35, entry.Points);
            Assert.Equal(testDate, entry.TestDate);
            Assert.Equal(87.5d, entry.CorrectAnswersPercent, 10);
        }

        /// <summary>
        /// Тест: GetArchiveAsync коректно мапить декілька результатів, зберігаючи їх порядок.
        /// </summary>
        [Fact]
        public async Task GetArchiveAsync_MultipleResults_PreservesOrderAndMapsAllEntries()
        {
            int userId = 10;

            var results = new List<TestResult>
            {
                new TestResult
                {
                    TestResultId = 1,
                    TestId = 11,
                    CorrectAnswersPercent = 50m,
                    Points = 10,
                    TestDate = new DateTime(2025, 1, 1),
                },
                new TestResult
                {
                    TestResultId = 2,
                    TestId = 22,
                    CorrectAnswersPercent = 100m,
                    Points = 40,
                    TestDate = new DateTime(2025, 2, 2),
                },
            };

            this.resultsMock
                .Setup(r => r.GetByUserAsync(userId, this.ct))
                .ReturnsAsync(results);

            var archive = await this.service.GetArchiveAsync(userId, this.ct);

            Assert.Equal(2, archive.Count);

            Assert.Equal(1, archive[0].TestResultId);
            Assert.Equal("Завершений Тест №11", archive[0].TestTitle);
            Assert.Equal(50d, archive[0].CorrectAnswersPercent, 10);
            Assert.Equal(10, archive[0].Points);
            Assert.Equal(results[0].TestDate, archive[0].TestDate);

            Assert.Equal(2, archive[1].TestResultId);
            Assert.Equal("Завершений Тест №22", archive[1].TestTitle);
            Assert.Equal(100d, archive[1].CorrectAnswersPercent, 10);
            Assert.Equal(40, archive[1].Points);
            Assert.Equal(results[1].TestDate, archive[1].TestDate);
        }

        /// <summary>
        /// Тест: GetArchiveAsync коректно перетворює 'decimal' відсотки в 'double' без втрати точності.
        /// </summary>
        [Fact]
        public async Task GetArchiveAsync_CorrectAnswersPercent_DecimalToDoubleIsAccurate()
        {
            int userId = 99;

            var percentages = new[] { 0m, 33.33m, 99.99m, 100m };

            var results = percentages.Select((p, i) => new TestResult
            {
                TestResultId = i + 1,
                TestId = i + 10,
                CorrectAnswersPercent = p,
                Points = i * 5,
                TestDate = new DateTime(2025, 1, 1).AddDays(i),
            }).ToList();

            this.resultsMock
                .Setup(r => r.GetByUserAsync(userId, this.ct))
                .ReturnsAsync(results);

            var archive = await this.service.GetArchiveAsync(userId, this.ct);

            Assert.Equal(percentages.Length, archive.Count);

            for (int i = 0; i < percentages.Length; i++)
            {
                double expected = (double)percentages[i];
                Assert.Equal(expected, archive[i].CorrectAnswersPercent, 10);
            }
        }

        /// <summary>
        /// Тест: якщо репозиторій кидає виняток → сервіс логуює помилку і проброшує її далі.
        /// Покриваємо catch (Exception) у GetArchiveAsync.
        /// </summary>
        [Fact]
        public async Task GetArchiveAsync_RepositoryThrows_LogsErrorAndRethrows()
        {
            int userId = 123;

            this.resultsMock
                .Setup(r => r.GetByUserAsync(userId, this.ct))
                .ThrowsAsync(new InvalidOperationException("DB error"));

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                this.service.GetArchiveAsync(userId, this.ct));

            this.resultsMock.Verify(r => r.GetByUserAsync(userId, this.ct), Times.Once);
        }
    }
}