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
    /// Логіка взаємодії для View відображення доступних тестів (згрупованих за темами).
    /// </summary>
    public partial class TestsView : UserControl
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IFlashcardService _flashcardService;
        private readonly IAuthContext _authContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestsView"/> class.
        /// </summary>
        /// <param name="serviceProvider">Постачальник служб DI (для навігації).</param>
        /// <param name="flashcardService">Сервіс для отримання списку карток.</param>
        /// <param name="authContext">Контекст автентифікації для отримання ID користувача.</param>
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
            }
        }

        private async Task LoadTestsAsync()
        {
            try
            {
                var allCards = await this._flashcardService.ListAsync(this._authContext.CurrentUserId, null, CancellationToken.None);

                var groupedDecks = allCards
                    .Where(c => c.Tags.Any())
                    .GroupBy(c => c.Tags.First())
                    .Select(g => new DeckItem
                    {
                        DeckTag = g.Key,
                        CardCount = g.Count(),
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
                var testTakingView = this._serviceProvider.GetRequiredService<TestTakingView>();
                NavigationService.GetNavigationService(this).Navigate(testTakingView);
            }
        }

        private void CreateTest_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.GetNavigationService(this) != null)
            {
                var createTestView = this._serviceProvider.GetRequiredService<CreateTestView>();
                NavigationService.GetNavigationService(this).Navigate(createTestView);
            }
        }

        /// <summary>
        /// Внутрішній клас, що представляє одну доступну колоду (тему) для тесту.
        /// </summary>
        private class DeckItem
        {
            public string DeckTag { get; set; } = string.Empty;

            public int CardCount { get; set; }
        }
    }
}