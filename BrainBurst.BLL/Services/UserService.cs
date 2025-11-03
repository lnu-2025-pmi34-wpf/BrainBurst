using BrainBurst.BLL.DTO;
using BrainBurst.BLL.Enums;
using BrainBurst.DAL.Entities;

namespace BrainBurst.BLL.Services;

public class UserService
{
    public UserDTO MapToDTO(User user)
    {
        var rankService = new RankingService();

        return new UserDTO
        {
            Id = user.UserId,
            Email = user.Email,
            Points = user.Points,
            Rank = rankService.GetRank(user.Points)
        };
    }
}