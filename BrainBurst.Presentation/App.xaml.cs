using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;
using BrainBurst.DAL.Data;
using BrainBurst.DAL.Abstractions;
using BrainBurst.DAL.Repositories;
using BrainBurst.BLL.Interfaces;
using BrainBurst.BLL.Services;
using BrainBurst.BLL.Interfaces.Abstractions;
using System;
using BrainBurst.Presentation.Views;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace BrainBurst.Presentation;

public partial class App : Application
{
    private readonly IHost _host;
    public IHost ServiceHost => _host; 

    public App()
    {
        // Встановлюємо змінні середовища ДО створення хоста
        SetupEnvironmentVariables();

        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                ConfigureServices(services);
            })
            .Build();
    }

    private void SetupEnvironmentVariables()
    {
        Environment.SetEnvironmentVariable("DB_HOST", "dpg-d494k1odl3ps73dbasmg-a.frankfurt-postgres.render.com");
        Environment.SetEnvironmentVariable("DB_PORT", "5432");
        Environment.SetEnvironmentVariable("DB_NAME", "brainburst_ge7w");
        Environment.SetEnvironmentVariable("DB_USER", "whylek");
        Environment.SetEnvironmentVariable("DB_PASSWORD", "L1vFCiVN2WUncXGQx5fTx1iAJDBtOmgI");
    }

    // Цей метод викликається автоматично при запуску WPF-додатку
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e); // Викликаємо базовий метод спочатку

        await _host.StartAsync();

        // *** СПРОБА МІГРАЦІЇ ПРИ ЗАПУСКУ ***
        bool migrationSuccess = ApplyMigrations();

        if (!migrationSuccess)
        {
            // Якщо міграція не вдалася, закриваємо додаток, щоб не показувати головне вікно
            Shutdown();
            return;
        }

        // Якщо міграція пройшла успішно, показуємо головне вікно
        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private bool ApplyMigrations()
    {
        try
        {
            using (var scope = _host.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                
                // Перевіряємо, чи можемо ми взагалі підключитися
                if (!dbContext.Database.CanConnect())
                {
                     MessageBox.Show("Не вдалося підключитися до бази даних. Перевірте інтернет-з'єднання або правильність Connection String.", "Помилка підключення");
                     return false;
                }

                // Застосовуємо міграції
                dbContext.Database.Migrate();
            }
            // Якщо дійдемо сюди, міграція пройшла успішно
            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Критична помилка при оновленні бази даних:\n{ex.Message}\n\nДеталі: {ex.InnerException?.Message}", "Помилка міграції");
            return false;
        }
    }

    private void ConfigureServices(IServiceCollection services)
    {
        var connectionString = $"Host={Environment.GetEnvironmentVariable("DB_HOST")};Port={Environment.GetEnvironmentVariable("DB_PORT")};Database={Environment.GetEnvironmentVariable("DB_NAME")};Username={Environment.GetEnvironmentVariable("DB_USER")};Password={Environment.GetEnvironmentVariable("DB_PASSWORD")};Include Error Detail=true;SSL Mode=Require;Trust Server Certificate=True";

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        }, ServiceLifetime.Transient);

        services.AddTransient<IUserRepository, UserRepository>();
        services.AddTransient<IFlashcardRepository, FlashcardRepository>();
        services.AddTransient<ITestRepository, TestRepository>();
        services.AddTransient<ITestResultRepository, TestResultRepository>();

        services.AddTransient<IRatingService, RankingService>();
        services.AddSingleton<IAuthContext, AuthContext>();

        services.AddTransient<IAuthService, AuthService>();
        services.AddTransient<IUserService, UserService>();
        services.AddTransient<IFlashcardService, FlashcardService>();
        services.AddTransient<IArchiveService, ArchiveService>();
        services.AddTransient<ITestService, TestService>();
        services.AddTransient<ITestGenerationService, TestGenerationService>();

        services.AddTransient<IQuizGenerator, OpenAIQuizGenerator>();
        
        services.AddSingleton<MainWindow>();
        services.AddTransient<LoginWindow>();
        services.AddTransient<RegistrationWindow>();
        services.AddTransient<ProfileWindow>();

        services.AddTransient<ProfileView>();
        services.AddTransient<CardsView>();
        services.AddTransient<TestsView>();
        services.AddTransient<AwardsView>();
        services.AddTransient<SettingsView>();
        services.AddTransient<EditProfileView>();
        services.AddTransient<ChangePasswordView>();
        services.AddTransient<DeleteAccountView>();
        services.AddTransient<ArchiveView>();
        services.AddTransient<TestResultsView>();
        services.AddTransient<CreateCardView>();
        services.AddTransient<CreateTestView>();
        services.AddTransient<StudyView>();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        using (_host)
        {
            await _host.StopAsync(TimeSpan.FromSeconds(5));
        }
        base.OnExit(e);
    }
}