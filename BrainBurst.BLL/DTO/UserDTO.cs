namespace BrainBurst.BLL.DTO;

using BrainBurst.BLL.Enums;

public sealed class UserDTO
{
    public int Id { get; init; }
    public string Email { get; init; } = "";
  public string FullName { get; set; } = ""; // <-- Змінено init на set
    public int Points { get; init; }
    public UserRank Rank { get; set; }
    public string? RankLabel { get; set; }
    public DateTime CreatedAt { get; init; }
}