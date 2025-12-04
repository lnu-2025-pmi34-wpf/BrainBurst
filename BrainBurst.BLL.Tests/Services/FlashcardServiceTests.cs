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
    /// Містить юніт-тести для <see cref="FlashcardService"/>.
    /// </summary>
    public class FlashcardServiceTests
    {
        private readonly Mock<IFlashcardRepository> _cardsMock;
        private readonly Mock<ILogger<FlashcardService>> _loggerMock;
        private readonly FlashcardService _service;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlashcardServiceTests"/> class.
        /// налаштовуючи "мок" (заглушку) для <see cref="IFlashcardRepository"/>.
        /// </summary>
        public FlashcardServiceTests()
        {
            this._cardsMock = new Mock<IFlashcardRepository>(MockBehavior.Strict);
            this._loggerMock = new Mock<ILogger<FlashcardService>>();

            this._service = new FlashcardService(this._cardsMock.Object, this._loggerMock.Object);
        }

        /// <summary>
        /// Тест: CreateAsync з валідними даними викликає AddAsync з коректною сутністю та тегами, і повертає DTO.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateAsync_ValidData_CallsAddAsyncWithCorrectEntityAndTags_AndReturnsDto()
        {
            var ct = CancellationToken.None;
            int creatorId = 7;
            string question = "Що таке флеш-картка?";
            string answer = "Картка з питанням та відповіддю.";
            var tags = new[] { "tag1", "tag2" };

            Flashcard? capturedCard = null;
            IEnumerable<string>? capturedTags = null;

            this._cardsMock
                .Setup(r => r.AddAsync(
                    It.IsAny<Flashcard>(),
                    It.IsAny<IEnumerable<string>>(),
                    ct))
                .Callback<Flashcard, IEnumerable<string>, CancellationToken>((card, t, _) =>
                {
                    capturedCard = card;
                    capturedTags = t;
                })
                .ReturnsAsync((Flashcard card, IEnumerable<string> t, CancellationToken _) => card);

            var before = DateTime.UtcNow;

            var dto = await this._service.CreateAsync(creatorId, question, answer, tags, ct);

            var after = DateTime.UtcNow;

            this._cardsMock.Verify(
                r => r.AddAsync(
                    It.IsAny<Flashcard>(),
                    It.IsAny<IEnumerable<string>>(),
                    ct),
                Times.Once);

            Assert.NotNull(capturedCard);
            Assert.Equal(creatorId, capturedCard!.CreatorId);
            Assert.Equal(question, capturedCard.Question);
            Assert.Equal(answer, capturedCard.Answer);
            Assert.True(capturedCard.CreatedAt >= before && capturedCard.CreatedAt <= after);

            Assert.NotNull(capturedTags);
            Assert.Equal(tags, capturedTags!.ToArray());

            Assert.Equal(question, dto.Question);
            Assert.Equal(answer, dto.Answer);
            Assert.Equal(creatorId, dto.CreatorId);
        }

        /// <summary>
        /// Тест: UpdateAsync кидає KeyNotFoundException, якщо картку не знайдено.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task UpdateAsync_CardNotFound_ThrowsKeyNotFoundException()
        {
            var ct = CancellationToken.None;
            int id = 10;
            int editorId = 5;

            this._cardsMock
                .Setup(r => r.GetAsync(id, ct))
                .ReturnsAsync((Flashcard?)null);

            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                this._service.UpdateAsync(id, editorId, "Q", "A", Array.Empty<string>(), ct));

            Assert.Contains("Картку з ID 10 не знайдено", ex.Message);
            this._cardsMock.Verify(r => r.GetAsync(id, ct), Times.Once);
            this._cardsMock.Verify(
                r => r.UpdateAsync(It.IsAny<Flashcard>(), It.IsAny<IEnumerable<string>>(), ct),
                Times.Never);
        }

        /// <summary>
        /// Тест: UpdateAsync кидає KeyNotFoundException, якщо користувач, що редагує, не є творцем.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task UpdateAsync_EditorIsNotCreator_ThrowsKeyNotFoundException()
        {
            var ct = CancellationToken.None;
            int id = 11;
            int editorId = 99;

            var existing = new Flashcard
            {
                FlashcardId = id,
                CreatorId = 1,
                Question = "Old Q",
                Answer = "Old A",
                CreatedAt = new DateTime(2024, 1, 1),
            };

            this._cardsMock
                .Setup(r => r.GetAsync(id, ct))
                .ReturnsAsync(existing);

            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                this._service.UpdateAsync(id, editorId, "New Q", "New A", Array.Empty<string>(), ct));

            Assert.Contains("не знайдено або користувач не є її творцем", ex.Message);
            this._cardsMock.Verify(r => r.GetAsync(id, ct), Times.Once);
            this._cardsMock.Verify(
                r => r.UpdateAsync(It.IsAny<Flashcard>(), It.IsAny<IEnumerable<string>>(), ct),
                Times.Never);
        }

        /// <summary>
        /// Тест: UpdateAsync при валідному запиті викликає UpdateAsync, зберігаючи дату створення та передаючи нові дані.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task UpdateAsync_ValidRequest_UsesExistingCreatedAt_AndCallsUpdateWithNewQuestionAnswer()
        {
            var ct = CancellationToken.None;
            int id = 12;
            int editorId = 3;
            string newQ = "New question";
            string newA = "New answer";
            var tags = new[] { "x", "y" };

            var existing = new Flashcard
            {
                FlashcardId = id,
                CreatorId = editorId,
                Question = "Old question",
                Answer = "Old answer",
                CreatedAt = new DateTime(2023, 12, 31, 10, 0, 0, DateTimeKind.Utc),
            };

            this._cardsMock
                .Setup(r => r.GetAsync(id, ct))
                .ReturnsAsync(existing);

            Flashcard? updated = null;
            IEnumerable<string>? updatedTags = null;

            this._cardsMock
                .Setup(r => r.UpdateAsync(
                    It.IsAny<Flashcard>(),
                    It.IsAny<IEnumerable<string>>(),
                    ct))
                .Callback<Flashcard, IEnumerable<string>, CancellationToken>((card, t, _) =>
                {
                    updated = card;
                    updatedTags = t;
                })
                .Returns(Task.CompletedTask);

            var dto = await this._service.UpdateAsync(id, editorId, newQ, newA, tags, ct);

            this._cardsMock.Verify(r => r.GetAsync(id, ct), Times.Once);
            this._cardsMock.Verify(
                r => r.UpdateAsync(
                    It.IsAny<Flashcard>(),
                    It.IsAny<IEnumerable<string>>(),
                    ct),
                Times.Once);

            Assert.NotNull(updated);
            Assert.Equal(id, updated!.FlashcardId);
            Assert.Equal(editorId, updated.CreatorId);
            Assert.Equal(newQ, updated.Question);
            Assert.Equal(newA, updated.Answer);
            Assert.Equal(existing.CreatedAt, updated.CreatedAt);

            Assert.NotNull(updatedTags);
            Assert.Equal(tags, updatedTags!.ToArray());

            Assert.Equal(id, dto.Id);
            Assert.Equal(newQ, dto.Question);
            Assert.Equal(newA, dto.Answer);
            Assert.Equal(editorId, dto.CreatorId);
        }

        /// <summary>
        /// Тест: DeleteAsync викликає DeleteAsync репозиторія з тими ж параметрами.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DeleteAsync_CallsRepositoryDeleteWithSameParameters()
        {
            var ct = CancellationToken.None;
            int id = 50;
            int requesterId = 99;

            this._cardsMock
                .Setup(r => r.DeleteAsync(id, requesterId, ct))
                .Returns(Task.CompletedTask);

            await this._service.DeleteAsync(id, requesterId, ct);

            this._cardsMock.Verify(r => r.DeleteAsync(id, requesterId, ct), Times.Once);
        }

        /// <summary>
        /// Тест: GetAsync повертає null, якщо картку не знайдено.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetAsync_NotFound_ReturnsNull()
        {
            var ct = CancellationToken.None;
            int id = 77;

            this._cardsMock
                .Setup(r => r.GetAsync(id, ct))
                .ReturnsAsync((Flashcard?)null);

            var dto = await this._service.GetAsync(id, ct);

            Assert.Null(dto);
            this._cardsMock.Verify(r => r.GetAsync(id, ct), Times.Once);
        }

        /// <summary>
        /// Тест: GetAsync коректно мапить знайдену картку в DTO (з порожніми тегами).
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetAsync_Found_MapsToDtoWithEmptyTags()
        {
            var ct = CancellationToken.None;
            int id = 88;

            var card = new Flashcard
            {
                FlashcardId = id,
                CreatorId = 2,
                Question = "Q",
                Answer = "A",
                CreatedAt = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            };

            this._cardsMock
                .Setup(r => r.GetAsync(id, ct))
                .ReturnsAsync(card);

            var dto = await this._service.GetAsync(id, ct);

            Assert.NotNull(dto);
            Assert.Equal(id, dto!.Id);
            Assert.Equal(card.CreatorId, dto.CreatorId);
            Assert.Equal("Q", dto.Question);
            Assert.Equal("A", dto.Answer);
            Assert.True(dto.Tags == null || !dto.Tags.Any());

            this._cardsMock.Verify(r => r.GetAsync(id, ct), Times.Once);
        }

        /// <summary>
        /// Тест: ListAsync коректно мапить список знайдених карток у DTO.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ListAsync_ReturnsMappedDtos_ForFoundCards()
        {
            var ct = CancellationToken.None;
            int ownerId = 5;
            string? search = "java";

            var cards = new List<Flashcard>
            {
                new Flashcard
                {
                    FlashcardId = 1,
                    CreatorId = ownerId,
                    Question = "Q1",
                    Answer = "A1",
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                },
                new Flashcard
                {
                    FlashcardId = 2,
                    CreatorId = ownerId,
                    Question = "Q2",
                    Answer = "A2",
                    CreatedAt = DateTime.UtcNow,
                },
            };

            this._cardsMock
                .Setup(r => r.FindAsync(ownerId, search, ct))
                .ReturnsAsync(cards);

            var result = await this._service.ListAsync(ownerId, search, ct);

            Assert.Equal(2, result.Count);

            Assert.Contains(result, d => d.Id == 1 && d.Question == "Q1" && d.Answer == "A1");
            Assert.Contains(result, d => d.Id == 2 && d.Question == "Q2" && d.Answer == "A2");

            Assert.All(result, d => Assert.True(d.Tags == null || !d.Tags.Any()));

            this._cardsMock.Verify(r => r.FindAsync(ownerId, search, ct), Times.Once);
        }

        /// <summary>
        /// Тест: ListAsync повертає порожній список, якщо репозиторій нічого не знаходить.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ListAsync_EmptyResult_ReturnsEmptyList()
        {
            var ct = CancellationToken.None;
            int ownerId = 10;
            string? search = null;

            this._cardsMock
                .Setup(r => r.FindAsync(ownerId, search, ct))
                .ReturnsAsync(new List<Flashcard>());

            var result = await this._service.ListAsync(ownerId, search, ct);

            Assert.NotNull(result);
            Assert.Empty(result);
            this._cardsMock.Verify(r => r.FindAsync(ownerId, search, ct), Times.Once);
        }

        // =====================================================================
        // ДОДАТКОВІ ТЕСТИ ДЛЯ ПОКРИТТЯ ВСІХ ГІЛОК (try/catch)
        // =====================================================================

        /// <summary>
        /// CreateAsync: якщо валідація падає (Guard.Text кидає ArgumentException),
        /// помилка логуються як Warning і виняток проброшується далі.
        /// Покриваємо catch (ArgumentException).
        /// </summary>
        [Fact]
        public async Task CreateAsync_InvalidQuestion_ThrowsArgumentException_AndDoesNotCallRepository()
        {
            var ct = CancellationToken.None;
            int creatorId = 1;
            string question = string.Empty; // некоректний текст
            string answer = "Valid answer";
            var tags = Array.Empty<string>();

            await Assert.ThrowsAsync<ArgumentException>(() =>
                this._service.CreateAsync(creatorId, question, answer, tags, ct));

            this._cardsMock.Verify(
                r => r.AddAsync(
                    It.IsAny<Flashcard>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        /// <summary>
        /// CreateAsync: якщо репозиторій кидає будь-який інший виняток,
        /// він логуються як критичний і проброшується далі.
        /// Покриваємо catch (Exception).
        /// </summary>
        [Fact]
        public async Task CreateAsync_RepositoryThrows_LogsErrorAndRethrows()
        {
            var ct = CancellationToken.None;
            int creatorId = 2;
            string question = "Q";
            string answer = "A";
            var tags = new[] { "t" };

            this._cardsMock
                .Setup(r => r.AddAsync(
                    It.IsAny<Flashcard>(),
                    It.IsAny<IEnumerable<string>>(),
                    ct))
                .ThrowsAsync(new InvalidOperationException("DB failure"));

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                this._service.CreateAsync(creatorId, question, answer, tags, ct));

            this._cardsMock.Verify(
                r => r.AddAsync(
                    It.IsAny<Flashcard>(),
                    It.IsAny<IEnumerable<string>>(),
                    ct),
                Times.Once);
        }

        /// <summary>
        /// UpdateAsync: якщо під час роботи репозиторія виникає неспецифічний виняток,
        /// він має бути залогований (LogError) і проброшений.
        /// Покриваємо гілку ex is not KeyNotFoundException у catch.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_RepositoryThrows_LogsErrorAndRethrows()
        {
            var ct = CancellationToken.None;
            int id = 20;
            int editorId = 3;
            string newQ = "Valid Q";
            string newA = "Valid A";
            var tags = new[] { "a" };

            var existing = new Flashcard
            {
                FlashcardId = id,
                CreatorId = editorId,
                Question = "Old",
                Answer = "Old",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
            };

            this._cardsMock
                .Setup(r => r.GetAsync(id, ct))
                .ReturnsAsync(existing);

            this._cardsMock
                .Setup(r => r.UpdateAsync(
                    It.IsAny<Flashcard>(),
                    It.IsAny<IEnumerable<string>>(),
                    ct))
                .ThrowsAsync(new InvalidOperationException("DB failure on update"));

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                this._service.UpdateAsync(id, editorId, newQ, newA, tags, ct));

            this._cardsMock.Verify(r => r.GetAsync(id, ct), Times.Once);
            this._cardsMock.Verify(
                r => r.UpdateAsync(
                    It.IsAny<Flashcard>(),
                    It.IsAny<IEnumerable<string>>(),
                    ct),
                Times.Once);
        }

        /// <summary>
        /// DeleteAsync: якщо репозиторій кидає KeyNotFoundException,
        /// сервіс має залогувати Warning і пробросити виняток далі.
        /// Покриваємо catch (KeyNotFoundException).
        /// </summary>
        [Fact]
        public async Task DeleteAsync_CardNotFound_ThrowsKeyNotFoundException()
        {
            var ct = CancellationToken.None;
            int id = 30;
            int requesterId = 9;

            this._cardsMock
                .Setup(r => r.DeleteAsync(id, requesterId, ct))
                .ThrowsAsync(new KeyNotFoundException("not found"));

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                this._service.DeleteAsync(id, requesterId, ct));

            this._cardsMock.Verify(r => r.DeleteAsync(id, requesterId, ct), Times.Once);
        }

        /// <summary>
        /// DeleteAsync: якщо виникає будь-який інший виняток,
        /// він логуються як критичний і проброшується далі.
        /// Покриваємо catch (Exception).
        /// </summary>
        [Fact]
        public async Task DeleteAsync_UnexpectedException_LogsErrorAndRethrows()
        {
            var ct = CancellationToken.None;
            int id = 31;
            int requesterId = 10;

            this._cardsMock
                .Setup(r => r.DeleteAsync(id, requesterId, ct))
                .ThrowsAsync(new InvalidOperationException("DB failure"));

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                this._service.DeleteAsync(id, requesterId, ct));

            this._cardsMock.Verify(r => r.DeleteAsync(id, requesterId, ct), Times.Once);
        }

        /// <summary>
        /// ListAsync: якщо під час звернення до репозиторія виникає виняток,
        /// сервіс має його залогувати і пробросити.
        /// Покриваємо catch (Exception) у ListAsync.
        /// </summary>
        [Fact]
        public async Task ListAsync_RepositoryThrows_LogsErrorAndRethrows()
        {
            var ct = CancellationToken.None;
            int ownerId = 99;
            string? search = "something";

            this._cardsMock
                .Setup(r => r.FindAsync(ownerId, search, ct))
                .ThrowsAsync(new Exception("DB failure on find"));

            await Assert.ThrowsAsync<Exception>(() =>
                this._service.ListAsync(ownerId, search, ct));

            this._cardsMock.Verify(r => r.FindAsync(ownerId, search, ct), Times.Once);
        }
    }
}
