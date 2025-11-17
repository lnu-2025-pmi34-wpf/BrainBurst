using BrainBurst.BLL.Enums;
using BrainBurst.BLL.Services;
using Xunit;

namespace BrainBurst.BLL.Tests.Services
{
    public class RankingServiceTests
    {
        private readonly RankingService _service = new();

        // GetRank: тестуємо всі діапазони + межі

        [Theory]
        // Newbie: < 100
        [InlineData(-1,   UserRank.Newbie)]
        [InlineData(0,    UserRank.Newbie)]
        [InlineData(1,    UserRank.Newbie)]
        [InlineData(99,   UserRank.Newbie)]

        // Enthusiast: 100–499
        [InlineData(100,  UserRank.Enthusiast)]
        [InlineData(101,  UserRank.Enthusiast)]
        [InlineData(499,  UserRank.Enthusiast)]

        // Specialist: 500–1999
        [InlineData(500,  UserRank.Specialist)]
        [InlineData(501,  UserRank.Specialist)]
        [InlineData(1999, UserRank.Specialist)]

        // Expert: 2000–9999
        [InlineData(2000, UserRank.Expert)]
        [InlineData(2001, UserRank.Expert)]
        [InlineData(9999, UserRank.Expert)]

        // Master: 10000–49999
        [InlineData(10000, UserRank.Master)]
        [InlineData(10001, UserRank.Master)]
        [InlineData(49999, UserRank.Master)]

        // Guru: 50000–99999
        [InlineData(50000, UserRank.Guru)]
        [InlineData(50001, UserRank.Guru)]
        [InlineData(99999, UserRank.Guru)]

        // Legend: >= 100000
        [InlineData(100000,      UserRank.Legend)]
        [InlineData(100001,      UserRank.Legend)]
        [InlineData(int.MaxValue,UserRank.Legend)]
        public void GetRank_ReturnsExpectedRank_ForGivenPoints(int points, UserRank expectedRank)
        {
            var actual = _service.GetRank(points);

            Assert.Equal(expectedRank, actual);
        }

        // GetRankLabel: перевіряємо мапінг кожного rank

        [Theory]
        [InlineData(UserRank.Legend,     "Легенда ✨")]
        [InlineData(UserRank.Guru,       "Гуру 🧙‍♂️")]
        [InlineData(UserRank.Master,     "Майстер 🏆")]
        [InlineData(UserRank.Expert,     "Експерт ⭐")]
        [InlineData(UserRank.Specialist, "Спеціаліст 🛠")]
        [InlineData(UserRank.Enthusiast, "Ентузіаст 👍")]
        [InlineData(UserRank.Newbie,     "Початківець 👶")]
        public void GetRankLabel_ReturnsCorrectLabel_ForKnownRanks(UserRank rank, string expectedLabel)
        {
            var label = _service.GetRankLabel(rank);

            Assert.Equal(expectedLabel, label);
        }

        [Fact]
        public void GetRankLabel_UnknownEnumValue_FallsBackToBeginnerLabel()
        {
            var unknownRank = (UserRank)999;

            var label = _service.GetRankLabel(unknownRank);

            Assert.Equal("Початківець 👶", label);
        }
    }
}