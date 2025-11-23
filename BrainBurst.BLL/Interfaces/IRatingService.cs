namespace BrainBurst.BLL.Interfaces;

using BrainBurst.BLL.DTO;
using BrainBurst.BLL.Enums;

/// <summary>
/// Визначає контракт для сервісу, що відповідає за логіку рангів та балів.
/// </summary>
public interface IRatingService
{
    /// <summary>
    /// Визначає ранг користувача на основі його поточної кількості балів.
    /// </summary>
    /// <param name="points">Кількість балів користувача.</param>
    /// <returns>Ранг <see cref="UserRank"/>, що відповідає балам.</returns>
    UserRank GetRank(int points);

    /// <summary>
    /// Отримує текстову мітку або емодзі для.
    /// </summary>
    /// <param name="rank">Ранг користувача.</param>
    /// <returns>Рядок, що представляє ранг (наприклад, "Початківець" або "🏆").</returns>
    string GetRankLabel(UserRank rank); // опційно для підпису/емодзі
}
