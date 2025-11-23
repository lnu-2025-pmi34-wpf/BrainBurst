namespace BrainBurst.BLL.Tests.Mapping
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using BrainBurst.BLL.DTO;
    using BrainBurst.BLL.Enums;
    using BrainBurst.BLL.Interfaces;
    using BrainBurst.BLL.Mapping;
    using BrainBurst.DAL.Entities;
    using Moq;
    using Xunit;

    /// <summary>
    /// Містить юніт-тести для методів-розширень мапінгу в <see cref="MappingExtensions"/>.
    /// </summary>
    public class MappingExtensionsTests
    {
        /// <summary>
        /// Тест: Метод ToDTO для User коректно мапить всі поля та викликає сервіс рейтингу.
        /// </summary>
        [Fact]
        public void User_ToDTO_MapsAllFields_AndCallsRatingServiceCorrectly()
        {
            var user = new User
            {
                UserId = 10,
                Email = "user@example.com",
                FullName = "Test User",
                Points = 1234,
                CreatedAt = new DateTime(2025, 1, 2, 3, 4, 5, DateTimeKind.Utc),
            };

            var ratingMock = new Mock<IRatingService>(MockBehavior.Strict);

            ratingMock
                .Setup(r => r.GetRank(user.Points))
                .Returns(UserRank.Expert);

            ratingMock
                .Setup(r => r.GetRankLabel(UserRank.Expert))
                .Returns("Експерт ⭐");

            var dto = user.ToDTO(ratingMock.Object);

            Assert.Equal(user.UserId, dto.Id);
            Assert.Equal(user.Email, dto.Email);
            Assert.Equal(user.FullName, dto.FullName);
            Assert.Equal(user.Points, dto.Points);
            Assert.Equal(UserRank.Expert, dto.Rank);
            Assert.Equal("Експерт ⭐", dto.RankLabel);
            Assert.Equal(user.CreatedAt, dto.CreatedAt);

            ratingMock.Verify(r => r.GetRank(user.Points), Times.Exactly(2));
            ratingMock.Verify(r => r.GetRankLabel(UserRank.Expert), Times.Once);
        }

        /// <summary>
        /// Тест: Метод ToDTO для User коректно обробляє null значення для Email та FullName.
        /// </summary>
        [Fact]
        public void User_ToDTO_UsesEmptyString_WhenEmailOrFullNameNull()
        {
            var user = new User
            {
                UserId = 1,
                Email = null!,
                FullName = null,
                Points = 0,
                CreatedAt = DateTime.UtcNow,
            };

            var ratingMock = new Mock<IRatingService>(MockBehavior.Loose);
            ratingMock.Setup(r => r.GetRank(It.IsAny<int>())).Returns(UserRank.Newbie);
            ratingMock.Setup(r => r.GetRankLabel(It.IsAny<UserRank>())).Returns("Початківець 👶");

            var dto = user.ToDTO(ratingMock.Object);

            Assert.Equal(string.Empty, dto.Email);
            Assert.Equal(string.Empty, dto.FullName);
        }

        /// <summary>
        /// Тест: Метод ToDTO для Flashcard мапить поля та створює нову копію списку тегів.
        /// </summary>
        [Fact]
        public void Flashcard_ToDTO_MapsFields_AndCopiesTagsToNewList()
        {
            var card = new Flashcard
            {
                FlashcardId = 5,
                Question = "Q?",
                Answer = "A!",
                CreatorId = 7,
                CreatedAt = new DateTime(2024, 5, 6, 7, 8, 9, DateTimeKind.Utc),
            };

            var tags = new List<string> { "tag1", "tag2" };

            var dto = card.ToDTO(tags);

            Assert.Equal(card.FlashcardId, dto.Id);
            Assert.Equal(card.Question, dto.Question);
            Assert.Equal(card.Answer, dto.Answer);
            Assert.Equal(card.CreatorId, dto.CreatorId);
            Assert.Equal(card.CreatedAt, dto.CreatedAt);

            Assert.Equal(tags.Count, dto.Tags.Count);
            Assert.Equal(tags, dto.Tags);

            Assert.False(ReferenceEquals(tags, dto.Tags));

            tags.Add("tag3");
            Assert.Equal(2, dto.Tags.Count);
        }

        /// <summary>
        /// Тест: Метод ToDTO для Flashcard коректно обробляє null для Question, Answer та списку тегів.
        /// </summary>
        [Fact]
        public void Flashcard_ToDTO_HandlesNullsAndNullTags()
        {
            var card = new Flashcard
            {
                FlashcardId = 2,
                Question = null!,
                Answer = null!,
                CreatorId = 1,
                CreatedAt = DateTime.UtcNow,
            };

            var dto = card.ToDTO(null!);

            Assert.Equal(string.Empty, dto.Question);
            Assert.Equal(string.Empty, dto.Answer);
            Assert.NotNull(dto.Tags);
            Assert.Empty(dto.Tags);
        }

        /// <summary>
        /// Тест: Метод ToTestDTO повертає DTO з порожнім списком питань, якщо на вході порожня колекція карток.
        /// </summary>
        [Fact]
        public void ToTestDTO_EmptyCards_ReturnsTestDtoWithEmptyQuestions()
        {
            var cards = new List<Flashcard>();
            int testId = 100;
            int creatorId = 200;

            var dto = cards.ToTestDTO(testId, creatorId);

            Assert.Equal(testId, dto.Id);
            Assert.Equal(creatorId, dto.CreatorId);
            Assert.NotNull(dto.Questions);
            Assert.Empty(dto.Questions);
        }

        /// <summary>
        /// Тест: Метод ToTestDTO коректно мапить кожну картку на FlashcardDTO з порожніми тегами.
        /// </summary>
        [Fact]
        public void ToTestDTO_MapsEachCardToQuestionDto_WithEmptyTags()
        {
            var now = DateTime.UtcNow;
            var cards = new List<Flashcard>
            {
                new Flashcard { FlashcardId = 1, Question = "Q1", Answer = "A1", CreatorId = 10, CreatedAt = now.AddMinutes(-1) },
                new Flashcard { FlashcardId = 2, Question = null !,  Answer = null !,  CreatorId = 10, CreatedAt = now },
            };

            int testId = 5;
            int creatorId = 10;

            var dto = cards.ToTestDTO(testId, creatorId);

            Assert.Equal(testId, dto.Id);
            Assert.Equal(creatorId, dto.CreatorId);
            Assert.Equal(2, dto.Questions.Count);

            var q1 = dto.Questions[0];
            Assert.Equal(1, q1.Id);
            Assert.Equal("Q1", q1.Question);
            Assert.Equal("A1", q1.Answer);
            Assert.Equal(10, q1.CreatorId);
            Assert.Equal(now.AddMinutes(-1), q1.CreatedAt);
            Assert.NotNull(q1.Tags);
            Assert.Empty(q1.Tags);

            var q2 = dto.Questions[1];
            Assert.Equal(2, q2.Id);
            Assert.Equal(string.Empty, q2.Question);
            Assert.Equal(string.Empty, q2.Answer);
            Assert.Equal(10, q2.CreatorId);
            Assert.Equal(now, q2.CreatedAt);
            Assert.NotNull(q2.Tags);
            Assert.Empty(q2.Tags);
        }

        /// <summary>
        /// Тест: Метод ToDTO для QuestionResult мапить поля та замінює null UserInput на порожній рядок.
        /// </summary>
        [Fact]
        public void QuestionResult_ToDTO_MapsFields_AndReplacesNullUserInput()
        {
            var qr = new QuestionResult
            {
                QuestionResultId = 7,
                FlashcardId = 3,
                UserInput = null!,
                IsCorrect = true,
            };

            var dto = qr.ToDTO();

            Assert.Equal(7, dto.Id);
            Assert.Equal(3, dto.FlashcardId);
            Assert.Equal(string.Empty, dto.UserInput);
            Assert.True(dto.IsCorrect);
        }

        /// <summary>
        /// Тест: Метод ToDTO для QuestionResult коректно мапить не-null UserInput.
        /// </summary>
        [Fact]
        public void QuestionResult_ToDTO_MapsNonNullUserInput()
        {
            var qr = new QuestionResult
            {
                QuestionResultId = 8,
                FlashcardId = 4,
                UserInput = " user answer ",
                IsCorrect = false,
            };

            var dto = qr.ToDTO();

            Assert.Equal(8, dto.Id);
            Assert.Equal(4, dto.FlashcardId);
            Assert.Equal(" user answer ", dto.UserInput);
            Assert.False(dto.IsCorrect);
        }

        /// <summary>
        /// Тест: Метод ToDTO для TestResult мапить скалярні поля та копіює список QuestionResultDTO.
        /// </summary>
        [Fact]
        public void TestResult_ToDTO_MapsScalarFields_AndCopiesQuestionsList()
        {
            var tr = new TestResult
            {
                TestResultId = 50,
                TestId = 10,
                UserId = 3,
                CorrectAnswersPercent = 87.5m,
                Points = 40,
                TestDate = new DateTime(2025, 2, 3, 4, 5, 6, DateTimeKind.Utc),
            };

            var qrs = new List<QuestionResultDTO>
            {
                new QuestionResultDTO { Id = 1, FlashcardId = 10, UserInput = "A", IsCorrect = true },
                new QuestionResultDTO { Id = 2, FlashcardId = 20, UserInput = "B", IsCorrect = false },
            };

            var dto = tr.ToDTO(qrs);

            Assert.Equal(tr.TestResultId, dto.Id);
            Assert.Equal(tr.TestId, dto.TestId);
            Assert.Equal(tr.UserId, dto.UserId);
            Assert.Equal((double)tr.CorrectAnswersPercent, dto.CorrectAnswersPercent, 10);
            Assert.Equal(tr.Points, dto.Points);
            Assert.Equal(tr.TestDate, dto.Date);

            Assert.NotNull(dto.Questions);
            Assert.Equal(2, dto.Questions.Count);
            Assert.Equal(qrs[0].Id, dto.Questions[0].Id);
            Assert.Equal(qrs[1].Id, dto.Questions[1].Id);

            Assert.False(ReferenceEquals(qrs, dto.Questions));
        }

        /// <summary>
        /// Тест: Метод ToDTO для TestResult повертає DTO з порожнім списком питань, якщо на вході порожня колекція.
        /// </summary>
        [Fact]
        public void TestResult_ToDTO_EmptyQuestionsEnumerable_ReturnsDtoWithEmptyList()
        {
            var tr = new TestResult
            {
                TestResultId = 1,
                TestId = 2,
                UserId = 3,
                CorrectAnswersPercent = 0m,
                Points = 0,
                TestDate = DateTime.UtcNow,
            };

            IEnumerable<QuestionResultDTO> emptyQrs = Array.Empty<QuestionResultDTO>();

            var dto = tr.ToDTO(emptyQrs);

            Assert.NotNull(dto.Questions);
            Assert.Empty(dto.Questions);
        }
    }
}
