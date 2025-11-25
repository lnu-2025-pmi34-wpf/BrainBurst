namespace BrainBurst.Presentation.Views
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Navigation;
    using BrainBurst.BLL.Interfaces;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Внутрішній клас, що представляє одну доступну колоду (тему) для тесту.
    /// </summary>
    public class TestDeckItem
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

        /// <summary>
        /// Initializes a new instance of the <see cref="TestsView"/> class.
        /// </summary>
        public TestsView(IServiceProvider serviceProvider, IFlashcardService flashcardService, IAuthContext authContext)
        {
            this.InitializeComponent();
            this._serviceProvider = serviceProvider;
            this._flashcardService = flashcardService;
            this._authContext = authContext;

            this.Loaded += this.TestsView_Loaded;
        }

        private async void TestsView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await this.LoadTestsAsync();
            }
            catch (Exception)
            {
                this.NoTestsMessage.Text = "Помилка завантаження.";
            }
        }

        private async Task LoadTestsAsync()
        {
            try
            {
                var allCards = await this._flashcardService.ListAsync(this._authContext.CurrentUserId, null, CancellationToken.None);

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

                this.NoTestsMessage.Visibility = groupedDecks.Any() ? Visibility.Collapsed : Visibility.Visible;
            }
            catch (Exception)
            {
                this.NoTestsMessage.Text = "Помилка завантаження тестів.";
                this.NoTestsMessage.Visibility = Visibility.Visible;
            }
        }

        private async void Test_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var border = sender as Border;
                // Приводимо DataContext до нового типу TestDeckItem
                if (border?.DataContext is TestDeckItem deckItem)
                {
                    if (NavigationService.GetNavigationService(this) != null)
                    {
                        var testTakingView = this._serviceProvider.GetRequiredService<TestTakingView>();
                        
                        NavigationService.GetNavigationService(this).Navigate(testTakingView);

                        await testTakingView.InitializeTestByTagAsync(deckItem.DeckTag);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка запуску тесту: {ex.Message}", "Помилка");
            }
        }

        private void CreateTest_Click(object sender, RoutedEventArgs e)
        {
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
                MessageBox.Show($"Не вдалося відкрити вікно створення тесту: {ex.Message}", "Помилка");
            }
        }
    }
}