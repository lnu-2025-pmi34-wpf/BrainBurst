#pragma warning disable SA1200
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#pragma warning disable SA1210 // Using directives should be ordered alphabetically by namespace
using BrainBurst.DAL.Entities;
#pragma warning restore SA1210 // Using directives should be ordered alphabetically by namespace
using BrainBurst.BLL.Interfaces.Abstractions;
#pragma warning disable SA1210 // Using directives should be ordered alphabetically by namespace
using BrainBurst.DAL.Abstractions;
#pragma warning restore SA1210 // Using directives should be ordered alphabetically by namespace
using BrainBurst.BLL.Mapping;
using Microsoft.Extensions.Logging;
#pragma warning restore SA1200

namespace BrainBurst.BLL.Services;

/// <summary>
/// Реалізація сервісу, що відповідає за генерацію флеш-карток з тексту.
/// </summary>
public sealed class TestGenerationService : ITestGenerationService
{
    private readonly IQuizGenerator _ai;
    private readonly IFlashcardRepository _cards;
    private readonly ILogger<TestGenerationService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestGenerationService"/> class.
    /// </summary>
    /// <param name="ai">Сервіс-генератор квізів (наприклад, OpenAI).</param>
    /// <param name="cards">Репозиторій для збереження флеш-карток.</param>
    /// <param name="logger">Логер для запису подій.</param>
    public TestGenerationService(IQuizGenerator ai, IFlashcardRepository cards, ILogger<TestGenerationService> logger)
    {
        this._ai = ai;
        this._cards = cards;
        this._logger = logger;

        this._logger.LogDebug("TestGenerationService: Сервіс генерації тестів ініціалізовано.");
    }

    /// <summary>
    /// Асинхронно створює та зберігає флеш-картки на основі наданого тексту.
    /// Теги передаються окремо від користувача через UI.
    /// </summary>
    /// <param name="creatorId">ID користувача, який створює картки.</param>
    /// <param name="text">Вхідний текст для аналізу та генерації карток.</param>
    /// <param name="tags">Список тегів, які призначити всім створеним карткам.</param>
    /// <param name="ct">Токен скасування операції.</param>
    /// <returns>Список <see cref="FlashcardDTO"/> новостворених флеш-карток.</returns>
    public async Task<IReadOnlyList<FlashcardDTO>> CreateFlashcardsFromTextAsync(
        int creatorId, string text, IEnumerable<string> tags, CancellationToken ct)
    {
        this._logger.LogInformation(
            "CreateFlashcardsFromTextAsync: Запущено генерацію карток користувачем {CreatorId} для тексту довжиною {TextLength}.", creatorId, text.Length);

        try
        {
            this._logger.LogDebug("CreateFlashcardsFromTextAsync: Виклик AI генератора...");
            var items = await this._ai.GenerateFromTextAsync(text, ct);

            this._logger.LogInformation("CreateFlashcardsFromTextAsync: AI повернув {Count} пар (питання/відповідь).", items.Count);

            if (items.Count == 0)
            {
                this._logger.LogWarning("CreateFlashcardsFromTextAsync: AI повернув 0 карток. Скасування операції.");
                return Array.Empty<FlashcardDTO>();
            }

            var result = new List<FlashcardDTO>();
            var tagsList = tags.ToList();
            int count = 0;

            foreach (var (question, answer) in items)
            {
                this._logger.LogDebug("CreateFlashcardsFromTextAsync: Збереження картки {Index}...", ++count);

                var saved = await this._cards.AddAsync(
                    new Flashcard
                    {
                        Question = question,
                        Answer = answer,
                        CreatorId = creatorId,
                        CreatedAt = DateTime.UtcNow,
                    }, tagsList,
                    ct);

                result.Add(saved.ToDTO(tagsList));
            }

            this._logger.LogInformation(
                "CreateFlashcardsFromTextAsync: Успішно створено та збережено {Count} карток для користувача {CreatorId}.", result.Count, creatorId);

            return result;
        }
        catch (Exception ex)
        {
            this._logger.LogError(
                ex,
                "CreateFlashcardsFromTextAsync: Критична помилка під час генерації або збереження карток для користувача {CreatorId}.",
                creatorId);
            throw;
        }
    }
}
