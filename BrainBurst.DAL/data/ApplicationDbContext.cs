using BrainBurst.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
 // ← ПОВЕРНУЛИ
using Microsoft.Extensions.Configuration.Json;          // ← ПОВЕРНУЛИ
using System.IO;

namespace BrainBurst.DAL.Data
{
    public class ApplicationDbContext : DbContext
    {
        private readonly IConfiguration _configuration;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public ApplicationDbContext()
        {
            var currentDir = Directory.GetCurrentDirectory();
            var solutionDir = Directory.GetParent(currentDir)?.Parent?.FullName
                              ?? Directory.GetParent(currentDir)?.FullName
                              ?? currentDir;

            var envPath = Path.Combine(solutionDir, ".env");

            if (File.Exists(envPath))
            {
                foreach (var line in File.ReadAllLines(envPath))
                {
                    if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith('#'))
                        continue;

                    var parts = line.Split('=', 2);
                    if (parts.Length != 2) continue;

                    var key = parts[0].Trim();
                    var value = parts[1].Trim().Trim('"', '\'');

                    Environment.SetEnvironmentVariable(key, value);
                }
            }

            var builder = new ConfigurationBuilder()
                .SetBasePath(currentDir)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            _configuration = builder.Build();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                optionsBuilder.UseNpgsql(connectionString);
            }
        }

        // === DbSet ===
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Flashcard> Flashcards { get; set; } = null!;
        public DbSet<Tag> Tags { get; set; } = null!;
        public DbSet<Test> Tests { get; set; } = null!;
        public DbSet<TestResult> TestResults { get; set; } = null!;
        public DbSet<QuestionResult> QuestionResults { get; set; } = null!;

        // === OnModelCreating ===
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

                entity.HasOne(f => f.Creator)
                      .WithMany(u => u.Flashcards)
                      .HasForeignKey(f => f.CreatorId)
                      .OnDelete(DeleteBehavior.Cascade);
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