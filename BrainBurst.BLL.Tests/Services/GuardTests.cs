using System;
using Xunit;

namespace BrainBurst.BLL.Tests
{
    public class GuardTests
    {
        // ============================================================
        // Email()
        // ============================================================

        [Fact]
        public void Email_Valid_DoesNotThrow()
        {
            Guard.Email("test@example.com");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Email_Empty_Throws(string input)
        {
            Assert.Throws<ArgumentException>(() => Guard.Email(input));
        }

        [Fact]
        public void Email_NoAtSymbol_Throws()
        {
            Assert.Throws<ArgumentException>(() => Guard.Email("invalid.email.com"));
        }

        // ============================================================
        // Password()
        // ============================================================

        [Fact]
        public void Password_Valid_DoesNotThrow()
        {
            Guard.Password("valid123");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Password_Empty_Throws(string input)
        {
            Assert.Throws<ArgumentException>(() => Guard.Password(input));
        }

        [Fact]
        public void Password_TooShort_Throws()
        {
            Assert.Throws<ArgumentException>(() => Guard.Password("1234567"));
        }

        // ============================================================
        // Text()
        // ============================================================

        [Fact]
        public void Text_Valid_DoesNotThrow()
        {
            Guard.Text("Hello", "Поле");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Text_Empty_Throws(string input)
        {
            Assert.Throws<ArgumentException>(() => Guard.Text(input, "Назва"));
        }

        [Fact]
        public void Text_TooLong_Throws()
        {
            string veryLong = new string('A', 5000);
            Assert.Throws<ArgumentException>(() => Guard.Text(veryLong, "Поле", max: 100));
        }

        [Fact]
        public void Text_MaxBoundary_DoesNotThrow()
        {
            string exact = new string('A', 10);
            Guard.Text(exact, "Поле", max: 10);
        }
    }
}
