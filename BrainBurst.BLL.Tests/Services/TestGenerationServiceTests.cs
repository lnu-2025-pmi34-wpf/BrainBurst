namespace BrainBurst.BLL.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BrainBurst.BLL.Interfaces.Abstractions;
    using BrainBurst.BLL.Services;
    using BrainBurst.DAL.Abstractions;
    using BrainBurst.DAL.Entities;
    using Moq;
    using Xunit;

    /// <summary>
    /// Містить юніт-тести для <see cref="TestGenerationService"/>.
    /// </summary>
    public class TestGenerationServiceTests
    {
        private readonly Mock<IQuizGenerator> _aiMock;
        private readonly Mock<IFlashcardRepository> _cardsMock;
        private readonly TestGenerationService _service;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestGenerationServiceTests"/> class.
        /// налаштовуючи "моки" (заглушки) для <see cref="IQuizGenerator"/> та <see cref="IFlashcardRepository"/>.
        /// </summary>
        public TestGenerationServiceTests()
        {
            this._aiMock = new Mock<IQuizGenerator>(MockBehavior.Strict);
            this._cardsMock = new Mock<IFlashcardRepository>(MockBehavior.Strict);

            this._service = new TestGenerationService(this._aiMock.Object, this._cardsMock.Object);
        }

        /// <summary>
        /// Тест: CreateFlashcardsFromTextAsync викликає IQuizGenerator з тим самим текстом.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateFlashcardsFromTextAsync_CallsQuizGeneratorWithSameText()
        {
            var creatorId = 42;
            var text = "some input text";
            var ct = new CancellationTokenSource().Token;

            var aiResult = Array.Empty<(string Question, string Answer, IReadOnlyList<string> Tags)>();

            this._aiMock
                .Setup(ai => ai.GenerateFromTextAsync(text, ct))
                .ReturnsAsync(aiResult);

            this._cardsMock
                .Setup(r => r.AddAsync(
                    It.IsAny<Flashcard>(),
                    It.IsAny<IReadOnlyList<string>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Flashcard f, IReadOnlyList<string> tags, CancellationToken _) => f);

            var result = await this._service.CreateFlashcardsFromTextAsync(creatorId, text, ct);

            this._aiMock.Verify(ai => ai.GenerateFromTextAsync(text, ct), Times.Once);
        }

        /// <summary>
        /// Тест: Якщо AI повертає порожній список, репозиторій не викликається, і сервіс повертає порожній список.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateFlashcardsFromTextAsync_EmptyAiResult_ReturnsEmptyAndDoesNotCallRepository()
        {
            var creatorId = 1;
            var text = "nothing useful";
            var ct = CancellationToken.None;

            this._aiMock
                .Setup(ai => ai.GenerateFromTextAsync(text, ct))
                .ReturnsAsync(Array.Empty<(string Question, string Answer, IReadOnlyList<string> Tags)>());

            var result = await this._service.CreateFlashcardsFromTextAsync(creatorId, text, ct);

            Assert.NotNull(result);
            Assert.Empty(result);
            this._cardsMock.Verify(
                r => r.AddAsync(
                    It.IsAny<Flashcard>(),
                    It.IsAny<IReadOnlyList<string>>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        /// <summary>
        /// Тест: CreateFlashcardsFromTextAsync викликає AddAsync для кожного елемента, повернутого AI.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateFlashcardsFromTextAsync_MultipleItems_CallsAddAsyncForEach()
        {
            var creatorId = 10;
            var text = "brain burst content";
            var ct = CancellationToken.None;

            var aiItems = new List<(string Question, string Answer, IReadOnlyList<string> Tags)>
            {
                ("Q1", "A1", new[] { "tag1", "tag2" }),
                ("Q2", "A2", new[] { "x" }),
                ("Q3", "A3", Array.Empty<string>()),
            };

            this._aiMock
                .Setup(ai => ai.GenerateFromTextAsync(text, ct))
                .ReturnsAsync(aiItems);

            this._cardsMock
                .Setup(r => r.AddAsync(
                    It.IsAny<Flashcard>(),
                    It.IsAny<IReadOnlyList<string>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Flashcard f, IReadOnlyList<string> tags, CancellationToken _) => f);

            var result = await this._service.CreateFlashcardsFromTextAsync(creatorId, text, ct);

            this._cardsMock.Verify(
                r => r.AddAsync(
                    It.IsAny<Flashcard>(),
                    It.IsAny<IReadOnlyList<string>>(),
                    It.IsAny<CancellationToken>()),
                Times.Exactly(aiItems.Count));

            Assert.Equal(aiItems.Count, result.Count);
        }

        /// <summary>
        /// Тест: CreateFlashcardsFromTextAsync передає коректні дані (Q, A, CreatorId) в репозиторій.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateFlashcardsFromTextAsync_PassesCorrectFlashcardDataToRepository()
        {
            var creatorId = 777;
            var text = "unit test text";
            var ct = CancellationToken.None;

            var tags = new[] { "tagA", "tagB" };
            var aiItems = new[]
            {
                (Question: "What is BrainBurst?", Answer: "Project description", Tags: (IReadOnlyList<string>)tags),
            };

            this._aiMock
                .Setup(ai => ai.GenerateFromTextAsync(text, ct))
                .ReturnsAsync(aiItems);

            Flashcard? capturedFlashcard = null;
            IEnumerable<string>? capturedTags = null;

            this._cardsMock
                .Setup(r => r.AddAsync(
                            It.IsAny<Flashcard>(),
                            It.IsAny<IEnumerable<string>>(),
                            It.IsAny<CancellationToken>()))
                .Callback<Flashcard, IEnumerable<string>, CancellationToken>((f, t, _) =>
                {
                    capturedFlashcard = f;
                    capturedTags = t;
                })
                .ReturnsAsync((Flashcard f, IEnumerable<string> t, CancellationToken _) => f);

            var result = await this._service.CreateFlashcardsFromTextAsync(creatorId, text, ct);

            Assert.NotNull(capturedFlashcard);
            Assert.Equal(aiItems[0].Question, capturedFlashcard!.Question);
            Assert.Equal(aiItems[0].Answer, capturedFlashcard.Answer);
            Assert.Equal(creatorId, capturedFlashcard.CreatorId);

            Assert.NotNull(capturedTags);
            Assert.Equal(tags, capturedTags!.ToArray());
        }

        /// <summary>
        /// Тест: CreateFlashcardsFromTextAsync встановлює CreatedAt на поточний час (UtcNow).
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateFlashcardsFromTextAsync_SetsCreatedAtToUtcNow()
        {
            var creatorId = 5;
            var text = "time test";
            var ct = CancellationToken.None;

            var aiItems = new[]
            {
                ("Q time", "A time", (IReadOnlyList<string>)new[] { "time" }),
            };

            this._aiMock
                .Setup(ai => ai.GenerateFromTextAsync(text, ct))
                .ReturnsAsync(aiItems);

            DateTime utcBefore = DateTime.UtcNow;

            Flashcard? captured = null;

            this._cardsMock
                .Setup(r => r.AddAsync(
                            It.IsAny<Flashcard>(),
                            It.IsAny<IEnumerable<string>>(),
                            It.IsAny<CancellationToken>()))
                .Callback<Flashcard, IEnumerable<string>, CancellationToken>((f, tags, _) =>
                {
                    captured = f;
                })
                .ReturnsAsync((Flashcard f, IEnumerable<string> t, CancellationToken _) => f);

            var result = await this._service.CreateFlashcardsFromTextAsync(creatorId, text, ct);

            DateTime utcAfter = DateTime.UtcNow;

            Assert.NotNull(captured);
            Assert.True(
                captured!.CreatedAt >= utcBefore && captured.CreatedAt <= utcAfter,
                $"CreatedAt {captured.CreatedAt:o} має бути між {utcBefore:o} та {utcAfter:o}");
        }

        /// <summary>
        /// Тест: CreateFlashcardsFromTextAsync передає CancellationToken в репозиторій.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateFlashcardsFromTextAsync_PropagatesCancellationTokenToRepository()
        {
            var creatorId = 9;
            var text = "cancellation";
            using var cts = new CancellationTokenSource();

            var ct = cts.Token;

            var aiItems = new[]
            {
                ("Q", "A", (IReadOnlyList<string>)Array.Empty<string>()),
            };

            this._aiMock
                .Setup(ai => ai.GenerateFromTextAsync(text, ct))
                .ReturnsAsync(aiItems);

            this._cardsMock
                .Setup(r => r.AddAsync(
                            It.IsAny<Flashcard>(),
                            It.IsAny<IReadOnlyList<string>>(),
                            It.Is<CancellationToken>(token => token == ct)))
                .ReturnsAsync((Flashcard f, IReadOnlyList<string> t, CancellationToken _) => f);

            var result = await this._service.CreateFlashcardsFromTextAsync(creatorId, text, ct);

            this._cardsMock.Verify(
                r => r.AddAsync(
                    It.IsAny<Flashcard>(),
                    It.IsAny<IReadOnlyList<string>>(),
                    It.Is<CancellationToken>(token => token == ct)),
                Times.Once);
        }

        /// <summary>
        /// Тест: CreateFlashcardsFromTextAsync повертає ту ж кількість DTO, що й AI, і в тому ж порядку.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateFlashcardsFromTextAsync_ReturnsSameNumberOfDtosInSameOrder()
        {
            // Arrange
            var creatorId = 123;
            var text = "order test";
            var ct = CancellationToken.None;

            var aiItems = new List<(string Question, string Answer, IReadOnlyList<string> Tags)>
            {
                ("First Q", "First A", new[] { "t1" }),
                ("Second Q", "Second A", new[] { "t2" }),
            };

            this._aiMock
                .Setup(ai => ai.GenerateFromTextAsync(text, ct))
                .ReturnsAsync(aiItems);

            this._cardsMock
                .Setup(r => r.AddAsync(
                    It.IsAny<Flashcard>(),
                    It.IsAny<IReadOnlyList<string>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Flashcard f, IReadOnlyList<string> t, CancellationToken _) => f);

            var result = await this._service.CreateFlashcardsFromTextAsync(creatorId, text, ct);

            Assert.Equal(aiItems.Count, result.Count);
            Assert.Equal(aiItems[0].Question, result[0].Question);
            Assert.Equal(aiItems[0].Answer, result[0].Answer);
            Assert.Equal(aiItems[1].Question, result[1].Question);
            Assert.Equal(aiItems[1].Answer, result[1].Answer);
        }
    }
}
