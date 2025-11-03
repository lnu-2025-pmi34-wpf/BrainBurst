namespace BrainBurst.BLL.DTO;

public sealed class RankingEntryDTO
{
    public int UserId { get; init; }
    public string FullName { get; init; } = "";
    public int Points { get; init; }
    public string Rank { get; init; } = "";
}