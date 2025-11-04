namespace BrainBurst.BLL.Interfaces;

using BrainBurst.BLL.DTO;

public interface IAuthContext
{
    // ID поточного автентифікованого користувача
    int CurrentUserId { get; }
    
    // Повний DTO поточного користувача
    UserDTO? CurrentUser { get; }

    // Встановлює контекст після успішного входу/реєстрації
    void SetCurrentUser(UserDTO user);
    
    // Очищає контекст при виході з системи
    void ClearContext();
}