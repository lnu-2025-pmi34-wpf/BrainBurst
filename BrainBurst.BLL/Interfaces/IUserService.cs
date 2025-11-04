namespace BrainBurst.BLL.Interfaces;

using BrainBurst.BLL.DTO;

public interface IUserService
{
    Task DeleteAccountAsync(int userId, CancellationToken ct); // НОВИЙ МЕТОД
    Task<UserDTO> GetAsync(int id, CancellationToken ct);
    Task<UserDTO> UpdateProfileAsync(int id, string fullName, CancellationToken ct);
    Task<IReadOnlyList<RankingEntryDTO>> GetLeaderboardAsync(int top, CancellationToken ct);

    // НОВИЙ МЕТОД:
    Task ChangePasswordAsync(int userId, string oldPassword, string newPassword, CancellationToken ct);
}