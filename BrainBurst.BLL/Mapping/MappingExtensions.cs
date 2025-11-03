namespace BrainBurst.BLL.Mapping;

using System;
using System.Linq;
using System.Collections.Generic;
using BrainBurst.BLL.DTO;
using BrainBurst.BLL.Interfaces;
using BrainBurst.DAL.Entities;

public static class MappingExtensions
{
    public static UserDTO ToDTO(this User u, IRatingService rating) => new()
    {
        Id = u.UserId,
        Email = u.Email ?? string.Empty,
        FullName = u.FullName ?? string.Empty,
        Points = u.Points,
        Rank = rating.GetRank(u.Points),
        RankLabel = rating.GetRankLabel(rating.GetRank(u.Points)),
        CreatedAt = u.CreatedAt
    };

    public static FlashcardDTO ToDTO(this Flashcard f, IEnumerable<string> tags) => new()
    {
        Id = f.FlashcardId,
        Question = f.Question ?? string.Empty,
        Answer = f.Answer ?? string.Empty,
        CreatorId = f.CreatorId,
        CreatedAt = f.CreatedAt,
        Tags = tags?.ToList() ?? new List<string>()
    };

    public static TestDTO ToTestDTO(this IEnumerable<Flashcard> cards, int testId, int creatorId) =>
        new()
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
                Tags = Array.Empty<string>()
            }).ToList()
        };

    public static QuestionResultDTO ToDTO(this QuestionResult q) => new()
    {
        Id = q.QuestionResultId,
        FlashcardId = q.FlashcardId,
        UserInput = q.UserInput ?? string.Empty,
        IsCorrect = q.IsCorrect
    };

    public static TestResultDTO ToDTO(this TestResult tr, IEnumerable<QuestionResultDTO> qrs) => new()
    {
        Id = tr.TestResultId,
        TestId = tr.TestId,
        UserId = tr.UserId,
        CorrectAnswersPercent = (double)tr.CorrectAnswersPercent,
        Points = tr.Points,
        Date = tr.TestDate,
        Questions = qrs.ToList()
    };
}
