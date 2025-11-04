namespace BrainBurst.BLL.Interfaces;

using BrainBurst.BLL.DTO;

public interface IArchiveService
{
    Task<IReadOnlyList<ArchiveEntryDTO>> GetArchiveAsync(int userId, CancellationToken ct);
}