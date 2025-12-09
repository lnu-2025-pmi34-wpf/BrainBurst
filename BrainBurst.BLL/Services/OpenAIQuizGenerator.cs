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
    /// Реалізація генератора квізів, що використовує OpenAI API або тестовий делегат.
    /// </summary>
    public class OpenAIQuizGenerator : IQuizGenerator
    {
        /// <summary>
        /// Логер для запису подій.
        /// </summary>
#pragma warning disable SA1401
        protected readonly ILogger<OpenAIQuizGenerator> _logger;
#pragma warning restore SA1401

        /// <summary>
        /// Делегат, який викликається замість реального OpenAI API.
        /// Дає можливість 100% тестування.
        /// </summary>
#pragma warning disable SA1401
        protected readonly Func<string, string, CancellationToken, Task<string>> _chatInvoker;
#pragma warning restore SA1401

        private const string ModelName = "gpt-4o";

        private const string SystemPrompt =
            "Ти — помічник для створення навчальних флеш-карток. " +
            "Твоє завдання: проаналізувати наданий текст і створити на його основі список питань та відповідей. " +
            "Вимоги до формату: ПОВИНЕН повернути лише валідний JSON-масив без жодного додаткового тексту чи форматування, відповідь повинна бути не більше 5 слів. " +
            "Структура: { \"Question\": \"текст питання\", \"Answer\": \"текст відповіді\" }. " +
            "Створи 3–10 карток. Мова: українська.";

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenAIQuizGenerator"/> class.
        /// </summary>
        /// <param name="logger">Логер для запису подій.</param>
        /// <param name="chatInvoker">Делегат-заглушка для тестування (опційно).</param>
        public OpenAIQuizGenerator(
            ILogger<OpenAIQuizGenerator> logger,
            Func<string, string, CancellationToken, Task<string>>? chatInvoker = null)
        {
            this._logger = logger;
            this._chatInvoker = chatInvoker ?? this.DefaultChatInvoker;
            this._logger.LogDebug("OpenAIQuizGenerator: ініціалізовано.");
        }

        /// <summary>
        /// Генерує список флеш-карток на основі вхідного тексту.
        /// </summary>
        /// <param name="text">Текст, який потрібно проаналізувати.</param>
        /// <param name="ct">Токен скасування операції.</param>
        /// <returns>Список кортежів (Питання, Відповідь).</returns>
        /// <exception cref="InvalidOperationException">Викидається, якщо не знайдено OpenAI API Key.</exception>
        public async Task<IReadOnlyList<(string Question, string Answer)>> GenerateFromTextAsync(
            string text, CancellationToken ct)
        {
            this._logger.LogDebug("GenerateFromTextAsync: довжина тексту = {Len}", text.Length);

            if (string.IsNullOrWhiteSpace(text))
            {
                this._logger.LogWarning("GenerateFromTextAsync: текст порожній → повертаємо пустий список.");
                return Array.Empty<(string, string)>();
            }

            string? apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            if (string.IsNullOrEmpty(apiKey))
            {
                this._logger.LogError("OpenAI API Key не знайдено.");
                throw new InvalidOperationException(
                    "OpenAI API Key не знайдено. Встановіть 'OPENAI_API_KEY'.");
            }

            try
            {
                string jsonResponse = await this._chatInvoker(apiKey, text, ct);

                this._logger.LogDebug("AI response: {Resp}", jsonResponse);

                // очищення ```json
                if (jsonResponse.StartsWith("```"))
                {
                    jsonResponse = jsonResponse.Trim('`');
                    if (jsonResponse.StartsWith("json"))
                    {
                        jsonResponse = jsonResponse.Substring(4);
                    }
                }

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var cards = JsonSerializer.Deserialize<FlashcardResponse[]>(jsonResponse, options);

                if (cards == null)
                {
                    this._logger.LogWarning("Десеріалізація повернула null.");
                    return Array.Empty<(string, string)>();
                }

                this._logger.LogInformation("Згенеровано {Count} карток.", cards.Length);

                return cards.Select(c => (c.Question, c.Answer)).ToList();
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Помилка при виклику OpenAI.");
                throw new InvalidOperationException($"Помилка OpenAI: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Реальна взаємодія з OpenAI.
        /// Перевизначається в тестах → дає 100% покриття.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <param name="apiKey">Ключ API OpenAI.</param>
        /// <param name="text">Вхідний текст для аналізу.</param>
        /// <param name="ct">Токен скасування.</param>
        /// <returns>JSON-рядок із відповіддю від моделі.</returns>
        protected virtual async Task<string> DefaultChatInvoker(
            string apiKey,
            string text,
            CancellationToken ct)
        {
            ChatClient client = new ChatClient(model: ModelName, apiKey: apiKey);

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(SystemPrompt),
                new UserChatMessage($"Текст:\n{text}"),
            };

            ChatCompletion result = await client.CompleteChatAsync(messages, cancellationToken: ct);
            return result.Content[0].Text.Trim();
        }

        private class FlashcardResponse
        {
            public string Question { get; set; } = string.Empty;

            public string Answer { get; set; } = string.Empty;
        }
    }
}