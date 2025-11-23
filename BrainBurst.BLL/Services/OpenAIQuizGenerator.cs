namespace BrainBurst.BLL.Services
{
    using System.Text.Json;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using BrainBurst.BLL.Interfaces.Abstractions;

    /// <summary>
    /// Реалізація генератора квізів, що використовує (зараз зімітовано) OpenAI API.
    /// </summary>
    public sealed partial class OpenAIQuizGenerator : IQuizGenerator
    {
        /// <summary>
        /// Ключ API для доступу (наразі заглушка).
        /// </summary>
        private const string ApiKey = "DUMMY_KEY";

        /// <summary>
        /// Системний промпт, який інструктує ШІ щодо формату та змісту відповіді.
        /// </summary>
        private const string SystemPrompt =
            "Ти — експерт з генерації навчальних флеш-карток. Згенеруй список флеш-карток (питання/відповідь/теги) на основі наданого тексту. " +
            "Використовуй формат JSON-масиву з об'єктами { \"Question\": \"...\", \"Answer\": \"...\", \"Tags\": [\"Tag1\", \"Tag2\"] }. " +
            "Згенеруй не менше 5 і не більше 10 карток. Всі питання та відповіді мають бути українською мовою. Текст для аналізу: ";

        /// <summary>
        /// Асинхронно генерує список пар "питання-відповідь" та тегів на основі наданого тексту.
        /// </summary>
        /// <param name="text">Вхідний текст, з якого потрібно згенерувати флеш-картки.</param>
        /// <param name="ct">Токен скасування операції.</param>
        /// <returns>Список кортежів, що містять (Питання, Відповідь, Список тегів).</returns>
        public async Task<IReadOnlyList<(string Question, string Answer, IReadOnlyList<string> Tags)>> GenerateFromTextAsync(
            string text, CancellationToken ct)
        {
            var fullPrompt = SystemPrompt + text;

            var jsonResponse = await this.CallOpenAIAsync(fullPrompt, ct);

            try
            {
                var cards = JsonSerializer.Deserialize<FlashcardResponse[]>(jsonResponse, new JsonSerializerOptions
                { PropertyNameCaseInsensitive = true });

                if (cards == null)
                {
                    return Array.Empty<(string, string, IReadOnlyList<string>)>();
                }

                return cards.Select(c => (c.Question, c.Answer, (IReadOnlyList<string>)c.Tags)).ToList();
            }
            catch (JsonException)
            {
                return Array.Empty<(string, string, IReadOnlyList<string>)>();
            }
        }

        /// <summary>
        /// Імітує асинхронний виклик до API OpenAI.
        /// </summary>
        /// <param name="prompt">Повний промпт для ШІ.</param>
        /// <param name="ct">Токен скасування операції.</param>
        /// <returns>Рядок JSON, що імітує відповідь API.</returns>
        private async Task<string> CallOpenAIAsync(string prompt, CancellationToken ct)
        {
            await Task.Delay(500, ct);

            var mockJson = "[{\"Question\":\"Що таке C#?\",\"Answer\":\"Об'єктно-орієнтована мова програмування\",\"Tags\":[\"Технології\"]},{\"Question\":\"Основна мета BrainBurst?\",\"Answer\":\"Швидке перетворення нотаток на навчальний контент за допомогою ШІ\",\"Tags\":[\"Проект\",\"ШІ\"]},{\"Question\":\"Який фреймворк UI використовується?\",\"Answer\":\"WPF\",\"Tags\":[\"Технології\",\"UI\"]}]";
            return mockJson;
        }

        /// <summary>
        /// Внутрішній клас для десеріалізації відповіді JSON від OpenAI.
        /// </summary>
        private sealed class FlashcardResponse
        {
            public string Question { get; set; } = string.Empty;

            public string Answer { get; set; } = string.Empty;

            public string[] Tags { get; set; } = Array.Empty<string>();
        }
    }
}