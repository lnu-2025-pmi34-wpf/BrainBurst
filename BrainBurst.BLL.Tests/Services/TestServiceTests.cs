using Moq;
using Xunit;

using BrainBurst.BLL.Services;
using BrainBurst.DAL.Abstractions;
using BrainBurst.BLL.Interfaces;
using BrainBurst.DAL.Entities;
using BrainBurst.BLL.Enums;

public class TestServiceTests
{
    [Fact]
    public async Task Submit_Saves_Result_And_Computes_Points()
    {
        // репозиторії
        var testsRepo    = new Mock<ITestRepository>();
        var resultsRepo  = new Mock<ITestResultRepository>();
        var usersRepo    = new Mock<IUserRepository>();
        var cardsRepo    = new Mock<IFlashcardRepository>();
        var rating       = new Mock<IRatingService>();

        // дані
        var test = new Test
        {
            TestId = 1,
            CreatorId = 7
        };

        testsRepo.Setup(r => r.GetAsync(1, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(test);

        // картки автора тесту
        cardsRepo.Setup(r => r.FindAsync(7, null, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<Flashcard>
                 {
                     new() { FlashcardId = 10, Question = "Capital of Ukraine?", Answer = "Kyiv", CreatorId = 7 },
                     new() { FlashcardId = 11, Question = "Capital of France?",  Answer = "Paris", CreatorId = 7 },
                 });

        // користувач
        usersRepo.Setup(r => r.GetByIdAsync(123, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new User { UserId = 123, Email = "x@y.z", Points = 0 });

        // збереження результату — повертаємо те, що прийшло
        resultsRepo.Setup(r => r.AddAsync(It.IsAny<TestResult>(),
                                          It.IsAny<IEnumerable<QuestionResult>>(),
                                          It.IsAny<CancellationToken>()))
                   .ReturnsAsync((TestResult tr, IEnumerable<QuestionResult> _, CancellationToken __) => tr);

        rating.Setup(r => r.GetRank(It.IsAny<int>())).Returns(UserRank.Newbie);
        rating.Setup(r => r.GetRankLabel(UserRank.Newbie)).Returns("Початківець 👶");


        // SUT
        var svc = new TestService(testsRepo.Object, resultsRepo.Object, usersRepo.Object, cardsRepo.Object, rating.Object);

        // act: 1 правильна з 2
        var dto = await svc.SubmitAsync(
            testId: 1,
            userId: 123,
            answers: new List<(int flashcardId, string userInput)>
            {
                (10, "Kyiv"),   // вірно
                (11, "Lyon")    // невірно
            },
            ct: default
        );

        // assert
        Assert.Equal(1, dto.TestId);
        Assert.Equal(123, dto.UserId);
        Assert.InRange(dto.CorrectAnswersPercent, 49.9, 50.1);
        Assert.Equal(10, dto.Points); // 1 правильна * 10, без бонусу 100%

        resultsRepo.Verify(r => r.AddAsync(It.IsAny<TestResult>(),
                                           It.IsAny<IEnumerable<QuestionResult>>(),
                                           It.IsAny<CancellationToken>()),
                           Times.Once);
        usersRepo.Verify(r => r.UpdateAsync(It.Is<User>(u => u.Points == 10), It.IsAny<CancellationToken>()),
                         Times.Once);
    }
}