namespace BrainBurst.BLL.Abstractions;

using BrainBurst.DAL.Entities;

public interface IUserRepository
{
    Task<User> GetByIdAsync(int id, CancellationToken ct);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct);
    Task<User> AddAsync(User user, CancellationToken ct);
    Task UpdateAsync(User user, CancellationToken ct);
    Task<IReadOnlyList<User>> GetTopAsync(int take, CancellationToken ct);
}

public interface IFlashcardRepository
{
    Task<Flashcard> AddAsync(Flashcard f, IEnumerable<string> tags, CancellationToken ct);
    Task UpdateAsync(Flashcard f, IEnumerable<string> tags, CancellationToken ct);
    Task DeleteAsync(int id, int ownerId, CancellationToken ct);
    Task<Flashcard?> GetAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<Flashcard>> FindAsync(int ownerId, string? search, CancellationToken ct);
}

public interface ITestRepository
{
    Task<Test> CreateFromFlashcardsAsync(int creatorId, IEnumerable<int> flashcardIds, CancellationToken ct);
    Task<Test?> GetAsync(int id, CancellationToken ct);
}

public interface ITestResultRepository
{
    Task<TestResult> AddAsync(TestResult tr, IEnumerable<QuestionResult> qr, CancellationToken ct);
    Task<IReadOnlyList<TestResult>> GetByUserAsync(int userId, CancellationToken ct);
}
