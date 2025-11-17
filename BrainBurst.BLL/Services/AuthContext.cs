using System;
using BrainBurst.BLL.DTO;
using BrainBurst.BLL.Interfaces;

namespace BrainBurst.BLL.Services;

// Цей клас буде зареєстрований як Singleton (один на додаток)
public class AuthContext : IAuthContext
{
    private UserDTO? _currentUser;

    public int CurrentUserId => this._currentUser?.Id ?? 0;

    public UserDTO? CurrentUser => this._currentUser;

    public void SetCurrentUser(UserDTO user)
    {
        this._currentUser = user ?? throw new ArgumentNullException(nameof(user));
    }

    public void ClearContext()
    {
        this._currentUser = null;
    }
}