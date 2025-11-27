namespace BrainBurst.Presentation;

using System;
using System.Windows;
using BrainBurst.BLL.Interfaces;
using BrainBurst.BLL.Interfaces.Abstractions;
using BrainBurst.BLL.Services;
using BrainBurst.DAL.Abstractions;
using BrainBurst.DAL.Data;
using BrainBurst.DAL.Repositories;
using BrainBurst.Presentation.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql.EntityFrameworkCore.PostgreSQL;

/// <summary>
/// Логіка взаємодії для App.xaml.
/// Головний клас додатка WPF, який налаштовує Dependency Injection (DI) та керування життєвим циклом хоста.
/// </summary>
public partial class App : Application
{
    private readonly IHost _host;

    /// <summary>
    /// Initializes a new instance of the <see cref="App"/> class.
    /// </summary>
    public App()
    {
        // Встановлюємо змінні середовища ДО створення хоста
        this.SetupEnvironmentVariables();

        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        this._host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                this.ConfigureServices(services);
            })
            .Build();
    }

    /// <summary>
    /// Gets отримує екземпляр хоста, який містить усі зареєстровані сервіси.
    /// </summary>
    public IHost ServiceHost => this._host;

    /// <summary>
    /// Викликається при закритті WPF-додатку.
    /// Гарантує коректне зупинення хоста.
    /// </summary>
    /// <param name="e">Дані події виходу.</param>
    protected override async void OnExit(ExitEventArgs e)
    {
        using (this._host)
        {
            await this._host.StopAsync(TimeSpan.FromSeconds(5));
        }

        base.OnExit(e);
    }

    /// <summary>
    /// Цей метод викликається автоматично при запуску WPF-додатку.
    /// Запускає хост, застосовує міграції та відображає головне вікно.
    /// </summary>
    /// <param name="e">Дані події запуску.</param>
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        await this._host.StartAsync();

        bool migrationSuccess = this.ApplyMigrations();

        if (!migrationSuccess)
        {
            this.Shutdown();
            return;
        }

        var mainWindow = this._host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private void SetupEnvironmentVariables()
    {
        Environment.SetEnvironmentVariable("DB_HOST", "dpg-d494k1odl3ps73dbasmg-a.frankfurt-postgres.render.com");
        Environment.SetEnvironmentVariable("DB_PORT", "5432");
        Environment.SetEnvironmentVariable("DB_NAME", "brainburst_ge7w");
        Environment.SetEnvironmentVariable("DB_USER", "whylek");
        Environment.SetEnvironmentVariable("DB_PASSWORD", "L1vFCiVN2WUncXGQx5fTx1iAJDBtOmgI");
        Environment.SetEnvironmentVariable("OPENAI_API_KEY", dummy);
    }

    /// <summary>
    /// Створює scope (область видимості) та застосовує міграції бази даних.
    /// </summary>
    /// <returns>True, якщо міграція успішна; False, якщо виникла помилка.</returns>
    private bool ApplyMigrations()
    {
        try
        {
            using (var scope = this._host.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                if (!dbContext.Database.CanConnect())
                {
                    MessageBox.Show("Не вдалося підключитися до бази даних. Перевірте інтернет-з'єднання або правильність Connection String.", "Помилка підключення");
                    return false;
                }

                dbContext.Database.Migrate();
            }

            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Критична помилка при оновленні бази даних:\n{ex.Message}\n\nДеталі: {ex.InnerException?.Message}", "Помилка міграції");
            return false;
        }
    }

    /// <summary>
    /// Реєструє всі сервіси, репозиторії та контексти даних у контейнері Dependency Injection.
    /// </summary>
    /// <param name="services">Колекція сервісів для конфігурації.</param>
    private void ConfigureServices(IServiceCollection services)
    {
        var connectionString = $"Host={Environment.GetEnvironmentVariable("DB_HOST")};Port={Environment.GetEnvironmentVariable("DB_PORT")};Database={Environment.GetEnvironmentVariable("DB_NAME")};Username={Environment.GetEnvironmentVariable("DB_USER")};Password={Environment.GetEnvironmentVariable("DB_PASSWORD")};Include Error Detail=true;SSL Mode=Require;Trust Server Certificate=True";

        services.AddDbContext<ApplicationDbContext>(
            options =>
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
        services.AddTransient<TestTakingView>();

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
}