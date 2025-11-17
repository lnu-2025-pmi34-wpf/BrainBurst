using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BrainBurst.BLL.Services;
using Xunit;

namespace BrainBurst.BLL.Tests.Services
{
    public class OpenAIQuizGeneratorTests
    {
        private readonly OpenAIQuizGenerator _generator;

        public OpenAIQuizGeneratorTests()
        {
            _generator = new OpenAIQuizGenerator();
        }

        // 1. метод повертає список карток, розібраних з mockJson
        [Fact]
        public async Task GenerateFromTextAsync_ReturnsParsedFlashcards_FromMockJson()
        {
            var ct = CancellationToken.None;
            var inputText = "якийсь навчальний текст";

            var result = await _generator.GenerateFromTextAsync(inputText, ct);

            Assert.NotNull(result);
            Assert.NotEmpty(result);

            // у mockJson у класі зараз 3 об’єкти
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

        // 2. перевіряємо, що Tags реально IReadOnlyList<string>, а не null
        [Fact]
        public async Task GenerateFromTextAsync_TagsAreNonNullAndReadOnlyList()
        {
            var ct = CancellationToken.None;

            var result = await _generator.GenerateFromTextAsync("будь-який текст", ct);

            Assert.All(result, item =>
            {
                Assert.NotNull(item.Tags);
                Assert.True(item.Tags.Count >= 0);
            });
        }

        // 3. токен відміни має скасовувати операцію (TaskCanceledException)
        [Fact]
        public async Task GenerateFromTextAsync_CanceledToken_ThrowsTaskCanceledException()
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel(); // скасовуємо до виклику, щоб Task.Delay(500, ct) одразу кинув

            await Assert.ThrowsAsync<TaskCanceledException>(async () =>
            {
                await _generator.GenerateFromTextAsync("будь-який текст", cts.Token);
            });
        }
    }
}
