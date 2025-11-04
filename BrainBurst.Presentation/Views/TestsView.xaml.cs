using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Input;
using System;
using Microsoft.Extensions.DependencyInjection;
using BrainBurst.BLL.Interfaces; // Додаємо для IFlashcardService та IAuthContext
using System.Threading.Tasks;
using System.Linq;

namespace BrainBurst.Presentation.Views
{
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
            InitializeComponent();
            _serviceProvider = serviceProvider;
            _flashcardService = flashcardService;
            _authContext = authContext; // <--- ІНІЦІАЛІЗОВАНО
            
            this.Loaded += TestsView_Loaded;
        }

        private void TestsView_Loaded(object sender, RoutedEventArgs e)
        {
            LoadTestsAsync(); 
        }

        private async Task LoadTestsAsync()
        {
             try
            {
                // Отримуємо всі картки поточного користувача
                // ВИКОРИСТАННЯ: CurrentUserId замінено на _authContext.CurrentUserId
                var allCards = await _flashcardService.ListAsync(_authContext.CurrentUserId, null, CancellationToken.None);

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
                
                DecksItemsControl.ItemsSource = groupedDecks;
                
                if (!groupedDecks.Any())
                {
                    NoTestsMessage.Visibility = Visibility.Visible;
                }
                else
                {
                    NoTestsMessage.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception)
            {
                NoTestsMessage.Text = "Помилка завантаження тестів.";
                NoTestsMessage.Visibility = Visibility.Visible;
            }
        }

        private void Test_Click(object sender, MouseButtonEventArgs e)
        {
            if (NavigationService.GetNavigationService(this) != null)
            {
                // Створюємо TestTakingView через DI
                var testTakingView = _serviceProvider.GetRequiredService<TestTakingView>();
                NavigationService.GetNavigationService(this).Navigate(testTakingView);
            }
        }
        private void CreateTest_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.GetNavigationService(this) != null)
            {
                // Створюємо CreateTestView через DI
                var createTestView = _serviceProvider.GetRequiredService<CreateTestView>();
                NavigationService.GetNavigationService(this).Navigate(createTestView);
            }
        }
    }
}