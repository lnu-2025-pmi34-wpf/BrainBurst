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
    using Moq;
    using Xunit;

    /// <summary>
    /// Містить юніт-тести для <see cref="TestService"/>.
    /// </summary>
    public class TestServiceTests
    {
        private readonly Mock<ITestRepository> _testsMock;
        private readonly Mock<ITestResultRepository> _resultsMock;
        private readonly Mock<IUserRepository> _usersMock;
        private readonly Mock<IFlashcardRepository> _cardsMock;
        private readonly Mock<IRatingService> _ratingMock;

        private readonly TestService _service;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestServiceTests"/> class.
        /// налаштовуючи всі необхідні "моки" (заглушки) репозиторіїв та сервісів.
        /// </summary>
        public TestServiceTests()
        {
            this._testsMock = new Mock<ITestRepository>(MockBehavior.Strict);
            this._resultsMock = new Mock<ITestResultRepository>(MockBehavior.Strict);
            this._usersMock = new Mock<IUserRepository>(MockBehavior.Strict);
            this._cardsMock = new Mock<IFlashcardRepository>(MockBehavior.Strict);
            this._ratingMock = new Mock<IRatingService>(MockBehavior.Strict);

            this._service = new TestService(
                this._testsMock.Object,
                this._resultsMock.Object,
                this._usersMock.Object,
                this._cardsMock.Object,
                this._ratingMock.Object);
        }

        /// <summary>
        /// Тест: GenerateFromFlashcardsAsync кидає ArgumentException, якщо список ID є null.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GenerateFromFlashcardsAsync_NullFlashcardIds_ThrowsArgumentException()
        {
            int creatorId = 1;
            IEnumerable<int>? ids = null;
            var ct = CancellationToken.None;

            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                this._service.GenerateFromFlashcardsAsync(creatorId, ids!, ct));

            Assert.Contains("Потрібен хоча б один flashcardId", ex.Message);
        }

        /// <summary>
        /// Тест: GenerateFromFlashcardsAsync кидає ArgumentException, якщо список ID порожній.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GenerateFromFlashcardsAsync_EmptyFlashcardIds_ThrowsArgumentException()
        {
            int creatorId = 1;
            IEnumerable<int> ids = Array.Empty<int>();
            var ct = CancellationToken.None;

            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                this._service.GenerateFromFlashcardsAsync(creatorId, ids, ct));

            Assert.Contains("Потрібен хоча б один flashcardId", ex.Message);
        }

        /// <summary>
        /// Тест: GenerateFromFlashcardsAsync коректно викликає репозиторії та мапить результат.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GenerateFromFlashcardsAsync_CallsRepositoriesWithCorrectParameters()
        {
            int creatorId = 7;
            var flashcardIds = new[] { 10, 20 };
            var ct = CancellationToken.None;

            var testEntity = new Test
            {
                TestId = 123,
                CreatorId = creatorId,
            };

            this._testsMock
                .Setup(r => r.CreateFromFlashcardsAsync(
                    creatorId,
                    It.Is<IEnumerable<int>>(ids => ids.SequenceEqual(flashcardIds)),
                    ct))
                .ReturnsAsync(testEntity);

            var allCards = new List<Flashcard>
            {
                new Flashcard { FlashcardId = 10, Question = "Q1", Answer = "A1", CreatorId = creatorId },
                new Flashcard { FlashcardId = 20, Question = "Q2", Answer = "A2", CreatorId = creatorId },
                new Flashcard { FlashcardId = 30, Question = "Q3", Answer = "A3", CreatorId = creatorId },
            };

            this._cardsMock
                .Setup(r => r.FindAsync(creatorId, null, ct))
                .ReturnsAsync(allCards);

            var dto = await this._service.GenerateFromFlashcardsAsync(creatorId, flashcardIds, ct);

            this._testsMock.Verify(
                r => r.CreateFromFlashcardsAsync(
                creatorId,
                It.Is<IEnumerable<int>>(ids => ids.SequenceEqual(flashcardIds)),
                ct), Times.Once);

            this._cardsMock.Verify(r => r.FindAsync(creatorId, null, ct), Times.Once);

            Assert.NotNull(dto);
            Assert.Equal(testEntity.TestId, dto.Id);
            Assert.Equal(creatorId, dto.CreatorId);
            Assert.Equal(2, dto.Questions.Count);
            Assert.All(dto.Questions, q => Assert.Contains(q.Id, flashcardIds));
        }

        /// <summary>
        /// Тест: GetAsync повертає null, якщо тест не знайдено.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetAsync_NotFound_ReturnsNull()
        {
            int testId = 42;
            var ct = CancellationToken.None;

            this._testsMock
                .Setup(r => r.GetAsync(testId, ct))
                .ReturnsAsync((Test?)null);

            var dto = await this._service.GetAsync(testId, ct);

            Assert.Null(dto);
            this._testsMock.Verify(r => r.GetAsync(testId, ct), Times.Once);
        }

        /// <summary>
        /// Тест: GetAsync повертає "легкий" DTO (без питань), якщо тест знайдено.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetAsync_Found_ReturnsDtoWithEmptyQuestions()
        {
            int testId = 100;
            int creatorId = 5;
            var ct = CancellationToken.None;

            var entity = new Test
            {
                TestId = testId,
                CreatorId = creatorId,
            };

            this._testsMock
                .Setup(r => r.GetAsync(testId, ct))
                .ReturnsAsync(entity);

            var dto = await this._service.GetAsync(testId, ct);

            Assert.NotNull(dto);
            Assert.Equal(testId, dto!.Id);
            Assert.Equal(creatorId, dto.CreatorId);
            Assert.NotNull(dto.Questions);
            Assert.Empty(dto.Questions);
        }

        /// <summary>
        /// Тест: SubmitAsync кидає ArgumentException, якщо список відповідей порожній.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task SubmitAsync_EmptyAnswers_ThrowsArgumentException()
        {
            int testId = 1;
            int userId = 2;
            var answers = Array.Empty<(int flashcardId, string? userInput)>();
            var ct = CancellationToken.None;

            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                this._service.SubmitAsync(testId, userId, answers, ct));

            Assert.Contains("Відповіді відсутні", ex.Message);
        }

        /// <summary>
        /// Тест: SubmitAsync кидає InvalidOperationException, якщо тест не знайдено.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task SubmitAsync_TestNotFound_ThrowsInvalidOperationException()
        {
            int testId = 1;
            int userId = 2;
            var answers = new[] { (1, (string?)"a") };
            var ct = CancellationToken.None;

            this._testsMock
                .Setup(r => r.GetAsync(testId, ct))
                .ReturnsAsync((Test?)null);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                this._service.SubmitAsync(testId, userId, answers, ct));

            Assert.Contains("Тест не знайдено", ex.Message);
        }

        /// <summary>
        /// Тест: SubmitAsync коректно розраховує відсоток та бали при частково правильних відповідях.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task SubmitAsync_CalculatesPercentAndPoints_ForPartiallyCorrectAnswers()
        {
            int testId = 10;
            int userId = 20;
            var ct = CancellationToken.None;

            var answers = new[]
            {
                (flashcardId: 1, userInput: (string?)"  aNsWeR1 "),
                (flashcardId: 2, userInput: (string?)"wrong"),
                (flashcardId: 3, userInput: (string?)"Answer3"),
            };

            var testEntity = new Test { TestId = testId, CreatorId = 99 };
            this._testsMock
                .Setup(r => r.GetAsync(testId, ct))
                .ReturnsAsync(testEntity);

            var user = new User { UserId = userId, Points = 50 };
            this._usersMock
                .Setup(r => r.GetByIdAsync(userId, ct))
                .ReturnsAsync(user);

            var cards = new List<Flashcard>
            {
                new Flashcard { FlashcardId = 1, Answer = "answer1" },
                new Flashcard { FlashcardId = 2, Answer = "correct2" },
                new Flashcard { FlashcardId = 3, Answer = " answer3 " },
            };

            this._cardsMock
                .Setup(r => r.FindAsync(testEntity.CreatorId, null, ct))
                .ReturnsAsync(cards);

            TestResult? capturedResult = null;
            IEnumerable<QuestionResult>? capturedQuestions = null;

            this._resultsMock
                .Setup(r => r.AddAsync(
                    It.IsAny<TestResult>(),
                    It.IsAny<IEnumerable<QuestionResult>>(),
                    It.IsAny<CancellationToken>()))
                .Callback<TestResult, IEnumerable<QuestionResult>, CancellationToken>((tr, qs, _) =>
                {
                    capturedResult = tr;
                    capturedQuestions = qs;
                })
                .ReturnsAsync((TestResult tr, IEnumerable<QuestionResult> qs, CancellationToken _) => tr);

            this._usersMock
                .Setup(r => r.UpdateAsync(It.IsAny<User>(), ct))
                .Returns(Task.CompletedTask);

            var dto = await this._service.SubmitAsync(testId, userId, answers, ct);

            Assert.NotNull(capturedResult);
            Assert.NotNull(capturedQuestions);

            int correct = 2;
            int total = answers.Length;
            double expectedPercentDouble = 100.0 * correct / total;
            decimal expectedPercent = (decimal)expectedPercentDouble;
            int expectedPoints = correct * 10;

            Assert.Equal(testId, capturedResult!.TestId);
            Assert.Equal(userId, capturedResult.UserId);
            Assert.Equal(expectedPercent, capturedResult.CorrectAnswersPercent);
            Assert.Equal(expectedPoints, capturedResult.Points);

            Assert.Equal(50 + expectedPoints, user.Points);
            this._usersMock.Verify(r => r.UpdateAsync(user, ct), Times.Once);
        }

        /// <summary>
        /// Тест: SubmitAsync коректно нараховує бонусні бали за 100% правильних відповідей.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task SubmitAsync_AllCorrectAnswers_GivesBonusPoints()
        {
            int testId = 11;
            int userId = 22;
            var ct = CancellationToken.None;

            var answers = new[]
            {
                (1, (string?)"answer1"),
                (2, (string?)"answer2"),
            };

            var testEntity = new Test { TestId = testId, CreatorId = 5 };
            this._testsMock
                .Setup(r => r.GetAsync(testId, ct))
                .ReturnsAsync(testEntity);

            var user = new User { UserId = userId, Points = 0 };
            this._usersMock
                .Setup(r => r.GetByIdAsync(userId, ct))
                .ReturnsAsync(user);

            var cards = new List<Flashcard>
            {
                new Flashcard { FlashcardId = 1, Answer = "answer1" },
                new Flashcard { FlashcardId = 2, Answer = "answer2" },
            };

            this._cardsMock
                .Setup(r => r.FindAsync(testEntity.CreatorId, null, ct))
                .ReturnsAsync(cards);

            TestResult? capturedResult = null;

            this._resultsMock
                .Setup(r => r.AddAsync(
                    It.IsAny<TestResult>(),
                    It.IsAny<IEnumerable<QuestionResult>>(),
                    It.IsAny<CancellationToken>()))
                .Callback<TestResult, IEnumerable<QuestionResult>, CancellationToken>((tr, qs, _) =>
                {
                    capturedResult = tr;
                })
                .ReturnsAsync((TestResult tr, IEnumerable<QuestionResult> qs, CancellationToken _) => tr);

            this._usersMock
                .Setup(r => r.UpdateAsync(It.IsAny<User>(), ct))
                .Returns(Task.CompletedTask);

            var dto = await this._service.SubmitAsync(testId, userId, answers, ct);

            int correct = answers.Length;
            double expectedPercentDouble = 100.0;
            decimal expectedPercent = (decimal)expectedPercentDouble;
            int expectedPoints = (correct * 10) + 20;

            Assert.NotNull(capturedResult);
            Assert.Equal(expectedPercent, capturedResult!.CorrectAnswersPercent);
            Assert.Equal(expectedPoints, capturedResult.Points);
            Assert.Equal(expectedPoints, user.Points);
        }

        /// <summary>
        /// Тест: SubmitAsync коректно створює QuestionResults та позначає відсутні картки як неправильні.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task SubmitAsync_BuildsQuestionResultsAndMarksMissingCardsAsIncorrect()
        {
            int testId = 33;
            int userId = 44;
            var ct = CancellationToken.None;

            var answers = new[]
            {
                (flashcardId: 1, userInput: "ans1"),
                (flashcardId: 2, userInput: (string?)null),
            };

            var testEntity = new Test { TestId = testId, CreatorId = 9 };
            this._testsMock
                .Setup(r => r.GetAsync(testId, ct))
                .ReturnsAsync(testEntity);

            var user = new User { UserId = userId, Points = 0 };
            this._usersMock
                .Setup(r => r.GetByIdAsync(userId, ct))
                .ReturnsAsync(user);

            var cards = new List<Flashcard>
            {
                new Flashcard { FlashcardId = 1, Answer = "ans1" },
            };

            this._cardsMock
                .Setup(r => r.FindAsync(testEntity.CreatorId, null, ct))
                .ReturnsAsync(cards);

            IEnumerable<QuestionResult>? capturedQuestions = null;

            this._resultsMock
                .Setup(r => r.AddAsync(
                    It.IsAny<TestResult>(),
                    It.IsAny<IEnumerable<QuestionResult>>(),
                    It.IsAny<CancellationToken>()))
                .Callback<TestResult, IEnumerable<QuestionResult>, CancellationToken>((tr, qs, _) =>
                {
                    capturedQuestions = qs;
                })
                .ReturnsAsync((TestResult tr, IEnumerable<QuestionResult> qs, CancellationToken _) => tr);

            this._usersMock
                .Setup(r => r.UpdateAsync(It.IsAny<User>(), ct))
                .Returns(Task.CompletedTask);

            var dto = await this._service.SubmitAsync(testId, userId, answers, ct);

            Assert.NotNull(capturedQuestions);
            var list = capturedQuestions!.ToList();
            Assert.Equal(2, list.Count);

            var q1 = list[0];
            Assert.Equal(1, q1.FlashcardId);
            Assert.Equal("ans1", q1.UserInput);
            Assert.True(q1.IsCorrect);

            var q2 = list[1];
            Assert.Equal(2, q2.FlashcardId);
            Assert.Equal(string.Empty, q2.UserInput);
            Assert.False(q2.IsCorrect);
        }

        /// <summary>
        /// Тест: SubmitAsync використовує CreatorId тесту (а не UserId) для завантаження карток.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task SubmitAsync_UsesTestCreatorIdWhenLoadingFlashcards()
        {
            int testId = 99;
            int userId = 1;
            var ct = CancellationToken.None;

            var answers = new[] { (1, (string?)"a") };

            var testCreatorId = 777;
            var testEntity = new Test { TestId = testId, CreatorId = testCreatorId };

            this._testsMock
                .Setup(r => r.GetAsync(testId, ct))
                .ReturnsAsync(testEntity);

            var user = new User { UserId = userId, Points = 0 };
            this._usersMock
                .Setup(r => r.GetByIdAsync(userId, ct))
                .ReturnsAsync(user);

            this._cardsMock
                .Setup(r => r.FindAsync(testCreatorId, null, ct))
                .ReturnsAsync(new List<Flashcard>
                {
                    new Flashcard { FlashcardId = 1, Answer = "a" },
                });

            this._resultsMock
                .Setup(r => r.AddAsync(
                    It.IsAny<TestResult>(),
                    It.IsAny<IEnumerable<QuestionResult>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((TestResult tr, IEnumerable<QuestionResult> qs, CancellationToken _) => tr);

            this._usersMock
                .Setup(r => r.UpdateAsync(It.IsAny<User>(), ct))
                .Returns(Task.CompletedTask);

            var dto = await this._service.SubmitAsync(testId, userId, answers, ct);

            this._cardsMock.Verify(r => r.FindAsync(testCreatorId, null, ct), Times.Once);
        }
    }
}