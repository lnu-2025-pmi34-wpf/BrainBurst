namespace BrainBurst.BLL.Interfaces;

using BrainBurst.BLL.DTO;
using BrainBurst.BLL.Enums;

public interface IRatingService
{
    UserRank GetRank(int points);
    string GetRankLabel(UserRank rank); // опційно для підпису/емодзі
}
