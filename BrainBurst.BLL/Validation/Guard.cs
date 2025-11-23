#pragma warning disable SA1514
/// <summary>
/// Статичний клас-помічник ("охоронець") для валідації вхідних аргументів.
/// </summary>
public static class Guard
{
    /// <summary>
    /// Перевіряє, чи є рядок коректною адресою email (базова перевірка).
    /// </summary>
    /// <param name="email">Рядок для перевірки.</param>
    /// <exception cref="ArgumentException">Виникає, якщо email порожній або не містить "@".</exception>
    public static void Email(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
        {
            throw new ArgumentException("Некоректний email.");
        }
    }

    /// <summary>
    /// Перевіряє, чи відповідає пароль мінімальним вимогам безпеки.
    /// </summary>
    /// <param name="password">Пароль для перевірки.</param>
    /// <exception cref="ArgumentException">Виникає, якщо пароль порожній або коротший за 8 символів.</exception>
    public static void Password(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
        {
            throw new ArgumentException("Пароль має містити щонайменше 8 символів.");
        }
    }

    /// <summary>
    /// Перевіряє, чи текстове значення не є порожнім та не перевищує максимальну довжину.
    /// </summary>
    /// <param name="value">Текстове значення для перевірки.</param>
    /// <param name="field">Назва поля (для повідомлення про помилку).</param>
    /// <param name="max">Максимально дозволена довжина.</param>
    /// <exception cref="ArgumentException">Виникає, якщо текст порожній або довший за `max`.</exception>
    public static void Text(string value, string field, int max = 4000)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{field} порожнє.");
        }

        if (value.Length > max)
        {
            throw new ArgumentException($"{field} надто довге.");
        }
    }
}
