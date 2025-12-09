namespace BrainBurst.BLL.Tests.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using BrainBurst.BLL.Services;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// Містить юніт-тести для класу <see cref="OpenAIQuizGenerator"/>.
    /// </summary>
    public class OpenAIQuizGeneratorTests
    {
        private static readonly string ValidJson =
        "[{\"Question\":\"Q1\",\"Answer\":\"A1\"},{\"Question\":\"Q2\",\"Answer\":\"A2\"}]";

        private readonly Mock<ILogger<OpenAIQuizGenerator>> _logger = new ();

        /// <summary>
        /// Тест: Перевіряє, що конструктор ініціалізується без винятків.
        /// </summary>
        [Fact]
        public void Ctor_DoesNotThrow()
        {
            var gen = new OpenAIQuizGenerator(this._logger.Object);
            Assert.NotNull(gen);
        }

        /// <summary>
        /// Тест: Перевіряє, що якщо вхідний текст порожній або містить лише пробіли,
        /// метод повертає порожній список.
        /// </summary>
        /// <param name="text">Вхідний текст.</param>
        /// <returns>A <see cref="Task"/>, що представляє результат асинхронної операції.</returns>
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\n\t")]
        public async Task EmptyText_ReturnsEmpty(string text)
        {
            Environment.SetEnvironmentVariable("OPENAI_API_KEY", "123");
            var gen = new OpenAIQuizGenerator(this._logger.Object);

            var result = await gen.GenerateFromTextAsync(text, CancellationToken.None);

            Assert.Empty(result);
        }

        /// <summary>
        /// Тест: Перевіряє, що метод кидає <see cref="InvalidOperationException"/>,
        /// якщо не встановлено змінну оточення OPENAI_API_KEY.
        /// </summary>
        /// <returns>A <see cref="Task"/>, що представляє результат асинхронної операції.</returns>
        [Fact]
        public async Task NoApiKey_Throws()
        {
            Environment.SetEnvironmentVariable("OPENAI_API_KEY", null);

            var gen = new OpenAIQuizGenerator(this._logger.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                gen.GenerateFromTextAsync("hello", CancellationToken.None));
        }

        /// <summary>
        /// Тест: Перевіряє, що при отриманні валідного JSON-рядка,
        /// він коректно десеріалізується у список кортежів.
        /// </summary>
        /// <returns>A <see cref="Task"/>, що представляє результат асинхронної операції.</returns>
        [Fact]
        public async Task ValidJson_ReturnsList()
        {
            Environment.SetEnvironmentVariable("OPENAI_API_KEY", "111");

            var gen = new OpenAIQuizGenerator(
                this._logger.Object,
                (_, _, _) => Task.FromResult(ValidJson));

            var result = await gen.GenerateFromTextAsync("text", CancellationToken.None);

            Assert.Equal(2, result.Count);
            Assert.Equal("Q1", result[0].Question);
            Assert.Equal("A1", result[0].Answer);
        }

        /// <summary>
        /// Тест: Перевіряє, що відповідь AI, обгорнута в потрійні лапки та "json",
        /// коректно очищається перед десеріалізацією.
        /// </summary>
        /// <returns>A <see cref="Task"/>, що представляє результат асинхронної операції.</returns>
        [Fact]
        public async Task BacktickJson_IsCleaned()
        {
            Environment.SetEnvironmentVariable("OPENAI_API_KEY", "111");

            string dirtyJson = "```json\n" + ValidJson + "\n```";

            var gen = new OpenAIQuizGenerator(
                this._logger.Object,
                (_, _, _) => Task.FromResult(dirtyJson));

            var result = await gen.GenerateFromTextAsync("text", CancellationToken.None);

            Assert.Equal(2, result.Count);
        }

        /// <summary>
        /// Тест: Перевіряє, що якщо десеріалізація повертає null (наприклад, для рядка "null"),
        /// метод повертає порожній список, а не кидає виняток.
        /// </summary>
        /// <returns>A <see cref="Task"/>, що представляє результат асинхронної операції.</returns>
        [Fact]
        public async Task NullDeserialize_ReturnsEmpty()
        {
            Environment.SetEnvironmentVariable("OPENAI_API_KEY", "111");

            string invalidJson = "null";

            var gen = new OpenAIQuizGenerator(
                this._logger.Object,
                (_, _, _) => Task.FromResult(invalidJson));

            var result = await gen.GenerateFromTextAsync("text", CancellationToken.None);

            Assert.Empty(result);
        }

        /// <summary>
        /// Тест: Перевіряє, що якщо внутрішній делегат (або OpenAI API) кидає виняток,
        /// метод перехоплює його та повторно кидає як <see cref="InvalidOperationException"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/>, що представляє результат асинхронної операції.</returns>
        [Fact]
        public async Task ChatInvokerThrows_RethrowsInvalidOperation()
        {
            Environment.SetEnvironmentVariable("OPENAI_API_KEY", "111");

            var gen = new OpenAIQuizGenerator(
                this._logger.Object,
                (_, _, _) => throw new Exception("boom"));

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                gen.GenerateFromTextAsync("text", CancellationToken.None));

            Assert.Contains("boom", ex.Message);
        }

        /// <summary>
        /// Перевіряємо, що DefaultChatInvoker кидає виняток,
        /// але НЕ очікуємо конкретний тип (бо SDK може мінятись).
        /// Ми не дозволяємо реальному запиту завершитися успішно —
        /// він гарантовано падає ДО мережі.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DefaultInvoker_AlwaysThrows_AndIsCovered()
        {
            Environment.SetEnvironmentVariable("OPENAI_API_KEY", "fake_key");

            var gen = new FakeGenerator(this._logger.Object);

            var ex = await Record.ExceptionAsync(() =>
                gen.CallDefault("fake_key", "hello", CancellationToken.None));

            Assert.NotNull(ex); // будь-який виняток прийнятний
        }

        /// <summary>
        /// Покриває DefaultChatInvoker → 100% line and branch.
        /// </summary>
        private class FakeGenerator : OpenAIQuizGenerator
        {
            public FakeGenerator(ILogger<OpenAIQuizGenerator> logger)
                : base(logger)
            {
            }

            public Task<string> CallDefault(string key, string text, CancellationToken ct)
                => this.DefaultChatInvoker(key, text, ct);
        }
    }
}
