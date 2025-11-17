using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BrainBurst.BLL.Interfaces.Abstractions; 
using BrainBurst.BLL.Services;
using BrainBurst.DAL.Entities;
using BrainBurst.DAL.Abstractions;
using Moq;
using Xunit;

namespace BrainBurst.BLL.Tests.Services
{
    public class TestGenerationServiceTests
    {
        private readonly Mock<IQuizGenerator> _aiMock;
        private readonly Mock<IFlashcardRepository> _cardsMock;
        private readonly TestGenerationService _service;

        public TestGenerationServiceTests()
        {
            _aiMock = new Mock<IQuizGenerator>(MockBehavior.Strict);
            _cardsMock = new Mock<IFlashcardRepository>(MockBehavior.Strict);

            _service = new TestGenerationService(_aiMock.Object, _cardsMock.Object);
        }

        // 1. перевіряємо, що сервіс передає правильний text у AI
        [Fact]
        public async Task CreateFlashcardsFromTextAsync_CallsQuizGeneratorWithSameText()
        {
            var creatorId = 42;
            var text = "some input text";
            var ct = new CancellationTokenSource().Token;

            var aiResult = Array.Empty<(string Question, string Answer, IReadOnlyList<string> Tags)>();

            _aiMock
                .Setup(ai => ai.GenerateFromTextAsync(text, ct))
                .ReturnsAsync(aiResult);

            // для порожнього результату репозиторій не викликається
            _cardsMock
                .Setup(r => r.AddAsync(It.IsAny<Flashcard>(),
                                       It.IsAny<IReadOnlyList<string>>(),
                                       It.IsAny<CancellationToken>()))
                .ReturnsAsync((Flashcard f, IReadOnlyList<string> tags, CancellationToken _) => f);

            var result = await _service.CreateFlashcardsFromTextAsync(creatorId, text, ct);

            _aiMock.Verify(ai => ai.GenerateFromTextAsync(text, ct), Times.Once);
        }

        // 2. якщо AI повернув порожній список — репозиторій жодного разу не викликається, результат — порожній
        [Fact]
        public async Task CreateFlashcardsFromTextAsync_EmptyAiResult_ReturnsEmptyAndDoesNotCallRepository()
        {
            var creatorId = 1;
            var text = "nothing useful";
            var ct = CancellationToken.None;

            _aiMock
                .Setup(ai => ai.GenerateFromTextAsync(text, ct))
                .ReturnsAsync(Array.Empty<(string Question, string Answer, IReadOnlyList<string> Tags)>());

            var result = await _service.CreateFlashcardsFromTextAsync(creatorId, text, ct);

            Assert.NotNull(result);
            Assert.Empty(result);
            _cardsMock.Verify(r => r.AddAsync(
                    It.IsAny<Flashcard>(),
                    It.IsAny<IReadOnlyList<string>>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        // 3. для кожного згенерованого елементу викликається AddAsync
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
                ("Q3", "A3", Array.Empty<string>())
            };

            _aiMock
                .Setup(ai => ai.GenerateFromTextAsync(text, ct))
                .ReturnsAsync(aiItems);

            // репозиторій повертає просто ту ж Flashcard (ID можна не чіпати)
            _cardsMock
                .Setup(r => r.AddAsync(It.IsAny<Flashcard>(),
                                       It.IsAny<IReadOnlyList<string>>(),
                                       It.IsAny<CancellationToken>()))
                .ReturnsAsync((Flashcard f, IReadOnlyList<string> tags, CancellationToken _) => f);

            var result = await _service.CreateFlashcardsFromTextAsync(creatorId, text, ct);

            _cardsMock.Verify(r => r.AddAsync(
                    It.IsAny<Flashcard>(),
                    It.IsAny<IReadOnlyList<string>>(),
                    It.IsAny<CancellationToken>()),
                Times.Exactly(aiItems.Count));

            Assert.Equal(aiItems.Count, result.Count);
        }

        // 4. перевіряємо, що Question/Answer/CreatorId правильно передаються в репозиторій
        [Fact]
        public async Task CreateFlashcardsFromTextAsync_PassesCorrectFlashcardDataToRepository()
        {
            var creatorId = 777;
            var text = "unit test text";
            var ct = CancellationToken.None;

            var tags = new[] { "tagA", "tagB" };
            var aiItems = new[]
            {
                (Question: "What is BrainBurst?", Answer: "Project description", Tags: (IReadOnlyList<string>)tags)
            };

            _aiMock
                .Setup(ai => ai.GenerateFromTextAsync(text, ct))
                .ReturnsAsync(aiItems);

            Flashcard? capturedFlashcard = null;
            IEnumerable<string>? capturedTags = null;

            _cardsMock
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

            var result = await _service.CreateFlashcardsFromTextAsync(creatorId, text, ct);

            Assert.NotNull(capturedFlashcard);
            Assert.Equal(aiItems[0].Question, capturedFlashcard!.Question);
            Assert.Equal(aiItems[0].Answer, capturedFlashcard.Answer);
            Assert.Equal(creatorId, capturedFlashcard.CreatorId);

            Assert.NotNull(capturedTags);
            Assert.Equal(tags, capturedTags!.ToArray());
        }

        // 5. перевіряємо, що CreatedAt ставиться приблизно зараз (UTC)
        [Fact]
        public async Task CreateFlashcardsFromTextAsync_SetsCreatedAtToUtcNow()
        {
            var creatorId = 5;
            var text = "time test";
            var ct = CancellationToken.None;

            var aiItems = new[]
            {
                ("Q time", "A time", (IReadOnlyList<string>)new[] { "time" })
            };

            _aiMock
                .Setup(ai => ai.GenerateFromTextAsync(text, ct))
                .ReturnsAsync(aiItems);

            DateTime utcBefore = DateTime.UtcNow;

            Flashcard? captured = null;

            _cardsMock
                .Setup(r => r.AddAsync(
                            It.IsAny<Flashcard>(),
                            It.IsAny<IEnumerable<string>>(),
                            It.IsAny<CancellationToken>()))
                .Callback<Flashcard, IEnumerable<string>, CancellationToken>((f, tags, _) =>
                {
                    captured = f;
                })
                .ReturnsAsync((Flashcard f, IEnumerable<string> t, CancellationToken _) => f);

            var result = await _service.CreateFlashcardsFromTextAsync(creatorId, text, ct);

            DateTime utcAfter = DateTime.UtcNow;

            Assert.NotNull(captured);
            Assert.True(captured!.CreatedAt >= utcBefore && captured.CreatedAt <= utcAfter,
                $"CreatedAt {captured.CreatedAt:o} має бути між {utcBefore:o} та {utcAfter:o}");
        }

        // 6. CancellationToken передається далі в репозиторій
        [Fact]
        public async Task CreateFlashcardsFromTextAsync_PropagatesCancellationTokenToRepository()
        {
            var creatorId = 9;
            var text = "cancellation";
            using var cts = new CancellationTokenSource();

            var ct = cts.Token;

            var aiItems = new[]
            {
                ("Q", "A", (IReadOnlyList<string>)Array.Empty<string>())
            };

            _aiMock
                .Setup(ai => ai.GenerateFromTextAsync(text, ct))
                .ReturnsAsync(aiItems);

            _cardsMock
                .Setup(r => r.AddAsync(
                            It.IsAny<Flashcard>(),
                            It.IsAny<IReadOnlyList<string>>(),
                            It.Is<CancellationToken>(token => token == ct)))
                .ReturnsAsync((Flashcard f, IReadOnlyList<string> t, CancellationToken _) => f);

            var result = await _service.CreateFlashcardsFromTextAsync(creatorId, text, ct);

            _cardsMock.Verify(r => r.AddAsync(
                    It.IsAny<Flashcard>(),
                    It.IsAny<IReadOnlyList<string>>(),
                    It.Is<CancellationToken>(token => token == ct)),
                Times.Once);
        }

        // 7. перевіряємо, що кількість результатів = кількості aiItems, і порядок збережено
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

            _aiMock
                .Setup(ai => ai.GenerateFromTextAsync(text, ct))
                .ReturnsAsync(aiItems);

            // повертаємо Flashcard з тими ж Question/Answer
            _cardsMock
                .Setup(r => r.AddAsync(It.IsAny<Flashcard>(),
                                       It.IsAny<IReadOnlyList<string>>(),
                                       It.IsAny<CancellationToken>()))
                .ReturnsAsync((Flashcard f, IReadOnlyList<string> t, CancellationToken _) => f);

            var result = await _service.CreateFlashcardsFromTextAsync(creatorId, text, ct);

            Assert.Equal(aiItems.Count, result.Count);
            Assert.Equal(aiItems[0].Question, result[0].Question);
            Assert.Equal(aiItems[0].Answer, result[0].Answer);
            Assert.Equal(aiItems[1].Question, result[1].Question);
            Assert.Equal(aiItems[1].Answer, result[1].Answer);
        }
    }
}
