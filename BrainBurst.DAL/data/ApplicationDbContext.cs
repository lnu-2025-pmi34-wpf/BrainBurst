namespace BrainBurst.DAL.Data
{
    using System.IO;
    using BrainBurst.DAL.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Configuration.Json;

    /// <summary>
    /// Контекст бази даних Entity Framework для програми.
    /// Відповідає за зв'язок з БД та визначення таблиць.
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationDbContext"/> class.
        /// </summary>
        /// <param name="options">Опції для конфігурації DbContext, зазвичай передаються через DI.</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Gets or sets отримує або встановлює набір даних (таблицю) для сутностей <see cref="User"/>.
        /// </summary>
        public DbSet<User> Users { get; set; } = null!;

        /// <summary>
        /// Gets or sets отримує або встановлює набір даних (таблицю) для сутностей <see cref="Flashcard"/>.
        /// </summary>
        public DbSet<Flashcard> Flashcards { get; set; } = null!;

        /// <summary>
        /// Gets or sets отримує або встановлює набір даних (таблицю) для сутностей <see cref="Tag"/>.
        /// </summary>
        public DbSet<Tag> Tags { get; set; } = null!;

        /// <summary>
        /// Gets or sets отримує або встановлює набір даних (таблицю) для сутностей <see cref="Test"/>.
        /// </summary>
        public DbSet<Test> Tests { get; set; } = null!;

        /// <summary>
        /// Gets or sets отримує або встановлює набір даних (таблицю) для сутностей <see cref="TestResult"/>.
        /// </summary>
        public DbSet<TestResult> TestResults { get; set; } = null!;

        /// <summary>
        /// Gets or sets отримує або встановлює набір даних (таблицю) для сутностей <see cref="QuestionResult"/>.
        /// </summary>
        public DbSet<QuestionResult> QuestionResults { get; set; } = null!;

        /// <summary>
        /// Перевизначений метод для конфігурації опцій DbContext.
        /// </summary>
        /// <param name="optionsBuilder">Будівельник опцій для налаштування.</param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Конфігурація відбувається через DI в App.xaml.cs
        }

        /// <summary>
        /// Налаштовує модель бази даних, зв'язки та обмеження за допомогою Fluent API.
        /// </summary>
        /// <param name="modelBuilder">Будівельник, що використовується для конструювання моделі.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ... (той самий Fluent API, що був раніше)
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.UserId);
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Email).HasMaxLength(255).IsRequired();
                entity.Property(u => u.PasswordHash).HasMaxLength(255).IsRequired();
                entity.Property(u => u.FullName).HasMaxLength(100);
                entity.Property(u => u.Rank).HasMaxLength(20).HasDefaultValue("Початківець");
                entity.Property(u => u.Points).HasDefaultValue(0);
                entity.Property(u => u.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            modelBuilder.Entity<Flashcard>(entity =>
            {
                entity.HasKey(f => f.FlashcardId);
                entity.Property(f => f.Question).HasColumnType("text").IsRequired();
                entity.Property(f => f.Answer).HasColumnType("text").IsRequired();
                entity.Property(f => f.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasMany(f => f.Tags)
                    .WithMany(t => t.Flashcards)
                    .UsingEntity(j => j.ToTable("FlashcardTags"));
            });

            modelBuilder.Entity<Tag>(entity =>
            {
                entity.HasKey(t => t.TagId);
                entity.Property(t => t.Name).HasMaxLength(50).IsRequired();

                entity.HasOne(t => t.Creator)
                      .WithMany(u => u.Tags)
                      .HasForeignKey(t => t.CreatorId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Test>(entity =>
            {
                entity.HasKey(t => t.TestId);

                entity.HasOne(t => t.Creator)
                      .WithMany(u => u.Tests)
                      .HasForeignKey(t => t.CreatorId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<TestResult>(entity =>
            {
                entity.HasKey(tr => tr.TestResultId);
                entity.Property(tr => tr.CorrectAnswersPercent)
                      .HasColumnType("numeric(5,2)")
                      .IsRequired();
                entity.Property(tr => tr.TestDate)
                      .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(tr => tr.Test)
                      .WithMany(t => t.TestResults)
                      .HasForeignKey(tr => tr.TestId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(tr => tr.User)
                      .WithMany(u => u.TestResults)
                      .HasForeignKey(tr => tr.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<QuestionResult>(entity =>
            {
                entity.HasKey(qr => qr.QuestionResultId);
                entity.Property(qr => qr.UserInput).HasColumnType("text").IsRequired();

                entity.HasOne(qr => qr.TestResult)
                      .WithMany(tr => tr.QuestionResults)
                      .HasForeignKey(qr => qr.TestResultId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(qr => qr.Flashcard)
                      .WithMany(f => f.QuestionResults)
                      .HasForeignKey(qr => qr.FlashcardId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<User>().HasIndex(u => u.Email);
            modelBuilder.Entity<Flashcard>().HasIndex(f => f.CreatorId);
            modelBuilder.Entity<TestResult>().HasIndex(tr => tr.UserId);
            modelBuilder.Entity<TestResult>().HasIndex(tr => tr.TestId);
            modelBuilder.Entity<QuestionResult>().HasIndex(qr => qr.TestResultId);
            modelBuilder.Entity<QuestionResult>().HasIndex(qr => qr.FlashcardId);
            modelBuilder.Entity<Tag>().HasIndex(t => t.CreatorId);
        }
    }
}