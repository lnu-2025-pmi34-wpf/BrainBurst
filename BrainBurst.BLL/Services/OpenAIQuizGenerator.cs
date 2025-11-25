namespace BrainBurst.BLL.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using BrainBurst.BLL.Interfaces.Abstractions;
    using OpenAI.Chat; // Переконайтеся, що пакет OpenAI встановлено (версія 2.0+)

    /// <summary>
    /// Реалізація генератора квізів, що використовує реальний OpenAI API.
    /// </summary>
    public sealed class OpenAIQuizGenerator : IQuizGenerator
    {
        // Використовуємо модель gpt-4o або gpt-3.5-turbo (дешевше)
        private const string ModelName = "gpt-4o"; 

        /// <summary>
        /// Системний промпт, який інструктує ШІ щодо формату відповіді.
        /// Ми суворо вимагаємо JSON формат.
        /// </summary>
        private const string SystemPrompt =
            "Ти — помічник для створення навчальних флеш-карток. " +
            "Твоє завдання: проаналізувати наданий текст і створити на його основі список питань та відповідей. " +
            "Вимоги до формату: ПОВИНЕН повернути лише валідний JSON-масив без жодного додаткового тексту чи форматування (наприклад, без ```json). " +
            "Структура JSON об'єкта: { \"Question\": \"текст питання\", \"Answer\": \"текст відповіді\", \"Tags\": [\"тег1\", \"тег2\"] }. " +
            "Створи від 3 до 10 карток залежно від розміру тексту. Мова: українська.";

        public async Task<IReadOnlyList<(string Question, string Answer, IReadOnlyList<string> Tags)>> GenerateFromTextAsync(
            string text, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return Array.Empty<(string, string, IReadOnlyList<string>)>();
            }

            // Отримуємо ключ з змінних середовища (рекомендований спосіб)
            string? apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

            if (string.IsNullOrEmpty(apiKey))
            {
                // Тимчасовий фоллбек для тестування (замініть на свій ключ, якщо не налаштували Env Var)
                // apiKey = "sk-proj-...."; 
                throw new InvalidOperationException("OpenAI API Key не знайдено. Встановіть змінну середовища 'OPENAI_API_KEY'.");
            }

            try
            {
                // Ініціалізація клієнта
                ChatClient client = new ChatClient(model: ModelName, apiKey: apiKey);

                // Формуємо повідомлення
                List<ChatMessage> messages = new()
                {
                    new SystemChatMessage(SystemPrompt),
                    new UserChatMessage($"Текст для аналізу:\n{text}")
                };

                // Відправляємо запит
                ChatCompletion completion = await client.CompleteChatAsync(messages, cancellationToken: ct);

                // Отримуємо відповідь
                string jsonResponse = completion.Content[0].Text.Trim();

                // Очищаємо від можливих маркерів коду (```json ... ```)
                if (jsonResponse.StartsWith("```"))
                {
                    jsonResponse = jsonResponse.Trim('`');
                    if (jsonResponse.StartsWith("json"))
                    {
                        jsonResponse = jsonResponse.Substring(4);
                    }
                }

                // Десеріалізуємо JSON
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var cards = JsonSerializer.Deserialize<FlashcardResponse[]>(jsonResponse, options);

                if (cards == null)
                {
                    return Array.Empty<(string, string, IReadOnlyList<string>)>();
                }

                // Перетворюємо у формат кортежів, який очікує наш сервіс
                return cards.Select(c => (
                    c.Question, 
                    c.Answer, 
                    (IReadOnlyList<string>)c.Tags
                )).ToList();
            }
            catch (Exception ex)
            {
                // Логування помилки (можна додати logger пізніше)
                // Наразі просто повертаємо пустий список або кидаємо помилку далі
                throw new InvalidOperationException($"Помилка OpenAI: {ex.Message}", ex);
            }
        }

        // Внутрішній клас для парсингу JSON
        private class FlashcardResponse
        {
            public string Question { get; set; } = string.Empty;
            public string Answer { get; set; } = string.Empty;
            public string[] Tags { get; set; } = Array.Empty<string>();
        }
    }
}