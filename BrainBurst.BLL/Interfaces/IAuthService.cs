namespace BrainBurst.BLL.Interfaces;

using BrainBurst.BLL.DTO;

public interface IAuthService
{
    Task<UserDTO> RegisterAsync(string email, string password, string fullName, CancellationToken ct);

    Task<UserDTO> LoginAsync(string email, string password, CancellationToken ct);
}