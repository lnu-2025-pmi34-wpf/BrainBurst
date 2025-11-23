namespace BrainBurst.BLL.Tests.Services
{
    using BrainBurst.BLL.Enums;
    using BrainBurst.BLL.Services;
    using Xunit;

    /// <summary>
    /// Містить юніт-тести для <see cref="RankingService"/>.
    /// </summary>
    public class RankingServiceTests
    {
        private readonly RankingService _service = new ();

        /// <summary>
        /// Тест: GetRank коректно повертає очікуваний ранг для різних значень балів.
        /// </summary>
        /// <param name="points">Вхідна кількість балів.</param>
        /// <param name="expectedRank">Очікуваний ранг.</param>
        [Theory]
        [InlineData(-1, UserRank.Newbie)]
        [InlineData(0, UserRank.Newbie)]
        [InlineData(1, UserRank.Newbie)]
        [InlineData(99, UserRank.Newbie)]

        [InlineData(100, UserRank.Enthusiast)]
        [InlineData(101, UserRank.Enthusiast)]
        [InlineData(499, UserRank.Enthusiast)]

        [InlineData(500, UserRank.Specialist)]
        [InlineData(501, UserRank.Specialist)]
        [InlineData(1999, UserRank.Specialist)]

        [InlineData(2000, UserRank.Expert)]
        [InlineData(2001, UserRank.Expert)]
        [InlineData(9999, UserRank.Expert)]

        [InlineData(10000, UserRank.Master)]
        [InlineData(10001, UserRank.Master)]
        [InlineData(49999, UserRank.Master)]

        [InlineData(50000, UserRank.Guru)]
        [InlineData(50001, UserRank.Guru)]
        [InlineData(99999, UserRank.Guru)]

        [InlineData(100000, UserRank.Legend)]
        [InlineData(100001, UserRank.Legend)]
        [InlineData(int.MaxValue, UserRank.Legend)]
        public void GetRank_ReturnsExpectedRank_ForGivenPoints(int points, UserRank expectedRank)
        {
            var actual = this._service.GetRank(points);

            Assert.Equal(expectedRank, actual);
        }

        /// <summary>
        /// Тест: GetRankLabel повертає коректну текстову мітку (з емодзі) для кожного відомого рангу.
        /// </summary>
        /// <param name="rank">Вхідний ранг.</param>
        /// <param name="expectedLabel">Очікувана мітка.</param>
        [Theory]
        [InlineData(UserRank.Legend, "Легенда ✨")]
        [InlineData(UserRank.Guru, "Гуру 🧙‍♂️")]
        [InlineData(UserRank.Master, "Майстер 🏆")]
        [InlineData(UserRank.Expert, "Експерт ⭐")]
        [InlineData(UserRank.Specialist, "Спеціаліст 🛠")]
        [InlineData(UserRank.Enthusiast, "Ентузіаст 👍")]
        [InlineData(UserRank.Newbie, "Початківець 👶")]
        public void GetRankLabel_ReturnsCorrectLabel_ForKnownRanks(UserRank rank, string expectedLabel)
        {
            var label = this._service.GetRankLabel(rank);

            Assert.Equal(expectedLabel, label);
        }

        /// <summary>
        /// Тест: GetRankLabel повертає мітку "Початківець" для будь-якого невідомого (невизначеного) значення enum.
        /// </summary>
        [Fact]
        public void GetRankLabel_UnknownEnumValue_FallsBackToBeginnerLabel()
        {
            var unknownRank = (UserRank)999;

            var label = this._service.GetRankLabel(unknownRank);

            Assert.Equal("Початківець 👶", label);
        }
    }
}