public static class Guard
{
    public static void Email(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
            throw new ArgumentException("Некоректний email.");
    }

    public static void Password(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            throw new ArgumentException("Пароль має містити щонайменше 8 символів.");
    }

    public static void Text(string value, string field, int max = 4000)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException($"{field} порожнє.");
        if (value.Length > max) throw new ArgumentException($"{field} надто довге.");
    }
}
