namespace BrainBurst.BLL.Interfaces;

using BrainBurst.BLL.DTO;

/// <summary>
/// Визначає контракт для сервісу, що зберігає стан поточного автентифікованого користувача.
/// </summary>
public interface IAuthContext
{
    /// <summary>
    /// Gets отримує ID поточного автентифікованого користувача.
    /// </summary>
    int CurrentUserId { get; }

    /// <summary>
    /// Gets отримує повний DTO поточного користувача (може бути null, якщо ніхто не ввійшов).
    /// </summary>
    UserDTO? CurrentUser { get; }

    /// <summary>
    /// Встановлює контекст поточного користувача після успішного входу або реєстрації.
    /// </summary>
    /// <param name="user">DTO користувача, який увійшов у систему.</param>
    void SetCurrentUser(UserDTO user);

    /// <summary>
    /// Очищає контекст автентифікації (використовується при виході з системи).
    /// </summary>
    void ClearContext();
}