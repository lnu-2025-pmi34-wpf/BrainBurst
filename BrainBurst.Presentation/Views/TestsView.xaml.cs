namespace BrainBurst.Presentation.Views
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Navigation;
    using BrainBurst.BLL.Interfaces;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Внутрішній клас, що представляє одну доступну колоду (тему) для тесту.
    /// </summary>
#pragma warning disable SA1649
#pragma warning disable SA1402
    public class TestDeckItem
#pragma warning restore SA1402
#pragma warning restore SA1649
    {
        /// <summary>
        /// Gets or sets назву тегу (теми).
        /// </summary>
        public string DeckTag { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets кількість карток.
        /// </summary>
        public int CardCount { get; set; }
    }

    /// <summary>
    /// Логіка взаємодії для View відображення доступних тестів.
    /// </summary>
    public partial class TestsView : UserControl
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IFlashcardService _flashcardService;
        private readonly IAuthContext _authContext;
        private readonly ILogger<TestsView> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestsView"/> class.
        /// </summary>
        /// <param name="logger">Логер для запису подій.</param>
        /// <param name="serviceProvider">Постачальник служб DI (для навігації).</param>
        /// <param name="flashcardService">Сервіс для отримання списку карток (для групування тем).</param>
        /// <param name="authContext">Контекст автентифікації для отримання ID користувача.</param>
        public TestsView(IServiceProvider serviceProvider, IFlashcardService flashcardService, IAuthContext authContext, ILogger<TestsView> logger)
        {
            this.InitializeComponent();
            this._serviceProvider = serviceProvider;
            this._flashcardService = flashcardService;
            this._authContext = authContext;
            this._logger = logger;

            this._logger.LogDebug("TestsView: View ініціалізовано.");
            this.Loaded += this.TestsView_Loaded;
        }

        private async void TestsView_Loaded(object sender, RoutedEventArgs e)
        {
            this._logger.LogInformation("TestsView_Loaded: Запуск завантаження доступних тестів.");

            try
            {
                await this.LoadTestsAsync();
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "TestsView_Loaded: Непередбачена помилка при завантаженні тестів.");
                this.NoTestsMessage.Text = "Помилка завантаження.";
            }
        }

        private async Task LoadTestsAsync()
        {
            this._logger.LogDebug("LoadTestsAsync: Початок отримання карток для групування в тести.");

            try
            {
                var allCards = await this._flashcardService.ListAsync(this._authContext.CurrentUserId, null, CancellationToken.None);

                this._logger.LogDebug("LoadTestsAsync: Отримано {Count} карток.", allCards.Count);

                var groupedDecks = allCards
                    .Where(c => c.Tags != null && c.Tags.Any())
                    .GroupBy(c => c.Tags.First())
                    .Select(g => new TestDeckItem // Використовуємо нове ім'я класу
                    {
                        DeckTag = g.Key,
                        CardCount = g.Count(),
                    })
                    .OrderByDescending(d => d.CardCount)
                    .ToList();

                this.DecksItemsControl.ItemsSource = groupedDecks;

                if (groupedDecks.Any())
                {
                    this._logger.LogInformation("LoadTestsAsync: Знайдено {DeckCount} доступних колод для тестування.", groupedDecks.Count);
                }

                this.NoTestsMessage.Visibility = groupedDecks.Any() ? Visibility.Collapsed : Visibility.Visible;
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "LoadTestsAsync: Критична помилка під час завантаження або групування колод тестів.");

                this.NoTestsMessage.Text = "Помилка завантаження тестів.";
                this.NoTestsMessage.Visibility = Visibility.Visible;
                throw;
            }
        }

        private async void Test_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var border = sender as Border;
                if (border?.DataContext is TestDeckItem deckItem)
                {
                    this._logger.LogInformation("Test_Click: Запуск тесту для колоди: {DeckTag}", deckItem.DeckTag);

                    if (NavigationService.GetNavigationService(this) != null)
                    {
                        var testTakingView = this._serviceProvider.GetRequiredService<TestTakingView>();

                        NavigationService.GetNavigationService(this).Navigate(testTakingView);
                        this._logger.LogDebug("Test_Click: Навігація до TestTakingView успішна.");

                        await testTakingView.InitializeTestByTagAsync(deckItem.DeckTag);
                        this._logger.LogInformation("Test_Click: Тест для колоди {DeckTag} ініціалізовано.", deckItem.DeckTag);
                    }
                }
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Test_Click: Критична помилка при запуску або ініціалізації тесту.");
                MessageBox.Show($"Помилка запуску тесту: {ex.Message}", "Помилка");
            }
        }

        private void CreateTest_Click(object sender, RoutedEventArgs e)
        {
            this._logger.LogInformation("CreateTest_Click: Навігація до створення тесту.");

            try
            {
                if (NavigationService.GetNavigationService(this) != null)
                {
                    var createTestView = this._serviceProvider.GetRequiredService<CreateTestView>();
                    NavigationService.GetNavigationService(this).Navigate(createTestView);
                }
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "CreateTest_Click: Помилка при навігації до CreateTestView.");
                MessageBox.Show($"Не вдалося відкрити вікно створення тесту: {ex.Message}", "Помилка");
            }
        }
    }
}
