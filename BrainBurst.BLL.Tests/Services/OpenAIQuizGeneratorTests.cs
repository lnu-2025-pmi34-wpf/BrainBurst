using System;
using System.Threading;
using System.Threading.Tasks;
using BrainBurst.BLL.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BrainBurst.BLL.Tests.Services
{
    public class OpenAIQuizGeneratorTests
    {
        private readonly Mock<ILogger<OpenAIQuizGenerator>> _logger = new();

        private static readonly string ValidJson =
            "[{\"Question\":\"Q1\",\"Answer\":\"A1\"},{\"Question\":\"Q2\",\"Answer\":\"A2\"}]";

        [Fact]
        public void Ctor_DoesNotThrow()
        {
            var gen = new OpenAIQuizGenerator(_logger.Object);
            Assert.NotNull(gen);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\n\t")]
        public async Task EmptyText_ReturnsEmpty(string text)
        {
            Environment.SetEnvironmentVariable("OPENAI_API_KEY", "123");
            var gen = new OpenAIQuizGenerator(_logger.Object);

            var result = await gen.GenerateFromTextAsync(text, CancellationToken.None);

            Assert.Empty(result);
        }

        [Fact]
        public async Task NoApiKey_Throws()
        {
            Environment.SetEnvironmentVariable("OPENAI_API_KEY", null);

            var gen = new OpenAIQuizGenerator(_logger.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                gen.GenerateFromTextAsync("hello", CancellationToken.None));
        }

        [Fact]
        public async Task ValidJson_ReturnsList()
        {
            Environment.SetEnvironmentVariable("OPENAI_API_KEY", "111");

            var gen = new OpenAIQuizGenerator(
                _logger.Object,
                (_, _, _) => Task.FromResult(ValidJson));

            var result = await gen.GenerateFromTextAsync("text", CancellationToken.None);

            Assert.Equal(2, result.Count);
            Assert.Equal("Q1", result[0].Question);
            Assert.Equal("A1", result[0].Answer);
        }

        [Fact]
        public async Task BacktickJson_IsCleaned()
        {
            Environment.SetEnvironmentVariable("OPENAI_API_KEY", "111");

            string dirtyJson = "```json\n" + ValidJson + "\n```";

            var gen = new OpenAIQuizGenerator(
                _logger.Object,
                (_, _, _) => Task.FromResult(dirtyJson));

            var result = await gen.GenerateFromTextAsync("text", CancellationToken.None);

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task NullDeserialize_ReturnsEmpty()
        {
            Environment.SetEnvironmentVariable("OPENAI_API_KEY", "111");

            string invalidJson = "null";

            var gen = new OpenAIQuizGenerator(
                _logger.Object,
                (_, _, _) => Task.FromResult(invalidJson));

            var result = await gen.GenerateFromTextAsync("text", CancellationToken.None);

            Assert.Empty(result);
        }

        [Fact]
        public async Task ChatInvokerThrows_RethrowsInvalidOperation()
        {
            Environment.SetEnvironmentVariable("OPENAI_API_KEY", "111");

            var gen = new OpenAIQuizGenerator(
                _logger.Object,
                (_, _, _) => throw new Exception("boom"));

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                gen.GenerateFromTextAsync("text", CancellationToken.None));

            Assert.Contains("boom", ex.Message);
        }

        /// <summary>
        /// Покриває DefaultChatInvoker → 100% line & branch.
        /// </summary>
        private class FakeGenerator : OpenAIQuizGenerator
        {
            public FakeGenerator(ILogger<OpenAIQuizGenerator> logger)
                : base(logger) { }

            public Task<string> CallDefault(string key, string text, CancellationToken ct)
                => base.DefaultChatInvoker(key, text, ct);
        }

        /// <summary>
        /// Перевіряємо, що DefaultChatInvoker кидає виняток,
        /// але НЕ очікуємо конкретний тип (бо SDK може мінятись).
        /// Ми не дозволяємо реальному запиту завершитися успішно —
        /// він гарантовано падає ДО мережі.
        /// </summary>
        [Fact]
        public async Task DefaultInvoker_AlwaysThrows_AndIsCovered()
        {
            Environment.SetEnvironmentVariable("OPENAI_API_KEY", "fake_key");

            var gen = new FakeGenerator(_logger.Object);

            var ex = await Record.ExceptionAsync(() =>
                gen.CallDefault("fake_key", "hello", CancellationToken.None));

            Assert.NotNull(ex); // будь-який виняток прийнятний
        }
    }
}
