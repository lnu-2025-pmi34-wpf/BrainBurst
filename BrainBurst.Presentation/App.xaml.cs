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
using Microsoft.EntityFrameworkCore; // <-- Обов'язково для методів розширення
using Npgsql.EntityFrameworkCore.PostgreSQL; // <-- Обов'язково для UseNpgsql

namespace BrainBurst.Presentation;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private readonly IHost _host;
    // Зберігаємо хост у публічній властивості для доступу до ServiceProvider
    public IHost ServiceHost => _host; 

    public App()
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                ConfigureServices(services);
            })
            .Build();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // === ПРИМУСОВЕ ВСТАНОВЛЕННЯ ЗМІННИХ ДЛЯ HOST/EF CORE ===
        // Використовуємо тут для гарантованого доступу до конфігурації
        Environment.SetEnvironmentVariable("DB_HOST", "dpg-d3klhv24d50c73dd0e10-a.frankfurt-postgres.render.com");
        Environment.SetEnvironmentVariable("DB_PORT", "5432");
        Environment.SetEnvironmentVariable("DB_NAME", "brainburst");
        Environment.SetEnvironmentVariable("DB_USER", "whylek");
        Environment.SetEnvironmentVariable("DB_PASSWORD", "Ti2adBtW09S1josSAgeo29OpKuKqXcZJ");
        
        // Формуємо повний Connection String
        var connectionString = $"Host={Environment.GetEnvironmentVariable("DB_HOST")};Port={Environment.GetEnvironmentVariable("DB_PORT")};Database={Environment.GetEnvironmentVariable("DB_NAME")};Username={Environment.GetEnvironmentVariable("DB_USER")};Password={Environment.GetEnvironmentVariable("DB_PASSWORD")};Include Error Detail=true;SSL Mode=Require;Trust Server Certificate=True";


        // === 1. КОНТЕКСТ БД (КОРЕКТНА РЕЄСТРАЦІЯ З РЯДКОМ ПІДКЛЮЧЕННЯ) ===
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            // Використовуємо UseNpgsql
            options.UseNpgsql(connectionString);
        }, ServiceLifetime.Transient);

        // === 2. РЕПОЗИТОРІЇ (DAL) ===
        services.AddTransient<IUserRepository, UserRepository>();
        services.AddTransient<IFlashcardRepository, FlashcardRepository>();
        services.AddTransient<ITestRepository, TestRepository>();
        services.AddTransient<ITestResultRepository, TestResultRepository>();

        // === 3. СЕРВІСИ (BLL) ===
        services.AddTransient<IRatingService, RankingService>();
        
        services.AddSingleton<IAuthContext, AuthContext>(); // НОВИЙ РЯДОК: Реєструємо як Singleton

        services.AddTransient<IAuthService, AuthService>();
        services.AddTransient<IUserService, UserService>();
        services.AddTransient<IFlashcardService, FlashcardService>();
        services.AddTransient<IArchiveService, ArchiveService>();
        services.AddTransient<ITestService, TestService>();
        services.AddTransient<ITestGenerationService, TestGenerationService>();

        // === 4. AI-ГЕНЕРАТОР ===
        services.AddTransient<IQuizGenerator, OpenAIQuizGenerator>();
        
        // === 5. WINDOWS & VIEWS (UI) ===
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
        services.AddTransient<StudyView>(); // Додаємо StudyView, оскільки його конструктор оновлено
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        base.OnStartup(e);
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