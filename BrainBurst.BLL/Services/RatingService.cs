#pragma warning disable SA1200
using BrainBurst.BLL.Enums;
#pragma warning restore SA1200

namespace BrainBurst.BLL.Services;

/// <summary>
/// Реалізація сервісу, що відповідає за логіку рангів та балів.
/// </summary>
public class RankingService : IRatingService
{
    /// <summary>
    /// Визначає ранг користувача на основі його поточної кількості балів.
    /// </summary>
    /// <param name="points">Кількість балів користувача.</param>
    /// <returns>Ранг <see cref="UserRank"/>, що відповідає балам.</returns>
    public UserRank GetRank(int points) => points switch
    {
        >= 100000 => UserRank.Legend,
        >= 50000 => UserRank.Guru,
        >= 10000 => UserRank.Master,
        >= 2000 => UserRank.Expert,
        >= 500 => UserRank.Specialist,
        >= 100 => UserRank.Enthusiast,
        _ => UserRank.Newbie
    };

    /// <summary>
    /// Отримує текстову мітку та емодзі для вказаного рангу.
    /// </summary>
    /// <param name="rank">Ранг користувача.</param>
    /// <returns>Рядок, що представляє ранг (наприклад, "Початківець 👶").</returns>
    public string GetRankLabel(UserRank rank) => rank switch
    {
        UserRank.Legend => "Легенда ✨",
        UserRank.Guru => "Гуру 🧙‍♂️",
        UserRank.Master => "Майстер 🏆",
        UserRank.Expert => "Експерт ⭐",
        UserRank.Specialist => "Спеціаліст 🛠",
        UserRank.Enthusiast => "Ентузіаст 👍",
        _ => "Початківець 👶"
    };
}
