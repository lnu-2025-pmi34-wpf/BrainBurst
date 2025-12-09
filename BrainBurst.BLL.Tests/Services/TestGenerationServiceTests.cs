namespace BrainBurst.BLL.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BrainBurst.BLL.DTO;
    using BrainBurst.BLL.Interfaces.Abstractions;
    using BrainBurst.BLL.Services;
    using BrainBurst.DAL.Abstractions;
    using BrainBurst.DAL.Entities;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// Містить юніт-тести для класу <see cref="TestGenerationService"/>.
    /// </summary>
    public class TestGenerationServiceTests
    {
        private readonly Mock<IQuizGenerator> _aiMock;
        private readonly Mock<IFlashcardRepository> _cardsMock;
        private readonly Mock<ILogger<TestGenerationService>> _loggerMock;
        private readonly TestGenerationService _service;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestGenerationServiceTests"/> class.
        /// </summary>
        public TestGenerationServiceTests()
        {
            this._aiMock = new Mock<IQuizGenerator>(MockBehavior.Strict);
            this._cardsMock = new Mock<IFlashcardRepository>(MockBehavior.Strict);
            this._loggerMock = new Mock<ILogger<TestGenerationService>>(MockBehavior.Loose);

            this._service = new TestGenerationService(
                this._aiMock.Object,
                this._cardsMock.Object,
                this._loggerMock.Object);
        }

        /// <summary>
        /// Тест: Перевіряє, що метод викликає генератор AI з тими самими вхідними параметрами.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Fact]
        public async Task CreateFlashcardsFromTextAsync_CallsAiWithSameParameters()
        {
            var text = "hello world";
            var creatorId = 1;
            var tags = new[] { "tag1", "tag2" };
            var ct = CancellationToken.None;

            this._aiMock
                .Setup(ai => ai.GenerateFromTextAsync(text, ct))
                .ReturnsAsync(new List<(string Question, string Answer)>());

            var result = await this._service.CreateFlashcardsFromTextAsync(creatorId, text, tags, ct);

            this._aiMock.Verify(ai => ai.GenerateFromTextAsync(text, ct), Times.Once);
        }

        /// <summary>
        /// Тест: Перевіряє, що якщо AI не повертає карток, метод повертає порожній список
        /// і не викликає репозиторій для збереження.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Fact]
        public async Task CreateFlashcardsFromTextAsync_ZeroItemsFromAi_ReturnsEmptyList()
        {
            var text = "empty";
            var creatorId = 123;
            var tags = new[] { "t" };
            var ct = CancellationToken.None;

            this._aiMock
                .Setup(ai => ai.GenerateFromTextAsync(text, ct))
                .ReturnsAsync(Array.Empty<(string Question, string Answer)>());

            var result = await this._service.CreateFlashcardsFromTextAsync(creatorId, text, tags, ct);

            Assert.Empty(result);

            this._cardsMock.Verify(
                r => r.AddAsync(
                    It.IsAny<Flashcard>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        /// <summary>
        /// Тест: Перевіряє, що метод зберігає кожну згенеровану картку,
        /// передаючи коректні дані та теги до репозиторію.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Fact]
        public async Task CreateFlashcardsFromTextAsync_SavesEachFlashcard_WithCorrectData()
        {
            var creatorId = 10;
            var text = "some text";
            var tags = new[] { "A", "B" };
            var ct = CancellationToken.None;

            var aiItems = new List<(string Question, string Answer)>
            {
                ("Q1", "A1"),
                ("Q2", "A2"),
            };

            this._aiMock
                .Setup(ai => ai.GenerateFromTextAsync(text, ct))
                .ReturnsAsync(aiItems);

            this._cardsMock
                .Setup(r => r.AddAsync(
                    It.IsAny<Flashcard>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Flashcard f, IEnumerable<string> tg, CancellationToken _) => f);

            var result = await this._service.CreateFlashcardsFromTextAsync(creatorId, text, tags, ct);

            this._cardsMock.Verify(
                r => r.AddAsync(
                    It.IsAny<Flashcard>(),
                    tags,
                    ct),
                Times.Exactly(aiItems.Count));

            Assert.Equal(2, result.Count);
            Assert.Equal("Q1", result[0].Question);
            Assert.Equal("A1", result[0].Answer);
        }

        /// <summary>
        /// Тест: Перевіряє, що поле CreatedAt встановлюється близьким до поточного часу UTC.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Fact]
        public async Task CreateFlashcardsFromTextAsync_SetsCreatedAt_ToUtcNow()
        {
            var creatorId = 5;
            var text = "time-check";
            var tags = Array.Empty<string>();
            var ct = CancellationToken.None;

            var aiItems = new[]
            {
                ("What?", "Answer!"),
            };

            this._aiMock
                .Setup(ai => ai.GenerateFromTextAsync(text, ct))
                .ReturnsAsync(aiItems);

            DateTime before = DateTime.UtcNow;

            Flashcard? savedFlashcard = null;

            this._cardsMock
                .Setup(r => r.AddAsync(
                    It.IsAny<Flashcard>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
                .Callback<Flashcard, IEnumerable<string>, CancellationToken>((f, _, _) =>
                {
                    savedFlashcard = f;
                })
                .ReturnsAsync((Flashcard f, IEnumerable<string> tg, CancellationToken _) => f);

            var result = await this._service.CreateFlashcardsFromTextAsync(creatorId, text, tags, ct);

            DateTime after = DateTime.UtcNow;

            Assert.NotNull(savedFlashcard);
            Assert.True(savedFlashcard!.CreatedAt >= before && savedFlashcard!.CreatedAt <= after);
        }

        /// <summary>
        /// Тест: Перевіряє, що токен скасування коректно передається до методу AddAsync репозиторію.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Fact]
        public async Task CreateFlashcardsFromTextAsync_PassesCancellationTokenToRepository()
        {
            var creatorId = 9;
            var text = "cancel test";
            var tags = new[] { "t" };
            using var cts = new CancellationTokenSource();
            var ct = cts.Token;

            var aiItems = new[]
            {
                ("Q", "A"),
            };

            this._aiMock
                .Setup(ai => ai.GenerateFromTextAsync(text, ct))
                .ReturnsAsync(aiItems);

            this._cardsMock
                .Setup(r => r.AddAsync(
                    It.IsAny<Flashcard>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.Is<CancellationToken>(token => token == ct)))
                .ReturnsAsync((Flashcard f, IEnumerable<string> tg, CancellationToken _) => f);

            var result = await this._service.CreateFlashcardsFromTextAsync(creatorId, text, tags, ct);

            this._cardsMock.Verify(
                r => r.AddAsync(
                    It.IsAny<Flashcard>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.Is<CancellationToken>(token => token == ct)),
                Times.Once);
        }

        /// <summary>
        /// Якщо AI-генератор кидає виняток, сервіс має залогувати помилку і пробросити виняток далі.
        /// Покриваємо гілку catch (Exception) у CreateFlashcardsFromTextAsync.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateFlashcardsFromTextAsync_AiThrows_LogsErrorAndRethrows()
        {
            var creatorId = 77;
            var text = "boom";
            var tags = new[] { "x" };
            var ct = CancellationToken.None;

            this._aiMock
                .Setup(ai => ai.GenerateFromTextAsync(text, ct))
                .ThrowsAsync(new InvalidOperationException("AI failure"));

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                this._service.CreateFlashcardsFromTextAsync(creatorId, text, tags, ct));

            this._aiMock.Verify(ai => ai.GenerateFromTextAsync(text, ct), Times.Once);
            this._cardsMock.Verify(
                r => r.AddAsync(
                    It.IsAny<Flashcard>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
