using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System;
using Microsoft.Extensions.DependencyInjection;
using BrainBurst.BLL.Interfaces; // Додано для IAuthContext
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using BrainBurst.BLL.DTO;
using System.Collections.Generic;
using System.Windows.Media;

namespace BrainBurst.Presentation.Views
{
    public partial class CreateTestView : UserControl
    {
        // УСУНЕНО: private const int CurrentUserId = 1; 
        
        // Внутрішній DTO для відображення колод
        private class DeckItem
        {
            public int FlashcardId { get; set; }
            public string TagsStr { get; set; } 
            public DateTime CreatedAt { get; set; }
        }

        private readonly IFlashcardService _flashcardService;
        private readonly ITestService _testService;
        private readonly IAuthContext _authContext; // <--- ДОДАНО ПОЛЕ
        
        // Зберігаємо всі картки, щоб потім відфільтрувати їх за обраними "колодами"
        private IReadOnlyList<FlashcardDTO> AllCards = Array.Empty<FlashcardDTO>();

        // ОНОВЛЕНО: Конструктор приймає IAuthContext
        public CreateTestView(IFlashcardService flashcardService, ITestService testService, IAuthContext authContext)
        {
            InitializeComponent();
            _flashcardService = flashcardService;
            _testService = testService;
            _authContext = authContext; // <--- ІНІЦІАЛІЗОВАНО
            
            // Завантажуємо дані при завантаженні UI
            this.Loaded += CreateTestView_Loaded;
        }

        private void CreateTestView_Loaded(object sender, RoutedEventArgs e)
        {
            LoadDecksAsync();
        }
        
        // Завантажує всі картки та групує їх, імітуючи колоди
        private async Task LoadDecksAsync()
        {
            try
            {
                StatusText.Text = "Завантаження карток...";
                StatusText.Foreground = Brushes.Gray;
                
                // ВИКОРИСТАННЯ: CurrentUserId замінено на _authContext.CurrentUserId
                AllCards = await _flashcardService.ListAsync(_authContext.CurrentUserId, null, CancellationToken.None);
                
                // Групуємо картки за першим тегом (імітація колод)
                var decks = AllCards
                    .Where(c => c.Tags.Any())
                    .GroupBy(c => c.Tags.First()) // Групуємо за першим тегом
                    .Select(g => new DeckItem
                    {
                        FlashcardId = g.First().Id, // ID першої картки як представника колоди
                        TagsStr = g.Key,
                        CreatedAt = g.Min(c => c.CreatedAt) 
                    })
                    .OrderBy(d => d.TagsStr)
                    .ToList();

                DeckListBox.ItemsSource = decks;

                StatusText.Text = decks.Any() ? "" : "Картки не знайдено. Створіть нову картку, щоб мати можливість генерувати тест.";
                StatusText.Foreground = Brushes.Red;
            }
            catch (Exception)
            {
                StatusText.Text = "Помилка завантаження карток.";
                StatusText.Foreground = Brushes.Red;
            }
        }


        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.GetNavigationService(this).CanGoBack)
            {
                NavigationService.GetNavigationService(this).GoBack();
            }
        }

        private async void GenerateTest_Click(object sender, RoutedEventArgs e)
        {
            var selectedDecks = DeckListBox.SelectedItems.Cast<DeckItem>().ToList();

            if (!selectedDecks.Any())
            {
                StatusText.Text = "Оберіть хоча б одну колоду карток.";
                StatusText.Foreground = Brushes.Red;
                return;
            }
            
            StatusText.Text = "Створення тесту...";
            StatusText.Foreground = Brushes.Gray;

            try
            {
                // 1. Отримуємо ID всіх карток, що належать обраним "колодам"
                var selectedTags = selectedDecks.Select(d => d.TagsStr).ToList();
                var flashcardIds = AllCards
                    .Where(c => c.Tags.Any() && selectedTags.Contains(c.Tags.First()))
                    .Select(c => c.Id)
                    .ToList();

                if (!flashcardIds.Any())
                {
                    StatusText.Text = "Не вдалося знайти картки для обраних колод.";
                    StatusText.Foreground = Brushes.Red;
                    return;
                }
                
                // 2. Генеруємо тест
                // ВИКОРИСТАННЯ: CurrentUserId замінено на _authContext.CurrentUserId
                await _testService.GenerateFromFlashcardsAsync(_authContext.CurrentUserId, flashcardIds, CancellationToken.None);

                // 3. Успіх: повертаємось на попередній екран (TestsView).
                StatusText.Text = "Тест успішно згенеровано!";
                StatusText.Foreground = Brushes.Green;
                
                if (NavigationService.GetNavigationService(this).CanGoBack)
                {
                    NavigationService.GetNavigationService(this).GoBack();
                }
            }
            catch (ArgumentException ex)
            {
                StatusText.Text = ex.Message;
                StatusText.Foreground = Brushes.Red;
            }
            catch (Exception)
            {
                StatusText.Text = "Непередбачена помилка при генерації тесту.";
                StatusText.Foreground = Brushes.Red;
            }
        }
    }
}