using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BrainBurst.BLL.Services;
using BrainBurst.DAL.Abstractions;
using BrainBurst.DAL.Entities;
using BrainBurst.BLL.Interfaces;
using Moq;
using Xunit;

namespace BrainBurst.BLL.Tests.Services
{
    public class TestServiceTests
    {
        private readonly Mock<ITestRepository> _testsMock;
        private readonly Mock<ITestResultRepository> _resultsMock;
        private readonly Mock<IUserRepository> _usersMock;
        private readonly Mock<IFlashcardRepository> _cardsMock;
        private readonly Mock<IRatingService> _ratingMock;

        private readonly TestService _service;

        public TestServiceTests()
        {
            _testsMock   = new Mock<ITestRepository>(MockBehavior.Strict);
            _resultsMock = new Mock<ITestResultRepository>(MockBehavior.Strict);
            _usersMock   = new Mock<IUserRepository>(MockBehavior.Strict);
            _cardsMock   = new Mock<IFlashcardRepository>(MockBehavior.Strict);
            _ratingMock  = new Mock<IRatingService>(MockBehavior.Strict);

            _service = new TestService(
                _testsMock.Object,
                _resultsMock.Object,
                _usersMock.Object,
                _cardsMock.Object,
                _ratingMock.Object
            );
        }

        // GenerateFromFlashcardsAsync

        [Fact]
        public async Task GenerateFromFlashcardsAsync_NullFlashcardIds_ThrowsArgumentException()
        {
            int creatorId = 1;
            IEnumerable<int>? ids = null;
            var ct = CancellationToken.None;

            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.GenerateFromFlashcardsAsync(creatorId, ids!, ct));

            Assert.Contains("Потрібен хоча б один flashcardId", ex.Message);
        }

        [Fact]
        public async Task GenerateFromFlashcardsAsync_EmptyFlashcardIds_ThrowsArgumentException()
        {
            int creatorId = 1;
            IEnumerable<int> ids = Array.Empty<int>();
            var ct = CancellationToken.None;

            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.GenerateFromFlashcardsAsync(creatorId, ids, ct));

            Assert.Contains("Потрібен хоча б один flashcardId", ex.Message);
        }

        [Fact]
        public async Task GenerateFromFlashcardsAsync_CallsRepositoriesWithCorrectParameters()
        {
            int creatorId = 7;
            var flashcardIds = new[] { 10, 20 };
            var ct = CancellationToken.None;

            var testEntity = new Test
            {
                TestId = 123,
                CreatorId = creatorId
            };

            _testsMock
                .Setup(r => r.CreateFromFlashcardsAsync(
                    creatorId,
                    It.Is<IEnumerable<int>>(ids => ids.SequenceEqual(flashcardIds)),
                    ct))
                .ReturnsAsync(testEntity);

            var allCards = new List<Flashcard>
            {
                new Flashcard { FlashcardId = 10, Question = "Q1", Answer = "A1", CreatorId = creatorId },
                new Flashcard { FlashcardId = 20, Question = "Q2", Answer = "A2", CreatorId = creatorId },
                new Flashcard { FlashcardId = 30, Question = "Q3", Answer = "A3", CreatorId = creatorId }
            };

            _cardsMock
                .Setup(r => r.FindAsync(creatorId, null, ct))
                .ReturnsAsync(allCards);

            var dto = await _service.GenerateFromFlashcardsAsync(creatorId, flashcardIds, ct);

            _testsMock.Verify(r => r.CreateFromFlashcardsAsync(
                creatorId,
                It.Is<IEnumerable<int>>(ids => ids.SequenceEqual(flashcardIds)),
                ct), Times.Once);

            _cardsMock.Verify(r => r.FindAsync(creatorId, null, ct), Times.Once);

            Assert.NotNull(dto);
            Assert.Equal(testEntity.TestId, dto.Id);
            Assert.Equal(creatorId, dto.CreatorId);
            Assert.Equal(2, dto.Questions.Count);
            Assert.All(dto.Questions, q => Assert.Contains(q.Id, flashcardIds));
        }

        // GetAsync

        [Fact]
        public async Task GetAsync_NotFound_ReturnsNull()
        {
            int testId = 42;
            var ct = CancellationToken.None;

            _testsMock
                .Setup(r => r.GetAsync(testId, ct))
                .ReturnsAsync((Test?)null);

            var dto = await _service.GetAsync(testId, ct);

            Assert.Null(dto);
            _testsMock.Verify(r => r.GetAsync(testId, ct), Times.Once);
        }

        [Fact]
        public async Task GetAsync_Found_ReturnsDtoWithEmptyQuestions()
        {
            int testId = 100;
            int creatorId = 5;
            var ct = CancellationToken.None;

            var entity = new Test
            {
                TestId = testId,
                CreatorId = creatorId
            };

            _testsMock
                .Setup(r => r.GetAsync(testId, ct))
                .ReturnsAsync(entity);

            var dto = await _service.GetAsync(testId, ct);

            Assert.NotNull(dto);
            Assert.Equal(testId, dto!.Id);
            Assert.Equal(creatorId, dto.CreatorId);
            Assert.NotNull(dto.Questions);
            Assert.Empty(dto.Questions);
        }

        // SubmitAsync: валідація 

        [Fact]
        public async Task SubmitAsync_EmptyAnswers_ThrowsArgumentException()
        {
            int testId = 1;
            int userId = 2;
            var answers = Array.Empty<(int flashcardId, string userInput)>();
            var ct = CancellationToken.None;

            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.SubmitAsync(testId, userId, answers, ct));

            Assert.Contains("Відповіді відсутні", ex.Message);
        }

        [Fact]
        public async Task SubmitAsync_TestNotFound_ThrowsInvalidOperationException()
        {
            int testId = 1;
            int userId = 2;
            var answers = new[] { (1, "a") };
            var ct = CancellationToken.None;

            _testsMock
                .Setup(r => r.GetAsync(testId, ct))
                .ReturnsAsync((Test?)null);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.SubmitAsync(testId, userId, answers, ct));

            Assert.Contains("Тест не знайдено", ex.Message);
        }

        // SubmitAsync: логіка правильності / балів

        [Fact]
        public async Task SubmitAsync_CalculatesPercentAndPoints_ForPartiallyCorrectAnswers()
        {
            int testId = 10;
            int userId = 20;
            var ct = CancellationToken.None;

            var answers = new[]
            {
                (flashcardId: 1, userInput: "  aNsWeR1 "), // правильна (trim + ignore case)
                (flashcardId: 2, userInput: "wrong"),      // неправильна
                (flashcardId: 3, userInput: "Answer3")     // правильна
            };

            var testEntity = new Test { TestId = testId, CreatorId = 99 };
            _testsMock
                .Setup(r => r.GetAsync(testId, ct))
                .ReturnsAsync(testEntity);

            var user = new User { UserId = userId, Points = 50 };
            _usersMock
                .Setup(r => r.GetByIdAsync(userId, ct))
                .ReturnsAsync(user);

            var cards = new List<Flashcard>
            {
                new Flashcard { FlashcardId = 1, Answer = "answer1" },
                new Flashcard { FlashcardId = 2, Answer = "correct2" },
                new Flashcard { FlashcardId = 3, Answer = " answer3 " }
            };

            _cardsMock
                .Setup(r => r.FindAsync(testEntity.CreatorId, null, ct))
                .ReturnsAsync(cards);

            TestResult? capturedResult = null;
            IEnumerable<QuestionResult>? capturedQuestions = null;

            _resultsMock
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

            _usersMock
                .Setup(r => r.UpdateAsync(It.IsAny<User>(), ct))
                .Returns(Task.CompletedTask);

            var dto = await _service.SubmitAsync(testId, userId, answers, ct);

            Assert.NotNull(capturedResult);
            Assert.NotNull(capturedQuestions);

            int correct = 2;
            int total   = answers.Length;
            double expectedPercentDouble = 100.0 * correct / total;
            decimal expectedPercent = (decimal)expectedPercentDouble;
            int expectedPoints = correct * 10; // без бонусу, бо не всі правильні

            Assert.Equal(testId, capturedResult!.TestId);
            Assert.Equal(userId, capturedResult.UserId);
            Assert.Equal(expectedPercent, capturedResult.CorrectAnswersPercent);
            Assert.Equal(expectedPoints, capturedResult.Points);

            // user points оновлені й UpdateAsync викликаний
            Assert.Equal(50 + expectedPoints, user.Points);
            _usersMock.Verify(r => r.UpdateAsync(user, ct), Times.Once);
        }

        [Fact]
        public async Task SubmitAsync_AllCorrectAnswers_GivesBonusPoints()
        {
            int testId = 11;
            int userId = 22;
            var ct = CancellationToken.None;

            var answers = new[]
            {
                (1, "answer1"),
                (2, "answer2")
            };

            var testEntity = new Test { TestId = testId, CreatorId = 5 };
            _testsMock
                .Setup(r => r.GetAsync(testId, ct))
                .ReturnsAsync(testEntity);

            var user = new User { UserId = userId, Points = 0 };
            _usersMock
                .Setup(r => r.GetByIdAsync(userId, ct))
                .ReturnsAsync(user);

            var cards = new List<Flashcard>
            {
                new Flashcard { FlashcardId = 1, Answer = "answer1" },
                new Flashcard { FlashcardId = 2, Answer = "answer2" }
            };

            _cardsMock
                .Setup(r => r.FindAsync(testEntity.CreatorId, null, ct))
                .ReturnsAsync(cards);

            TestResult? capturedResult = null;

            _resultsMock
                .Setup(r => r.AddAsync(
                    It.IsAny<TestResult>(),
                    It.IsAny<IEnumerable<QuestionResult>>(),
                    It.IsAny<CancellationToken>()))
                .Callback<TestResult, IEnumerable<QuestionResult>, CancellationToken>((tr, qs, _) =>
                {
                    capturedResult = tr;
                })
                .ReturnsAsync((TestResult tr, IEnumerable<QuestionResult> qs, CancellationToken _) => tr);

            _usersMock
                .Setup(r => r.UpdateAsync(It.IsAny<User>(), ct))
                .Returns(Task.CompletedTask);

            var dto = await _service.SubmitAsync(testId, userId, answers, ct);

            int correct = answers.Length;
            double expectedPercentDouble = 100.0;
            decimal expectedPercent = (decimal)expectedPercentDouble;
            int expectedPoints = correct * 10 + 20; // бонус за 100%

            Assert.NotNull(capturedResult);
            Assert.Equal(expectedPercent, capturedResult!.CorrectAnswersPercent);
            Assert.Equal(expectedPoints, capturedResult.Points);
            Assert.Equal(expectedPoints, user.Points);
        }

        // SubmitAsync: QuestionResult та поведінка при відсутніх картках 

        [Fact]
        public async Task SubmitAsync_BuildsQuestionResultsAndMarksMissingCardsAsIncorrect()
        {
            int testId = 33;
            int userId = 44;
            var ct = CancellationToken.None;

            var answers = new[]
            {
                (flashcardId: 1, userInput: "ans1"),  // картка є
                (flashcardId: 2, userInput: (string?)null), // картка немає
            };

            var testEntity = new Test { TestId = testId, CreatorId = 9 };
            _testsMock
                .Setup(r => r.GetAsync(testId, ct))
                .ReturnsAsync(testEntity);

            var user = new User { UserId = userId, Points = 0 };
            _usersMock
                .Setup(r => r.GetByIdAsync(userId, ct))
                .ReturnsAsync(user);

            var cards = new List<Flashcard>
            {
                new Flashcard { FlashcardId = 1, Answer = "ans1" }
                // FlashcardId 2 відсутня
            };

            _cardsMock
                .Setup(r => r.FindAsync(testEntity.CreatorId, null, ct))
                .ReturnsAsync(cards);

            IEnumerable<QuestionResult>? capturedQuestions = null;

            _resultsMock
                .Setup(r => r.AddAsync(
                    It.IsAny<TestResult>(),
                    It.IsAny<IEnumerable<QuestionResult>>(),
                    It.IsAny<CancellationToken>()))
                .Callback<TestResult, IEnumerable<QuestionResult>, CancellationToken>((tr, qs, _) =>
                {
                    capturedQuestions = qs;
                })
                .ReturnsAsync((TestResult tr, IEnumerable<QuestionResult> qs, CancellationToken _) => tr);

            _usersMock
                .Setup(r => r.UpdateAsync(It.IsAny<User>(), ct))
                .Returns(Task.CompletedTask);

            var dto = await _service.SubmitAsync(testId, userId, answers, ct);

            Assert.NotNull(capturedQuestions);
            var list = capturedQuestions!.ToList();
            Assert.Equal(2, list.Count);

            var q1 = list[0];
            Assert.Equal(1, q1.FlashcardId);
            Assert.Equal("ans1", q1.UserInput);
            Assert.True(q1.IsCorrect);

            var q2 = list[1];
            Assert.Equal(2, q2.FlashcardId);
            Assert.Equal(string.Empty, q2.UserInput); // null -> ""
            Assert.False(q2.IsCorrect);
        }

        // SubmitAsync: перевіряємо, що використовується CreatorId тесту, а не userId

        [Fact]
        public async Task SubmitAsync_UsesTestCreatorIdWhenLoadingFlashcards()
        {
            int testId = 99;
            int userId = 1;
            var ct = CancellationToken.None;

            var answers = new[] { (1, "a") };

            var testCreatorId = 777; // відрізняється від userId
            var testEntity = new Test { TestId = testId, CreatorId = testCreatorId };

            _testsMock
                .Setup(r => r.GetAsync(testId, ct))
                .ReturnsAsync(testEntity);

            var user = new User { UserId = userId, Points = 0 };
            _usersMock
                .Setup(r => r.GetByIdAsync(userId, ct))
                .ReturnsAsync(user);

            _cardsMock
                .Setup(r => r.FindAsync(testCreatorId, null, ct))
                .ReturnsAsync(new List<Flashcard>
                {
                    new Flashcard { FlashcardId = 1, Answer = "a" }
                });

            _resultsMock
                .Setup(r => r.AddAsync(
                    It.IsAny<TestResult>(),
                    It.IsAny<IEnumerable<QuestionResult>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((TestResult tr, IEnumerable<QuestionResult> qs, CancellationToken _) => tr);

            _usersMock
                .Setup(r => r.UpdateAsync(It.IsAny<User>(), ct))
                .Returns(Task.CompletedTask);

            var dto = await _service.SubmitAsync(testId, userId, answers, ct);

            _cardsMock.Verify(r => r.FindAsync(testCreatorId, null, ct), Times.Once);
        }
    }
}