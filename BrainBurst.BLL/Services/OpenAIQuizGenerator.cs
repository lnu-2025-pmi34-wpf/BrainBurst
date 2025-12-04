using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BrainBurst.BLL.Interfaces.Abstractions;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;

namespace BrainBurst.BLL.Services
{
    /// <summary>
    /// Реалізація генератора квізів, що використовує OpenAI API або тестовий делегат.
    /// </summary>
    public class OpenAIQuizGenerator : IQuizGenerator
    {
        private const string ModelName = "gpt-4o";

        private const string SystemPrompt =
            "Ти — помічник для створення навчальних флеш-карток. " +
            "Твоє завдання: проаналізувати наданий текст і створити на його основі список питань та відповідей. " +
            "Вимоги до формату: ПОВИНЕН повернути лише валідний JSON-масив без жодного додаткового тексту чи форматування. " +
            "Структура: { \"Question\": \"текст питання\", \"Answer\": \"текст відповіді\" }. " +
            "Створи 3–10 карток. Мова: українська.";

        protected readonly ILogger<OpenAIQuizGenerator> _logger;

        /// <summary>
        /// Делегат, який викликається замість реального OpenAI API.
        /// Дає можливість 100% тестування.
        /// </summary>
        protected readonly Func<string, string, CancellationToken, Task<string>> _chatInvoker;

        public OpenAIQuizGenerator(
            ILogger<OpenAIQuizGenerator> logger,
            Func<string, string, CancellationToken, Task<string>>? chatInvoker = null)
        {
            _logger = logger;
            _chatInvoker = chatInvoker ?? DefaultChatInvoker;
            _logger.LogDebug("OpenAIQuizGenerator: ініціалізовано.");
        }

        public async Task<IReadOnlyList<(string Question, string Answer)>> GenerateFromTextAsync(
            string text, CancellationToken ct)
        {
            _logger.LogDebug("GenerateFromTextAsync: довжина тексту = {Len}", text.Length);

            if (string.IsNullOrWhiteSpace(text))
            {
                _logger.LogWarning("GenerateFromTextAsync: текст порожній → повертаємо пустий список.");
                return Array.Empty<(string, string)>();
            }

            string? apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogError("OpenAI API Key не знайдено.");
                throw new InvalidOperationException(
                    "OpenAI API Key не знайдено. Встановіть 'OPENAI_API_KEY'.");
            }

            try
            {
                string jsonResponse = await _chatInvoker(apiKey, text, ct);

                _logger.LogDebug("AI response: {Resp}", jsonResponse);

                // очищення ```json
                if (jsonResponse.StartsWith("```"))
                {
                    jsonResponse = jsonResponse.Trim('`');
                    if (jsonResponse.StartsWith("json"))
                        jsonResponse = jsonResponse.Substring(4);
                }

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var cards = JsonSerializer.Deserialize<FlashcardResponse[]>(jsonResponse, options);

                if (cards == null)
                {
                    _logger.LogWarning("Десеріалізація повернула null.");
                    return Array.Empty<(string, string)>();
                }

                _logger.LogInformation("Згенеровано {Count} карток.", cards.Length);

                return cards.Select(c => (c.Question, c.Answer)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка при виклику OpenAI.");
                throw new InvalidOperationException($"Помилка OpenAI: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Реальна взаємодія з OpenAI.  
        /// Перевизначається в тестах → дає 100% покриття.
        /// </summary>
        protected virtual async Task<string> DefaultChatInvoker(
            string apiKey,
            string text,
            CancellationToken ct)
        {
            ChatClient client = new ChatClient(model: ModelName, apiKey: apiKey);

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(SystemPrompt),
                new UserChatMessage($"Текст:\n{text}")
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