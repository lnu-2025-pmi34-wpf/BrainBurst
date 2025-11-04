using BrainBurst.BLL.DTO;
using BrainBurst.BLL.Interfaces;
using System;

namespace BrainBurst.BLL.Services;

// Цей клас буде зареєстрований як Singleton (один на додаток)
public class AuthContext : IAuthContext
{
    private UserDTO? _currentUser;

    public int CurrentUserId => _currentUser?.Id ?? 0;

    public UserDTO? CurrentUser => _currentUser;

    public void SetCurrentUser(UserDTO user)
    {
        _currentUser = user ?? throw new ArgumentNullException(nameof(user));
    }

    public void ClearContext()
    {
        _currentUser = null;
    }
}