namespace BrainBurst.DAL.Abstractions; // << НОВИЙ NAMESPACE
using BrainBurst.DAL.Entities;

/// <summary>
/// Визначає операції для роботи з сутностями (користувачами) <see cref="User"/>.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Видаляє користувача за його ID.
    /// </summary>
    /// <param name="id">Ідентифікатор користувача для видалення.</param>
    /// <param name="ct">Токен скасування операції.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task DeleteAsync(int id, CancellationToken ct);

    /// <summary>
    /// Отримує користувача за його ID.
    /// </summary>
    /// <param name="id">Ідентифікатор користувача.</param>
    /// <param name="ct">Токен скасування операції.</param>
    /// <returns>Знайдений <see cref="User"/> або null.</returns>
    Task<User> GetByIdAsync(int id, CancellationToken ct);

    /// <summary>
    /// Отримує користувача за його email.
    /// </summary>
    /// <param name="email">Email користувача.</param>
    /// <param name="ct">Токен скасування операції.</param>
    /// <returns>Знайдений <see cref="User"/> або null, якщо не знайдено.</returns>
    Task<User?> GetByEmailAsync(string email, CancellationToken ct);

    /// <summary>
    /// Додає нового користувача до бази даних.
    /// </summary>
    /// <param name="user">Сутність користувача для додавання.</param>
    /// <param name="ct">Токен скасування операції.</param>
    /// <returns>Додана сутність <see cref="User"/> (може містити оновлений ID).</returns>
    Task<User> AddAsync(User user, CancellationToken ct);

    /// <summary>
    /// Оновлює існуючого користувача.
    /// </summary>
    /// <param name="user">Сутність користувача з оновленими даними.</param>
    /// <param name="ct">Токен скасування операції.</param>
    /// <returns>A <see cref="Task"/>, що представляє асинхронну операцію.</returns>
    Task UpdateAsync(User user, CancellationToken ct);

    /// <summary>
    /// Отримує список найкращих (Top N) користувачів, наприклад, за рейтингом.
    /// </summary>
    /// <param name="take">Кількість користувачів, яку потрібно отримати.</param>
    /// <param name="ct">Токен скасування операції.</param>
    /// <returns>Список <see cref="User"/>, доступний лише для читання.</returns>
    Task<IReadOnlyList<User>> GetTopAsync(int take, CancellationToken ct);
}

/// <summary>
/// Визначає операції для роботи з флеш-картками <see cref="Flashcard"/>.
/// </summary>
public interface IFlashcardRepository
{
    /// <summary>
    /// Додає нову флеш-картку та пов'язує її з тегами.
    /// </summary>
    /// <param name="f">Сутність флеш-картки для додавання.</param>
    /// <param name="tags">Список рядків-тегів для асоціації.</param>
    /// <param name="ct">Токен скасування операції.</param>
    /// <returns>Створена сутність <see cref="Flashcard"/> з оновленим ID.</returns>
    Task<Flashcard> AddAsync(Flashcard f, IEnumerable<string> tags, CancellationToken ct);

    /// <summary>
    /// Оновлює існуючу флеш-картку та її набір тегів.
    /// </summary>
    /// <param name="f">Сутність флеш-картки з оновленими даними.</param>
    /// <param name="tags">Новий повний список тегів для цієї картки.</param>
    /// <param name="ct">Токен скасування операції.</param>
    /// <returns>A <see cref="Task"/>, що представляє асинхронну операцію.</returns>
    Task UpdateAsync(Flashcard f, IEnumerable<string> tags, CancellationToken ct);

    /// <summary>
    /// Видаляє флеш-картку за її ID, перевіряючи право власності.
    /// </summary>
    /// <param name="id">ID флеш-картки для видалення.</param>
    /// <param name="ownerId">ID власника (для перевірки безпеки).</param>
    /// <param name="ct">Токен скасування операції.</param>
    /// <returns>A <see cref="Task"/>, що представляє асинхронну операцію.</returns>
    Task DeleteAsync(int id, int ownerId, CancellationToken ct);

    /// <summary>
    /// Отримує одну флеш-картку за її ID.
    /// </summary>
    /// <param name="id">ID флеш-картки.</param>
    /// <param name="ct">Токен скасування операції.</param>
    /// <returns>Знайдена <see cref="Flashcard"/> або null.</returns>
    Task<Flashcard?> GetAsync(int id, CancellationToken ct);

    /// <summary>
    /// Знаходить флеш-картки за ID власника та (опційно) пошуковим рядком.
    /// </summary>
    /// <param name="ownerId">ID власника карток.</param>
    /// <param name="search">Пошуковий рядок (може бути null або порожнім).</param>
    /// <param name="ct">Токен скасування операції.</param>
    /// <returns>Список <see cref="Flashcard"/>, доступний лише для читання.</returns>
    Task<IReadOnlyList<Flashcard>> FindAsync(int ownerId, string? search, CancellationToken ct);
}

/// <summary>
/// Визначає операції для роботи з тестами <see cref="Test"/>.
/// </summary>
public interface ITestRepository
{
    /// <summary>
    /// Створює новий тест на основі наданого списку ID флеш-карток.
    /// </summary>
    /// <param name="creatorId">ID користувача, який створює тест.</param>
    /// <param name="flashcardIds">Список ID флеш-карток для включення у тест.</param>
    /// <param name="ct">Токен скасування операції.</param>
    /// <returns>Створений <see cref="Test"/>.</returns>
    Task<Test> CreateFromFlashcardsAsync(int creatorId, IEnumerable<int> flashcardIds, CancellationToken ct);

    /// <summary>
    /// Отримує один тест за його ID.
    /// </summary>
    /// <param name="id">ID тесту.</param>
    /// <param name="ct">Токен скасування операції.</param>
    /// <returns>Знайдений <see cref="Test"/> або null.</returns>
    Task<Test?> GetAsync(int id, CancellationToken ct);
}

/// <summary>
/// Визначає операції для роботи з результатами тестів <see cref="TestResult"/>.
/// </summary>
public interface ITestResultRepository
{
    /// <summary>
    /// Додає новий результат тесту разом з відповідями на питання.
    /// </summary>
    /// <param name="tr">Основна сутність результату тесту.</param>
    /// <param name="qr">Список результатів відповідей на питання.</param>
    /// <param name="ct">Токен скасування операції.</param>
    /// <returns>Створений <see cref="TestResult"/>.</returns>
    Task<TestResult> AddAsync(TestResult tr, IEnumerable<QuestionResult> qr, CancellationToken ct);

    /// <summary>
    /// Отримує всі результати тестів для конкретного користувача.
    /// </summary>
    /// <param name="userId">ID користувача, чиї результати потрібно знайти.</param>
    /// <param name="ct">Токен скасування операції.</param>
    /// <returns>Список <see cref="TestResult"/>, доступний лише для читання.</returns>
    Task<IReadOnlyList<TestResult>> GetByUserAsync(int userId, CancellationToken ct);
}