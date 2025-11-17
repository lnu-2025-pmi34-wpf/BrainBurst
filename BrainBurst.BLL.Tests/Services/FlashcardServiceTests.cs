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

namespace BrainBurst.BLL.Tests.Services
{
    public class FlashcardServiceTests
    {
        private readonly Mock<IFlashcardRepository> _cardsMock;
        private readonly FlashcardService _service;

        public FlashcardServiceTests()
        {
            _cardsMock = new Mock<IFlashcardRepository>(MockBehavior.Strict);
            _service = new FlashcardService(_cardsMock.Object);
        }

        // CreateAsync

        [Fact]
        public async Task CreateAsync_ValidData_CallsAddAsyncWithCorrectEntityAndTags_AndReturnsDto()
        {
            var ct = CancellationToken.None;
            int creatorId = 7;
            string question = "Що таке флеш-картка?";
            string answer   = "Картка з питанням та відповіддю.";
            var tags        = new[] { "tag1", "tag2" };

            Flashcard? capturedCard = null;
            IEnumerable<string>? capturedTags = null;

            _cardsMock
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

            var dto = await _service.CreateAsync(creatorId, question, answer, tags, ct);

            var after = DateTime.UtcNow;

            // репозиторій викликано
            _cardsMock.Verify(r => r.AddAsync(
                It.IsAny<Flashcard>(),
                It.IsAny<IEnumerable<string>>(),
                ct), Times.Once);

            Assert.NotNull(capturedCard);
            Assert.Equal(creatorId, capturedCard!.CreatorId);
            Assert.Equal(question, capturedCard.Question);
            Assert.Equal(answer, capturedCard.Answer);
            Assert.True(capturedCard.CreatedAt >= before && capturedCard.CreatedAt <= after);

            Assert.NotNull(capturedTags);
            Assert.Equal(tags, capturedTags!.ToArray());

            // перевіряємо DTO
            Assert.Equal(question, dto.Question);
            Assert.Equal(answer, dto.Answer);
            Assert.Equal(creatorId, dto.CreatorId);
        }

        // UpdateAsync

        [Fact]
        public async Task UpdateAsync_CardNotFound_ThrowsKeyNotFoundException()
        {
            var ct = CancellationToken.None;
            int id = 10;
            int editorId = 5;

            _cardsMock
                .Setup(r => r.GetAsync(id, ct))
                .ReturnsAsync((Flashcard?)null);

            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.UpdateAsync(id, editorId, "Q", "A", Array.Empty<string>(), ct));

            Assert.Contains("Картку з ID 10 не знайдено", ex.Message);
            _cardsMock.Verify(r => r.GetAsync(id, ct), Times.Once);
            _cardsMock.Verify(r => r.UpdateAsync(It.IsAny<Flashcard>(), It.IsAny<IEnumerable<string>>(), ct),
                Times.Never);
        }

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
                CreatedAt = new DateTime(2024, 1, 1)
            };

            _cardsMock
                .Setup(r => r.GetAsync(id, ct))
                .ReturnsAsync(existing);

            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.UpdateAsync(id, editorId, "New Q", "New A", Array.Empty<string>(), ct));

            Assert.Contains("не знайдено або користувач не є її творцем", ex.Message);
            _cardsMock.Verify(r => r.GetAsync(id, ct), Times.Once);
            _cardsMock.Verify(r => r.UpdateAsync(It.IsAny<Flashcard>(), It.IsAny<IEnumerable<string>>(), ct),
                Times.Never);
        }

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
                CreatedAt = new DateTime(2023, 12, 31, 10, 0, 0, DateTimeKind.Utc)
            };

            _cardsMock
                .Setup(r => r.GetAsync(id, ct))
                .ReturnsAsync(existing);

            Flashcard? updated = null;
            IEnumerable<string>? updatedTags = null;

            _cardsMock
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

            var dto = await _service.UpdateAsync(id, editorId, newQ, newA, tags, ct);

            _cardsMock.Verify(r => r.GetAsync(id, ct), Times.Once);
            _cardsMock.Verify(r => r.UpdateAsync(
                It.IsAny<Flashcard>(),
                It.IsAny<IEnumerable<string>>(),
                ct), Times.Once);

            Assert.NotNull(updated);
            Assert.Equal(id, updated!.FlashcardId);
            Assert.Equal(editorId, updated.CreatorId);
            Assert.Equal(newQ, updated.Question);
            Assert.Equal(newA, updated.Answer);
            Assert.Equal(existing.CreatedAt, updated.CreatedAt); // дата створення збережена

            Assert.NotNull(updatedTags);
            Assert.Equal(tags, updatedTags!.ToArray());

            // DTO теж має нові дані
            Assert.Equal(id, dto.Id);
            Assert.Equal(newQ, dto.Question);
            Assert.Equal(newA, dto.Answer);
            Assert.Equal(editorId, dto.CreatorId);
        }

        // DeleteAsync

        [Fact]
        public async Task DeleteAsync_CallsRepositoryDeleteWithSameParameters()
        {
            var ct = CancellationToken.None;
            int id = 50;
            int requesterId = 99;

            _cardsMock
                .Setup(r => r.DeleteAsync(id, requesterId, ct))
                .Returns(Task.CompletedTask);

            await _service.DeleteAsync(id, requesterId, ct);

            _cardsMock.Verify(r => r.DeleteAsync(id, requesterId, ct), Times.Once);
        }

        // GetAsync

        [Fact]
        public async Task GetAsync_NotFound_ReturnsNull()
        {
            var ct = CancellationToken.None;
            int id = 77;

            _cardsMock
                .Setup(r => r.GetAsync(id, ct))
                .ReturnsAsync((Flashcard?)null);

            var dto = await _service.GetAsync(id, ct);

            Assert.Null(dto);
            _cardsMock.Verify(r => r.GetAsync(id, ct), Times.Once);
        }

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
                CreatedAt = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc)
            };

            _cardsMock
                .Setup(r => r.GetAsync(id, ct))
                .ReturnsAsync(card);

            var dto = await _service.GetAsync(id, ct);

            Assert.NotNull(dto);
            Assert.Equal(id, dto!.Id);
            Assert.Equal(card.CreatorId, dto.CreatorId);
            Assert.Equal("Q", dto.Question);
            Assert.Equal("A", dto.Answer);
            // теги мають бути порожніми
            Assert.True(dto.Tags == null || !dto.Tags.Any());

            _cardsMock.Verify(r => r.GetAsync(id, ct), Times.Once);
        }

        // ListAsync

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
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new Flashcard
                {
                    FlashcardId = 2,
                    CreatorId = ownerId,
                    Question = "Q2",
                    Answer = "A2",
                    CreatedAt = DateTime.UtcNow
                }
            };

            _cardsMock
                .Setup(r => r.FindAsync(ownerId, search, ct))
                .ReturnsAsync(cards);

            var result = await _service.ListAsync(ownerId, search, ct);

            Assert.Equal(2, result.Count);

            Assert.Contains(result, d => d.Id == 1 && d.Question == "Q1" && d.Answer == "A1");
            Assert.Contains(result, d => d.Id == 2 && d.Question == "Q2" && d.Answer == "A2");

            // по замовчуванню теги пусті
            Assert.All(result, d => Assert.True(d.Tags == null || !d.Tags.Any()));

            _cardsMock.Verify(r => r.FindAsync(ownerId, search, ct), Times.Once);
        }

        [Fact]
        public async Task ListAsync_EmptyResult_ReturnsEmptyList()
        {
            var ct = CancellationToken.None;
            int ownerId = 10;
            string? search = null;

            _cardsMock
                .Setup(r => r.FindAsync(ownerId, search, ct))
                .ReturnsAsync(new List<Flashcard>());

            var result = await _service.ListAsync(ownerId, search, ct);

            Assert.NotNull(result);
            Assert.Empty(result);
            _cardsMock.Verify(r => r.FindAsync(ownerId, search, ct), Times.Once);
        }
    }
}
