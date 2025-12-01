namespace BrainBurst.BLL.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using BrainBurst.BLL.Interfaces.Abstractions;
    using Microsoft.Extensions.Logging;
    using OpenAI.Chat;

    /// <summary>
    /// Реалізація генератора квізів, що використовує реальний OpenAI API.
    /// </summary>
    public sealed class OpenAIQuizGenerator : IQuizGenerator
    {
        // Використовуємо модель gpt-4o
        private const string ModelName = "gpt-4o";

        /// <summary>
        /// Системний промпт, який інструктує ШІ щодо формату відповіді.
        /// Ми суворо вимагаємо JSON формат без тегів.
        /// </summary>
        private const string SystemPrompt =
            "Ти — помічник для створення навчальних флеш-карток. " +
            "Твоє завдання: проаналізувати наданий текст і створити на його основі список питань та відповідей. " +
            "Вимоги до формату: ПОВИНЕН повернути лише валідний JSON-масив без жодного додаткового тексту чи форматування (наприклад, без ```json). " +
            "Структура JSON об'єкта: { \"Question\": \"текст питання\", \"Answer\": \"текст відповіді\" }. " +
            "Створи від 3 до 10 карток залежно від розміру тексту. Мова: українська. " +
            "Не додавай теги до карток - це зроблять користувачі. Зосередься тільки на якісних питаннях та відповідях.";

        private readonly ILogger<OpenAIQuizGenerator> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenAIQuizGenerator"/> class.
        /// </summary>
        /// <param name="logger">Логер для запису подій.</param> // <-- ДОДАЙТЕ КОМЕНТАР
        public OpenAIQuizGenerator(ILogger<OpenAIQuizGenerator> logger) // <-- ДОДАЙТЕ КОНСТРУКТОР
        {
            this._logger = logger;
            this._logger.LogDebug("OpenAIQuizGenerator: Сервіс ініціалізовано.");
        }

        /// <summary>
        /// Асинхронно генерує список пар "питання-відповідь" на основі наданого тексту.
        /// </summary>
        /// <param name="text">Вхідний текст для аналізу.</param>
        /// <param name="ct">Токен скасування операції.</param>
        /// <returns>Список кортежів (Питання, Відповідь).</returns>
        public async Task<IReadOnlyList<(string Question, string Answer)>> GenerateFromTextAsync(
            string text, CancellationToken ct)
        {
            this._logger.LogDebug("GenerateFromTextAsync: Запущено генерацію для тексту довжиною {TextLength}.", text.Length);

            if (string.IsNullOrWhiteSpace(text))
            {
                this._logger.LogWarning("GenerateFromTextAsync: Вхідний текст порожній. Повертаємо пустий список.");
                return Array.Empty<(string, string)>();
            }

            // Отримуємо ключ з змінних середовища
            string? apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

            if (string.IsNullOrEmpty(apiKey))
            {
                this._logger.LogError("GenerateFromTextAsync: OpenAI API Key не знайдено.");
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

                this._logger.LogDebug("GenerateFromTextAsync: Відправка запиту до моделі {ModelName}.", ModelName);

                // Відправляємо запит
                ChatCompletion completion = await client.CompleteChatAsync(messages, cancellationToken: ct);

                // Отримуємо відповідь
                string jsonResponse = completion.Content[0].Text.Trim();

                this._logger.LogDebug("GenerateFromTextAsync: Отримана відповідь від AI: {Response}", jsonResponse);

                // Очищаємо від можливих маркерів коду (```json ... ```, від 3 до 10 карток)
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
                    this._logger.LogWarning("GenerateFromTextAsync: Десеріалізація повернула null. JSON не відповідає очікуваному формату.");
                    return Array.Empty<(string, string)>();
                }

                this._logger.LogInformation("GenerateFromTextAsync: Успішно згенеровано {Count} карток.", cards.Length);

                // Перетворюємо у формат кортежів (Question, Answer)
                return cards.Select(c => (c.Question, c.Answer)).ToList();
            }
            catch (Exception ex)
            {
                // Логування помилки
                this._logger.LogError(ex, "GenerateFromTextAsync: Критична помилка під час взаємодії з OpenAI API.");
                throw new InvalidOperationException($"Помилка OpenAI: {ex.Message}", ex);
            }
        }

        // Внутрішній клас для парсингу JSON
        private class FlashcardResponse
        {
            public string Question { get; set; } = string.Empty;

            public string Answer { get; set; } = string.Empty;
        }
    }
}