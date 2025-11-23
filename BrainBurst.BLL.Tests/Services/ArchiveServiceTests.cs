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
    using Moq;
    using Xunit;

    /// <summary>
    /// Містить юніт-тести для <see cref="ArchiveService"/>.
    /// </summary>
    public class ArchiveServiceTests
    {
        private readonly Mock<ITestResultRepository> _resultsMock;
        private readonly ArchiveService _service;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArchiveServiceTests"/> class.
        /// налаштовуючи "мок" (заглушку) для <see cref="ITestResultRepository"/>.
        /// </summary>
        public ArchiveServiceTests()
        {
            this._resultsMock = new Mock<ITestResultRepository>(MockBehavior.Strict);
            this._service = new ArchiveService(this._resultsMock.Object);
        }

        /// <summary>
        /// Тест: GetArchiveAsync викликає репозиторій з коректними userId та CancellationToken.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetArchiveAsync_CallsRepositoryWithUserIdAndCancellationToken()
        {
            int userId = 42;
            var ct = CancellationToken.None;

            this._resultsMock
                .Setup(r => r.GetByUserAsync(userId, ct))
                .ReturnsAsync(new List<TestResult>());

            var result = await this._service.GetArchiveAsync(userId, ct);

            this._resultsMock.Verify(r => r.GetByUserAsync(userId, ct), Times.Once);
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        /// <summary>
        /// Тест: GetArchiveAsync повертає порожній список, якщо репозиторій не повертає результатів.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetArchiveAsync_NoResults_ReturnsEmptyList()
        {
            int userId = 5;
            var ct = CancellationToken.None;

            this._resultsMock
                .Setup(r => r.GetByUserAsync(userId, ct))
                .ReturnsAsync(new List<TestResult>());

            var archive = await this._service.GetArchiveAsync(userId, ct);

            Assert.NotNull(archive);
            Assert.Empty(archive);
        }

        /// <summary>
        /// Тест: GetArchiveAsync коректно мапить один TestResult в один ArchiveEntryDTO.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetArchiveAsync_MapsSingleResultCorrectly()
        {
            int userId = 7;
            var ct = CancellationToken.None;

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

            this._resultsMock
                .Setup(r => r.GetByUserAsync(userId, ct))
                .ReturnsAsync(results);

            var archive = await this._service.GetArchiveAsync(userId, ct);

            Assert.Single(archive);
            var entry = archive[0];

            Assert.Equal(100, entry.TestResultId);
            Assert.Equal("Завершений Тест №200", entry.TestTitle);
            Assert.Equal(35, entry.Points);
            Assert.Equal(testDate, entry.TestDate);

            Assert.Equal(87.5d, entry.CorrectAnswersPercent, precision: 10);
        }

        /// <summary>
        /// Тест: GetArchiveAsync коректно мапить декілька результатів, зберігаючи їх порядок.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetArchiveAsync_MultipleResults_PreservesOrderAndMapsAllEntries()
        {
            int userId = 10;
            var ct = CancellationToken.None;

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

            this._resultsMock
                .Setup(r => r.GetByUserAsync(userId, ct))
                .ReturnsAsync(results);

            var archive = await this._service.GetArchiveAsync(userId, ct);

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

            Assert.True(archive[0].TestResultId == results[0].TestResultId &&
                        archive[1].TestResultId == results[1].TestResultId);
        }

        /// <summary>
        /// Тест: GetArchiveAsync коректно перетворює 'decimal' відсотки в 'double' без втрати точності.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetArchiveAsync_CorrectAnswersPercent_DecimalToDoubleIsAccurate()
        {
            int userId = 99;
            var ct = CancellationToken.None;

            var percentages = new[] { 0m, 33.33m, 99.99m, 100m };

            var results = percentages.Select((p, i) => new TestResult
            {
                TestResultId = i + 1,
                TestId = i + 10,
                CorrectAnswersPercent = p,
                Points = i * 5,
                TestDate = new DateTime(2025, 1, 1).AddDays(i),
            }).ToList();

            this._resultsMock
                .Setup(r => r.GetByUserAsync(userId, ct))
                .ReturnsAsync(results);

            var archive = await this._service.GetArchiveAsync(userId, ct);

            Assert.Equal(percentages.Length, archive.Count);

            for (int i = 0; i < percentages.Length; i++)
            {
                double expected = (double)percentages[i];
                Assert.Equal(expected, archive[i].CorrectAnswersPercent, 10);
            }
        }
    }
}
