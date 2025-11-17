using BrainBurst.BLL.Interfaces.Abstractions;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

// Примітка: Цей клас вимагає встановлення NuGet пакету для OpenAI.

namespace BrainBurst.BLL.Services
{
    public sealed partial class OpenAIQuizGenerator : IQuizGenerator
    {
        // УВАГА: Ключ має бути завантажений зі змінних середовища!
        private const string ApiKey = "DUMMY_KEY";
        private const string SystemPrompt = 
            "Ти — експерт з генерації навчальних флеш-карток. Згенеруй список флеш-карток (питання/відповідь/теги) на основі наданого тексту. " +
            "Використовуй формат JSON-масиву з об'єктами { \"Question\": \"...\", \"Answer\": \"...\", \"Tags\": [\"Tag1\", \"Tag2\"] }. " +
            "Згенеруй не менше 5 і не більше 10 карток. Всі питання та відповіді мають бути українською мовою. Текст для аналізу: ";
        
        // Модель-заглушка для імітації відповіді OpenAI
        private sealed class FlashcardResponse
        {
            public string Question { get; set; } = string.Empty;
            public string Answer { get; set; } = string.Empty;
            public string[] Tags { get; set; } = Array.Empty<string>();
        }

        // Імітуємо HTTP-запит до API
        private async Task<string> CallOpenAIAsync(string prompt, CancellationToken ct)
        {
            // У реальному житті тут був би виклик:
            // var client = new OpenAI.OpenAIClient(new OpenAI.OpenAIClientSettings(ApiKey));
            // var response = await client.Chat.CreateAsync(...);

            await Task.Delay(500, ct); // Імітуємо затримку мережі
            
            // Імітація відповіді OpenAI у JSON форматі
            var mockJson = "[{\"Question\":\"Що таке C#?\",\"Answer\":\"Об'єктно-орієнтована мова програмування\",\"Tags\":[\"Технології\"]},{\"Question\":\"Основна мета BrainBurst?\",\"Answer\":\"Швидке перетворення нотаток на навчальний контент за допомогою ШІ\",\"Tags\":[\"Проект\",\"ШІ\"]},{\"Question\":\"Який фреймворк UI використовується?\",\"Answer\":\"WPF\",\"Tags\":[\"Технології\",\"UI\"]}]";
            return mockJson;
        }

        public async Task<IReadOnlyList<(string Question, string Answer, IReadOnlyList<string> Tags)>> GenerateFromTextAsync(
            string text, CancellationToken ct)
        {
            // 1. Формуємо повний запит
            var fullPrompt = SystemPrompt + text;

            // 2. Викликаємо OpenAI API
            var jsonResponse = await CallOpenAIAsync(fullPrompt, ct);

            // 3. Десеріалізація JSON відповіді
            try
            {
                var cards = JsonSerializer.Deserialize<FlashcardResponse[]>(jsonResponse, new JsonSerializerOptions
                { PropertyNameCaseInsensitive = true });
                
                if (cards == null) return Array.Empty<(string, string, IReadOnlyList<string>)>();

                return cards.Select(c => (c.Question, c.Answer, (IReadOnlyList<string>)c.Tags)).ToList();
            }
            catch (JsonException)
            {
                // Обробка випадків, коли API повертає невірний JSON
                return Array.Empty<(string, string, IReadOnlyList<string>)>();
            }
        }
    }
}