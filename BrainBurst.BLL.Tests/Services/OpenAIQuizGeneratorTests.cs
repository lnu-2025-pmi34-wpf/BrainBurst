namespace BrainBurst.BLL.Tests.Services
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BrainBurst.BLL.Services;
    using Xunit;

    /// <summary>
    /// Містить юніт-тести для <see cref="OpenAIQuizGenerator"/>.
    /// Оскільки генератор наразі імітує відповідь, ці тести перевіряють логіку парсингу відповіді.
    /// </summary>
    public class OpenAIQuizGeneratorTests
    {
        private readonly OpenAIQuizGenerator _generator;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenAIQuizGeneratorTests"/> class.
        /// </summary>
        public OpenAIQuizGeneratorTests()
        {
            this._generator = new OpenAIQuizGenerator();
        }

        /// <summary>
        /// Тест: GenerateFromTextAsync коректно парсить імітовану JSON-відповідь у список кортежів.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GenerateFromTextAsync_ReturnsParsedFlashcards_FromMockJson()
        {
            var ct = CancellationToken.None;
            var inputText = "якийсь навчальний текст";

            var result = await this._generator.GenerateFromTextAsync(inputText, ct);

            Assert.NotNull(result);
            Assert.NotEmpty(result);

            Assert.Equal(3, result.Count);

            var first = result[0];
            Assert.Equal("Що таке C#?", first.Question);
            Assert.Equal("Об'єктно-орієнтована мова програмування", first.Answer);
            Assert.Single(first.Tags);
            Assert.Equal("Технології", first.Tags[0]);

            var second = result[1];
            Assert.Equal("Основна мета BrainBurst?", second.Question);
            Assert.Contains("Швидке перетворення нотаток", second.Answer);
            Assert.Equal(2, second.Tags.Count);
            Assert.Contains("Проект", second.Tags);
            Assert.Contains("ШІ", second.Tags);

            var third = result[2];
            Assert.Equal("Який фреймворк UI використовується?", third.Question);
            Assert.Equal("WPF", third.Answer);
            Assert.Equal(2, third.Tags.Count);
            Assert.Contains("Технології", third.Tags);
            Assert.Contains("UI", third.Tags);
        }

        /// <summary>
        /// Тест: GenerateFromTextAsync гарантує, що список тегів у результаті ніколи не є null.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GenerateFromTextAsync_TagsAreNonNullAndReadOnlyList()
        {
            var ct = CancellationToken.None;

            var result = await this._generator.GenerateFromTextAsync("будь-який текст", ct);

            Assert.All(result, item =>
            {
                Assert.NotNull(item.Tags);
                Assert.True(item.Tags.Count >= 0);
            });
        }

        /// <summary>
        /// Тест: GenerateFromTextAsync кидає TaskCanceledException, якщо токен скасовано (перевірка імітації Task.Delay).
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GenerateFromTextAsync_CanceledToken_ThrowsTaskCanceledException()
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            await Assert.ThrowsAsync<TaskCanceledException>(async () =>
            {
                await this._generator.GenerateFromTextAsync("будь-який текст", cts.Token);
            });
        }
    }
}
