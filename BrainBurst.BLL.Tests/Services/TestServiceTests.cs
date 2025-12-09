namespace BrainBurst.BLL.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BrainBurst.BLL.Interfaces;
    using BrainBurst.BLL.Services;
    using BrainBurst.DAL.Abstractions;
    using BrainBurst.DAL.Entities;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// Юніт-тести для сервісу <see cref="TestService"/>.
    /// Забезпечують покриття ВСІХ гілок логіки.
    /// </summary>
    public class TestServiceTests
    {
        private readonly Mock<ITestRepository> _testsMock;
        private readonly Mock<ITestResultRepository> _resultsMock;
        private readonly Mock<IUserRepository> _usersMock;
        private readonly Mock<IFlashcardRepository> _cardsMock;
        private readonly Mock<IRatingService> _ratingMock;
        private readonly Mock<ILogger<TestService>> _loggerMock;

        private readonly TestService _service;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestServiceTests"/> class.
        /// Ініціалізація тестового оточення.
        /// </summary>
        public TestServiceTests()
        {
            this._testsMock = new Mock<ITestRepository>(MockBehavior.Strict);
            this._resultsMock = new Mock<ITestResultRepository>(MockBehavior.Strict);
            this._usersMock = new Mock<IUserRepository>(MockBehavior.Strict);
            this._cardsMock = new Mock<IFlashcardRepository>(MockBehavior.Strict);
            this._ratingMock = new Mock<IRatingService>(MockBehavior.Strict);
            this._loggerMock = new Mock<ILogger<TestService>>(MockBehavior.Loose);

            this._service = new TestService(
                this._testsMock.Object,
                this._resultsMock.Object,
                this._usersMock.Object,
                this._cardsMock.Object,
                this._ratingMock.Object,
                this._loggerMock.Object);
        }

        // =====================================================================
        // ===========================  GENERATE  ===============================
        // =====================================================================

        /// <summary>
        /// Якщо flashcardIds == null → кидає ArgumentException.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GenerateFromFlashcardsAsync_Null_ThrowsArgumentException()
        {
            int creatorId = 1;
            IEnumerable<int>? ids = null;
            var ct = CancellationToken.None;

            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                this._service.GenerateFromFlashcardsAsync(creatorId, ids!, ct));

            Assert.Contains("Потрібен хоча б один flashcardId", ex.Message);
        }

        /// <summary>
        /// Якщо flashcardIds порожній → також ArgumentException.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GenerateFromFlashcardsAsync_Empty_ThrowsArgumentException()
        {
            int creatorId = 1;
            var ct = CancellationToken.None;

            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                this._service.GenerateFromFlashcardsAsync(creatorId, Array.Empty<int>(), ct));

            Assert.Contains("Потрібен хоча б один flashcardId", ex.Message);
        }

        /// <summary>
        /// Перевіряє базовий позитивний сценарій генерації тесту:
        /// виклик репозиторіїв, мапінг та коректність DTO.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GenerateFromFlashcardsAsync_Valid_CreatesTestCorrectly()
        {
            int creatorId = 99;
            var ids = new[] { 10, 20 };
            var ct = CancellationToken.None;

            var testEntity = new Test { TestId = 777, CreatorId = creatorId };

            this._testsMock
                .Setup(r => r.CreateFromFlashcardsAsync(creatorId, ids, ct))
                .ReturnsAsync(testEntity);

            var cards = new List<Flashcard>
            {
                new Flashcard { FlashcardId = 10, Question = "Q1", Answer = "A1", CreatorId = creatorId },
                new Flashcard { FlashcardId = 20, Question = "Q2", Answer = "A2", CreatorId = creatorId },
                new Flashcard { FlashcardId = 30, Question = "Q3", Answer = "A3", CreatorId = creatorId },
            };

            this._cardsMock
                .Setup(r => r.FindAsync(creatorId, null, ct))
                .ReturnsAsync(cards);

            var dto = await this._service.GenerateFromFlashcardsAsync(creatorId, ids, ct);

            Assert.NotNull(dto);
            Assert.Equal(777, dto.Id);
            Assert.Equal(creatorId, dto.CreatorId);
            Assert.Equal(2, dto.Questions.Count);

            Assert.Contains(dto.Questions, q => q.Id == 10);
            Assert.Contains(dto.Questions, q => q.Id == 20);

            this._testsMock.Verify(r => r.CreateFromFlashcardsAsync(creatorId, ids, ct), Times.Once);
            this._cardsMock.Verify(r => r.FindAsync(creatorId, null, ct), Times.Once);
        }

        /// <summary>
        /// Якщо репозиторій тестів кидає будь-який інший виняток → він логуються і проброшується далі.
        /// Покриваємо catch (Exception) у GenerateFromFlashcardsAsync.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GenerateFromFlashcardsAsync_RepositoryThrows_LogsAndRethrows()
        {
            int creatorId = 1;
            var ids = new[] { 42 };
            var ct = CancellationToken.None;

            this._testsMock
                .Setup(r => r.CreateFromFlashcardsAsync(creatorId, ids, ct))
                .ThrowsAsync(new InvalidOperationException("DB failure"));

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                this._service.GenerateFromFlashcardsAsync(creatorId, ids, ct));

            this._testsMock.Verify(r => r.CreateFromFlashcardsAsync(creatorId, ids, ct), Times.Once);
        }

        // =====================================================================
        // ===========================  GET TEST  ===============================
        // =====================================================================

        /// <summary>
        /// Якщо тест не знайдено → повертається null.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetAsync_NotFound_ReturnsNull()
        {
            int testId = 5;
            var ct = CancellationToken.None;

            this._testsMock
                .Setup(r => r.GetAsync(testId, ct))
                .ReturnsAsync((Test?)null);

            var dto = await this._service.GetAsync(testId, ct);

            Assert.Null(dto);
            this._testsMock.Verify(r => r.GetAsync(testId, ct), Times.Once);
        }

        /// <summary>
        /// Позитивний сценарій: тест знайдено → DTO повертається без питань.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetAsync_Found_ReturnsDto()
        {
            int testId = 10;
            var ct = CancellationToken.None;

            this._testsMock
                .Setup(r => r.GetAsync(testId, ct))
                .ReturnsAsync(new Test { TestId = 10, CreatorId = 3 });

            var dto = await this._service.GetAsync(testId, ct);

            Assert.NotNull(dto);
            Assert.Equal(10, dto!.Id);
            Assert.Equal(3, dto.CreatorId);
            Assert.Empty(dto.Questions);
        }

        /// <summary>
        /// Якщо репозиторій кидає виняток у GetAsync → сервіс логуює помилку і проброшує її далі.
        /// Покриваємо catch (Exception) у GetAsync.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetAsync_RepositoryThrows_LogsAndRethrows()
        {
            int testId = 123;
            var ct = CancellationToken.None;

            this._testsMock
                .Setup(r => r.GetAsync(testId, ct))
                .ThrowsAsync(new Exception("Unexpected repository error"));

            await Assert.ThrowsAsync<Exception>(() =>
                this._service.GetAsync(testId, ct));

            this._testsMock.Verify(r => r.GetAsync(testId, ct), Times.Once);
        }

        /// <summary>
        /// Якщо репозиторій кидає саме KeyNotFoundException,
        /// сервіс повинен просто пробросити його далі (гілка без додаткового логування).
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetAsync_RepositoryThrowsKeyNotFound_RethrowsWithoutExtraLogging()
        {
            int testId = 999;
            var ct = CancellationToken.None;

            this._testsMock
                .Setup(r => r.GetAsync(testId, ct))
                .ThrowsAsync(new KeyNotFoundException("Test not found"));

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                this._service.GetAsync(testId, ct));

            this._testsMock.Verify(r => r.GetAsync(testId, ct), Times.Once);
        }

        // =====================================================================
        // ===========================  SUBMIT  =================================
        // =====================================================================

        /// <summary>
        /// Якщо answers порожній → ArgumentException.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task SubmitAsync_EmptyAnswers_ThrowsArgumentException()
        {
            var ct = CancellationToken.None;

            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                this._service.SubmitAsync(1, 2, Array.Empty<(int, string?)>(), ct));

            Assert.Contains("Відповіді відсутні", ex.Message);
        }

        /// <summary>
        /// Якщо тест не знайдено → InvalidOperationException.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task SubmitAsync_TestNotFound_Throws()
        {
            int testId = 1;
            var ct = CancellationToken.None;

            this._testsMock
                .Setup(r => r.GetAsync(testId, ct))
                .ReturnsAsync((Test?)null);

            var answers = new (int flashcardId, string? userInput)[] { (1, "a") };

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                this._service.SubmitAsync(testId, 10, answers, ct));

            Assert.Contains("Тест не знайдено", ex.Message);
        }

        /// <summary>
        /// Частково правильні відповіді → перевіряється процент, бали, оновлення юзера.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task SubmitAsync_PartialCorrect_ComputesScoreCorrectly()
        {
            int testId = 10;
            int userId = 33;
            var ct = CancellationToken.None;

            var answers = new[]
            {
                (1, (string?)"Answer1"),
                (2, (string?)"wrong"),
                (3, (string?)"answer3 "),
            };

            this._testsMock
                .Setup(r => r.GetAsync(testId, ct))
                .ReturnsAsync(new Test { TestId = testId, CreatorId = 999 });

            var user = new User { UserId = userId, Points = 40 };
            this._usersMock
                .Setup(r => r.GetByIdAsync(userId, ct))
                .ReturnsAsync(user);

            this._cardsMock
                .Setup(r => r.FindAsync(999, null, ct))
                .ReturnsAsync(new List<Flashcard>
                {
                    new Flashcard { FlashcardId = 1, Answer = "answer1" },
                    new Flashcard { FlashcardId = 2, Answer = "correct2" },
                    new Flashcard { FlashcardId = 3, Answer = "Answer3" },
                });

            TestResult? savedResult = null;

            this._resultsMock
                .Setup(r => r.AddAsync(
                    It.IsAny<TestResult>(),
                    It.IsAny<IEnumerable<QuestionResult>>(),
                    ct))
                .Callback<TestResult, IEnumerable<QuestionResult>, CancellationToken>((tr, _, _) =>
                {
                    savedResult = tr;
                })
                .ReturnsAsync((TestResult tr, IEnumerable<QuestionResult> _, CancellationToken _) => tr);

            this._usersMock
                .Setup(r => r.UpdateAsync(user, ct))
                .Returns(Task.CompletedTask);

            var dto = await this._service.SubmitAsync(testId, userId, answers, ct);

            Assert.NotNull(savedResult);

            int correct = 2;
            double percent = 100.0 * correct / answers.Length;
            int expectedPoints = correct * 10;

            Assert.Equal((decimal)percent, savedResult!.CorrectAnswersPercent);
            Assert.Equal(expectedPoints, savedResult.Points);
            Assert.Equal(40 + expectedPoints, user.Points);
        }

        /// <summary>
        /// 100% правильні відповіді → нараховується бонус.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task SubmitAsync_AllCorrect_GivesBonus()
        {
            int testId = 3;
            int userId = 5;
            var ct = CancellationToken.None;

            var answers = new[]
            {
                (1, (string?)"a"),
                (2, (string?)"b"),
            };

            this._testsMock
                .Setup(r => r.GetAsync(testId, ct))
                .ReturnsAsync(new Test { TestId = testId, CreatorId = 5 });

            var user = new User { UserId = userId, Points = 10 };

            this._usersMock
                .Setup(r => r.GetByIdAsync(userId, ct))
                .ReturnsAsync(user);

            this._cardsMock
                .Setup(r => r.FindAsync(5, null, ct))
                .ReturnsAsync(new List<Flashcard>
                {
                    new Flashcard { FlashcardId = 1, Answer = "a" },
                    new Flashcard { FlashcardId = 2, Answer = "b" },
                });

            TestResult? saved = null;

            this._resultsMock
                .Setup(r => r.AddAsync(
                    It.IsAny<TestResult>(),
                    It.IsAny<IEnumerable<QuestionResult>>(),
                    ct))
                .Callback<TestResult, IEnumerable<QuestionResult>, CancellationToken>((tr, _, _) =>
                {
                    saved = tr;
                })
                .ReturnsAsync((TestResult tr, IEnumerable<QuestionResult> _, CancellationToken _) => tr);

            this._usersMock
                .Setup(r => r.UpdateAsync(user, ct))
                .Returns(Task.CompletedTask);

            var dto = await this._service.SubmitAsync(testId, userId, answers, ct);

            Assert.NotNull(saved);

            int expected = (answers.Length * 10) + 20;

            Assert.Equal(expected, saved!.Points);
            Assert.Equal(expected + 10, user.Points);
        }

        /// <summary>
        /// Якщо FlashcardId відсутній → відповідь уважається неправильною.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task SubmitAsync_MissingFlashcard_MarkedIncorrect()
        {
            int testId = 88;
            int userId = 2;
            var ct = CancellationToken.None;

            var answers = new[]
            {
                (1, (string?)"aaa"),
                (99, (string?)"zzz"),
            };

            this._testsMock
                .Setup(r => r.GetAsync(testId, ct))
                .ReturnsAsync(new Test { TestId = testId, CreatorId = 100 });

            this._usersMock
                .Setup(r => r.GetByIdAsync(userId, ct))
                .ReturnsAsync(new User { UserId = userId });

            this._cardsMock
                .Setup(r => r.FindAsync(100, null, ct))
                .ReturnsAsync(new List<Flashcard>
                {
                    new Flashcard { FlashcardId = 1, Answer = "aaa" },
                });

            IEnumerable<QuestionResult>? captured = null;

            this._resultsMock
                .Setup(r => r.AddAsync(
                    It.IsAny<TestResult>(),
                    It.IsAny<IEnumerable<QuestionResult>>(),
                    ct))
                .Callback<TestResult, IEnumerable<QuestionResult>, CancellationToken>((tr, qs, _) =>
                {
                    captured = qs;
                })
                .ReturnsAsync((TestResult tr, IEnumerable<QuestionResult> qs, CancellationToken _) => tr);

            this._usersMock
                .Setup(r => r.UpdateAsync(It.IsAny<User>(), ct))
                .Returns(Task.CompletedTask);

            await this._service.SubmitAsync(testId, userId, answers, ct);

            var list = captured!.ToList();

            Assert.True(list[0].IsCorrect);
            Assert.False(list[1].IsCorrect); // Flashcard 99 не існує
        }

        /// <summary>
        /// Перевіряє, що FindAsync викликається з CreatorId, а не з userId.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task SubmitAsync_UsesCreatorIdCorrectly()
        {
            int testId = 77;
            int userId = 5;
            int creatorId = 123;
            var ct = CancellationToken.None;

            this._testsMock
                .Setup(r => r.GetAsync(testId, ct))
                .ReturnsAsync(new Test { TestId = testId, CreatorId = creatorId });

            this._usersMock
                .Setup(r => r.GetByIdAsync(userId, ct))
                .ReturnsAsync(new User { UserId = userId });

            this._cardsMock
                .Setup(r => r.FindAsync(creatorId, null, ct))
                .ReturnsAsync(new List<Flashcard>
                {
                    new Flashcard { FlashcardId = 1, Answer = "x" },
                });

            this._resultsMock
                .Setup(r => r.AddAsync(
                    It.IsAny<TestResult>(),
                    It.IsAny<IEnumerable<QuestionResult>>(),
                    ct))
                .ReturnsAsync((TestResult tr, IEnumerable<QuestionResult> qs, CancellationToken _) => tr);

            this._usersMock
                .Setup(r => r.UpdateAsync(It.IsAny<User>(), ct))
                .Returns(Task.CompletedTask);

            await this._service.SubmitAsync(testId, userId, new[] { (1, (string?)"x") }, ct);

            this._cardsMock.Verify(r => r.FindAsync(creatorId, null, ct), Times.Once);
        }

        /// <summary>
        /// Якщо при збереженні результату виникає неспецифічний виняток,
        /// він має бути залогований (LogError) і проброшений.
        /// Покриваємо гілку if (ex is not ArgumentException ... ) у SubmitAsync.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task SubmitAsync_UnexpectedException_LogsErrorAndRethrows()
        {
            int testId = 55;
            int userId = 7;
            var ct = CancellationToken.None;

            var answers = new[]
            {
                (1, (string?)"a"),
            };

            this._testsMock
                .Setup(r => r.GetAsync(testId, ct))
                .ReturnsAsync(new Test { TestId = testId, CreatorId = 99 });

            this._usersMock
                .Setup(r => r.GetByIdAsync(userId, ct))
                .ReturnsAsync(new User { UserId = userId, Points = 0 });

            this._cardsMock
                .Setup(r => r.FindAsync(99, null, ct))
                .ReturnsAsync(new List<Flashcard>
                {
                    new Flashcard { FlashcardId = 1, Answer = "a" },
                });

            this._resultsMock
                .Setup(r => r.AddAsync(
                    It.IsAny<TestResult>(),
                    It.IsAny<IEnumerable<QuestionResult>>(),
                    ct))
                .ThrowsAsync(new Exception("DB failure on AddAsync"));

            await Assert.ThrowsAsync<Exception>(() =>
                this._service.SubmitAsync(testId, userId, answers, ct));

            this._testsMock.Verify(r => r.GetAsync(testId, ct), Times.Once);
            this._usersMock.Verify(r => r.GetByIdAsync(userId, ct), Times.Once);
            this._cardsMock.Verify(r => r.FindAsync(99, null, ct), Times.Once);
            this._resultsMock.Verify(
                r => r.AddAsync(
                    It.IsAny<TestResult>(),
                    It.IsAny<IEnumerable<QuestionResult>>(),
                    ct),
                Times.Once);
        }

        /// <summary>
        /// Якщо користувача не знайдено (GetByIdAsync кидає KeyNotFoundException),
        /// SubmitAsync повинен пробросити виняток далі без логування як "критичної" помилки.
        /// Це покриває гілку, де (ex is not ArgumentException and ex is not InvalidOperationException and ex is not KeyNotFoundException) == false.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task SubmitAsync_UserNotFound_KeyNotFoundRethrownWithoutCriticalLog()
        {
            int testId = 42;
            int userId = 123;
            var ct = CancellationToken.None;

            var answers = new[]
            {
                (1, (string?)"some answer"),
            };

            // Тест існує, щоб пройти першу частину логіки
            this._testsMock
                .Setup(r => r.GetAsync(testId, ct))
                .ReturnsAsync(new Test { TestId = testId, CreatorId = 10 });

            // На етапі завантаження користувача репозиторій кидає KeyNotFoundException
            this._usersMock
                .Setup(r => r.GetByIdAsync(userId, ct))
                .ThrowsAsync(new KeyNotFoundException("User not found"));

            // Cards / results не повинні навіть викликатися
            this._cardsMock
                .Setup(r => r.FindAsync(It.IsAny<int>(), It.IsAny<string?>(), ct))
                .ReturnsAsync(new List<Flashcard>());

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                this._service.SubmitAsync(testId, userId, answers, ct));

            this._testsMock.Verify(r => r.GetAsync(testId, ct), Times.Once);
            this._usersMock.Verify(r => r.GetByIdAsync(userId, ct), Times.Once);
            this._cardsMock.Verify(r => r.FindAsync(It.IsAny<int>(), It.IsAny<string?>(), ct), Times.Never);
            this._resultsMock.Verify(
                r => r.AddAsync(
                    It.IsAny<TestResult>(),
                    It.IsAny<IEnumerable<QuestionResult>>(),
                    ct),
                Times.Never);
        }
    }
}
