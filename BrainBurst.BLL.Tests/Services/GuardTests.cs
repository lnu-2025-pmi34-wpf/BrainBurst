namespace BrainBurst.BLL.Tests
{
    using System;
    using Xunit;

    /// <summary>
    /// Містить юніт-тести для класу Guard, який виконує перевірку аргументів.
    /// </summary>
    public class GuardTests
    {
        // ============================================================
        // Email()
        // ============================================================

        /// <summary>
        /// Тест: Перевіряє, що валідна електронна адреса не викликає винятку.
        /// </summary>
        [Fact]
        public void Email_Valid_DoesNotThrow()
        {
            Guard.Email("test@example.com");
        }

        /// <summary>
        /// Тест: Перевіряє, що порожні або null значення електронної адреси викликають виняток.
        /// </summary>
        /// <param name="input">Вхідний рядок (null, empty, whitespace).</param>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Email_Empty_Throws(string? input)
        {
            Assert.Throws<ArgumentException>(() => Guard.Email(input!));
        }

        /// <summary>
        /// Тест: Перевіряє, що адреса без символу '@' викликає виняток.
        /// </summary>
        [Fact]
        public void Email_NoAtSymbol_Throws()
        {
            Assert.Throws<ArgumentException>(() => Guard.Email("invalid.email.com"));
        }

        // ============================================================
        // Password()
        // ============================================================

        /// <summary>
        /// Тест: Перевіряє, що валідний пароль (за замовчуванням: довжина >= 8) не викликає винятку.
        /// </summary>
        [Fact]
        public void Password_Valid_DoesNotThrow()
        {
            Guard.Password("valid123");
        }

        /// <summary>
        /// Тест: Перевіряє, що порожні або null значення пароля викликають виняток.
        /// </summary>
        /// <param name="input">Вхідний рядок (null, empty, whitespace).</param>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Password_Empty_Throws(string? input)
        {
            Assert.Throws<ArgumentException>(() => Guard.Password(input!));
        }

        /// <summary>
        /// Тест: Перевіряє, що пароль, коротший за мінімальну довжину, викликає виняток.
        /// </summary>
        [Fact]
        public void Password_TooShort_Throws()
        {
            Assert.Throws<ArgumentException>(() => Guard.Password("1234567"));
        }

        // ============================================================
        // Text()
        // ============================================================

        /// <summary>
        /// Тест: Перевіряє, що валідний текст не викликає винятку.
        /// </summary>
        [Fact]
        public void Text_Valid_DoesNotThrow()
        {
            Guard.Text("Hello", "Поле");
        }

        /// <summary>
        /// Тест: Перевіряє, що порожні або null значення тексту викликають виняток.
        /// </summary>
        /// <param name="input">Вхідний рядок (null, empty, whitespace).</param>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Text_Empty_Throws(string? input)
        {
            Assert.Throws<ArgumentException>(() => Guard.Text(input!, "Назва"));
        }

        /// <summary>
        /// Тест: Перевіряє, що текст, довший за максимальну довжину, викликає виняток.
        /// </summary>
        [Fact]
        public void Text_TooLong_Throws()
        {
            string veryLong = new string('A', 5000);
            Assert.Throws<ArgumentException>(() => Guard.Text(veryLong, "Поле", max: 100));
        }

        /// <summary>
        /// Тест: Перевіряє, що текст, який дорівнює максимальній довжині, не викликає винятку.
        /// </summary>
        [Fact]
        public void Text_MaxBoundary_DoesNotThrow()
        {
            string exact = new string('A', 10);
            Guard.Text(exact, "Поле", max: 10);
        }
    }
}
