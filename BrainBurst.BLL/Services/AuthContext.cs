namespace BrainBurst.BLL.Services;

/// <summary>
/// Реалізація сервісу, що зберігає стан поточного автентифікованого користувача.
/// Цей клас зазвичай реєструється як Singleton.
/// </summary>
public class AuthContext : IAuthContext
{
    private UserDTO? _currentUser;

    /// <summary>
    /// Gets отримує ID поточного автентифікованого користувача.
    /// Повертає 0, якщо користувач не ввійшов у систему.
    /// </summary>
    public int CurrentUserId => this._currentUser?.Id ?? 0;

    /// <summary>
    /// Gets отримує повний DTO поточного користувача (може бути null, якщо ніхто не ввійшов).
    /// </summary>
    public UserDTO? CurrentUser => this._currentUser;

    /// <summary>
    /// Встановлює контекст поточного користувача після успішного входу або реєстрації.
    /// </summary>
    /// <param name="user">DTO користувача, який увійшов у систему.</param>
    public void SetCurrentUser(UserDTO user)
    {
        this._currentUser = user ?? throw new ArgumentNullException(nameof(user));
    }

    /// <summary>
    /// Очищає контекст автентифікації (використовується при виході з системи).
    /// </summary>
    public void ClearContext()
    {
        this._currentUser = null;
    }
}