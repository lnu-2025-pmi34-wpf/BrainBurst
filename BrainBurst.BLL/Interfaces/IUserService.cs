namespace BrainBurst.BLL.Interfaces;

using BrainBurst.BLL.DTO;

public interface IUserService
{
    Task<UserDTO> GetAsync(int id, CancellationToken ct);
    Task<UserDTO> UpdateProfileAsync(int id, string fullName, CancellationToken ct);
    Task<IReadOnlyList<RankingEntryDTO>> GetLeaderboardAsync(int top, CancellationToken ct);
}