namespace BrainBurst.Presentation.Views
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Navigation;
    using BrainBurst.BLL.Interfaces; // Додаємо для IFlashcardService та IAuthContext
    using Microsoft.Extensions.DependencyInjection;

    public partial class TestsView : UserControl
    {
        // УСУНЕНО: private const int CurrentUserId = 1;

        private readonly IServiceProvider _serviceProvider;
        private readonly IFlashcardService _flashcardService;
        private readonly IAuthContext _authContext; // <--- ДОДАНО ПОЛЕ

        // Внутрішній DTO для відображення колод-тестів
        private class DeckItem
        {
            public string DeckTag { get; set; } = string.Empty;

            public int CardCount { get; set; }
        }

        // ОНОВЛЕНО: Конструктор приймає IAuthContext
        public TestsView(IServiceProvider serviceProvider, IFlashcardService flashcardService, IAuthContext authContext)
        {
            this.InitializeComponent();
            this._serviceProvider = serviceProvider;
            this._flashcardService = flashcardService;
            this._authContext = authContext; // <--- ІНІЦІАЛІЗОВАНО

            this.Loaded += this.TestsView_Loaded;
        }

        private void TestsView_Loaded(object sender, RoutedEventArgs e)
        {
            this.LoadTestsAsync();
        }

        private async Task LoadTestsAsync()
        {
             try
            {
                // Отримуємо всі картки поточного користувача
                // ВИКОРИСТАННЯ: CurrentUserId замінено на _authContext.CurrentUserId
                var allCards = await this._flashcardService.ListAsync(this._authContext.CurrentUserId, null, CancellationToken.None);

                // Групуємо картки за першим тегом (імітація доступних тестів)
                var groupedDecks = allCards
                    .Where(c => c.Tags.Any())
                    .GroupBy(c => c.Tags.First())
                    .Select(g => new DeckItem
                    {
                        DeckTag = g.Key,
                        CardCount = g.Count()
                    })
                    .OrderByDescending(d => d.CardCount)
                    .ToList();

                this.DecksItemsControl.ItemsSource = groupedDecks;

                if (!groupedDecks.Any())
                {
                    this.NoTestsMessage.Visibility = Visibility.Visible;
                }
                else
                {
                    this.NoTestsMessage.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception)
            {
                this.NoTestsMessage.Text = "Помилка завантаження тестів.";
                this.NoTestsMessage.Visibility = Visibility.Visible;
            }
        }

        private void Test_Click(object sender, MouseButtonEventArgs e)
        {
            if (NavigationService.GetNavigationService(this) != null)
            {
                // Створюємо TestTakingView через DI
                var testTakingView = this._serviceProvider.GetRequiredService<TestTakingView>();
                NavigationService.GetNavigationService(this).Navigate(testTakingView);
            }
        }

        private void CreateTest_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.GetNavigationService(this) != null)
            {
                // Створюємо CreateTestView через DI
                var createTestView = this._serviceProvider.GetRequiredService<CreateTestView>();
                NavigationService.GetNavigationService(this).Navigate(createTestView);
            }
        }
    }
}