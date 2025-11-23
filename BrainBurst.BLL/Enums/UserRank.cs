namespace BrainBurst.BLL.Enums;

/// <summary>
/// Визначає ранги, яких може досягти користувач на основі зароблених балів.
/// </summary>
public enum UserRank
{
    /// <summary>
    /// Початковий ранг для нових користувачів (0+ балів).
    /// </summary>
    Newbie,      // 0 points

    /// <summary>
    /// Ранг "Ентузіаст" (100+ балів).
    /// </summary>
    Enthusiast,    // 100 points

    /// <summary>
    /// Ранг "Спеціаліст" (500+ балів).
    /// </summary>
    Specialist,    // 500 points

    /// <summary>
    /// Ранг "Експерт" (2000+ балів).
    /// </summary>
    Expert,        // 2000 points

    /// <summary>
    /// Ранг "Майстер" (10000+ балів).
    /// </summary>
    Master,        // 10000 points

    /// <summary>
    /// Ранг "Гуру" (50000+ балів).
    /// </summary>
    Guru,          // 50000 points

    /// <summary>
    /// Найвищий ранг "Легенда" (100000+ балів).
    /// </summary>
    Legend, // 100000+ points
}
