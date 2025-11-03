using BrainBurst.BLL.Enums;
using BrainBurst.BLL.Interfaces;

namespace BrainBurst.BLL.Services;

public class RankingService : IRatingService
{
    public UserRank GetRank(int points) => points switch
    {
        >= 100000 => UserRank.Legend,
        >= 50000  => UserRank.Guru,
        >= 10000  => UserRank.Master,
        >= 2000   => UserRank.Expert,
        >= 500    => UserRank.Specialist,
        >= 100    => UserRank.Enthusiast,
        _         => UserRank.Newbie
    };

    public string GetRankLabel(UserRank rank) => rank switch
    {
        UserRank.Legend      => "Легенда ✨",
        UserRank.Guru        => "Гуру 🧙‍♂️",
        UserRank.Master      => "Майстер 🏆",
        UserRank.Expert      => "Експерт ⭐",
        UserRank.Specialist  => "Спеціаліст 🛠",
        UserRank.Enthusiast  => "Ентузіаст 👍",
        _                    => "Початківець 👶"
    };
}
