namespace BrainBurst.BLL.Mapping;

using System;
using System.Linq;
using System.Collections.Generic;
using BrainBurst.BLL.DTO;
using BrainBurst.BLL.Interfaces;
using BrainBurst.DAL.Entities;

/// <summary>
/// Статичний клас, що містить методи-розширення для мапінгу (перетворення)
/// сутностей DAL в об'єкти передачі даних (DTO) BLL.
/// </summary>
public static class MappingExtensions
{
    /// <summary>
    /// Перетворює сутність <see cref="User"/> на <see cref="UserDTO"/>.
    /// </summary>
    /// <param name="u">Сутність користувача (DAL).</param>
    /// <param name="rating">Сервіс для визначення рангу та його мітки.</param>
    /// <returns>DTO користувача <see cref="UserDTO"/>.</returns>
    public static UserDTO ToDTO(this User u, IRatingService rating) => new ()
    {
        Id = u.UserId,
        Email = u.Email ?? string.Empty,
        FullName = u.FullName ?? string.Empty,
        Points = u.Points,
        Rank = rating.GetRank(u.Points),
        RankLabel = rating.GetRankLabel(rating.GetRank(u.Points)),
        CreatedAt = u.CreatedAt,
    };

    /// <summary>
    /// Перетворює сутність <see cref="Flashcard"/> на <see cref="FlashcardDTO"/>.
    /// </summary>
    /// <param name="f">Сутність флеш-картки (DAL).</param>
    /// <param name="tags">Список тегів, що асоціюються з цією карткою.</param>
    /// <returns>DTO флеш-картки <see cref="FlashcardDTO"/>.</returns>
    public static FlashcardDTO ToDTO(this Flashcard f, IEnumerable<string> tags) => new ()
    {
        Id = f.FlashcardId,
        Question = f.Question ?? string.Empty,
        Answer = f.Answer ?? string.Empty,
        CreatorId = f.CreatorId,
        CreatedAt = f.CreatedAt,
        Tags = tags?.ToList() ?? new List<string>(),
    };

    /// <summary>
    /// Перетворює колекцію сутностей <see cref="Flashcard"/> на <see cref="TestDTO"/>.
    /// </summary>
    /// <param name="cards">Колекція флеш-карток, що входять до тесту.</param>
    /// <param name="testId">ID новоствореного тесту.</param>
    /// <param name="creatorId">ID творця тесту.</param>
    /// <returns>DTO тесту <see cref="TestDTO"/>.</returns>
    public static TestDTO ToTestDTO(this IEnumerable<Flashcard> cards, int testId, int creatorId) =>
        new ()
        {
            Id = testId,
            CreatorId = creatorId,
            Questions = cards.Select(c => new FlashcardDTO
            {
                Id = c.FlashcardId,
                Question = c.Question ?? string.Empty,
                Answer = c.Answer ?? string.Empty,
                CreatorId = c.CreatorId,
                CreatedAt = c.CreatedAt,
                Tags = Array.Empty<string>(),
            }).ToList(),
        };

    /// <summary>
    /// Перетворює сутність <see cref="QuestionResult"/> на <see cref="QuestionResultDTO"/>.
    /// </summary>
    /// <param name="q">Сутність результату питання (DAL).</param>
    /// <returns>DTO результату питання <see cref="QuestionResultDTO"/>.</returns>
    public static QuestionResultDTO ToDTO(this QuestionResult q) => new ()
    {
        Id = q.QuestionResultId,
        FlashcardId = q.FlashcardId,
        UserInput = q.UserInput ?? string.Empty,
        IsCorrect = q.IsCorrect,
    };

    /// <summary>
    /// Перетворює сутність <see cref="TestResult"/> на <see cref="TestResultDTO"/>.
    /// </summary>
    /// <param name="tr">Сутність результату тесту (DAL).</param>
    /// <param name="qrs">Список DTO відповідей на питання, що відносяться до цього тесту.</param>
    /// <returns>DTO результату тесту <see cref="TestResultDTO"/>.</returns>
    public static TestResultDTO ToDTO(this TestResult tr, IEnumerable<QuestionResultDTO> qrs) => new ()
    {
        Id = tr.TestResultId,
        TestId = tr.TestId,
        UserId = tr.UserId,
        CorrectAnswersPercent = (double)tr.CorrectAnswersPercent,
        Points = tr.Points,
        Date = tr.TestDate,
        Questions = qrs.ToList(),
    };
}
