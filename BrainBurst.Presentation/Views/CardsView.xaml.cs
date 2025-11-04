using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using BrainBurst.BLL.Interfaces; // Додаємо для IFlashcardService та IAuthContext
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Media;

namespace BrainBurst.Presentation.Views
{
    public partial class CardsView : UserControl
    {
        // 🚨 Більше не використовуємо: private const int CurrentUserId = 1; 

        private readonly IServiceProvider _serviceProvider;
        private readonly IFlashcardService _flashcardService;
        private readonly IAuthContext _authContext; // <--- ДОДАНО ПОЛЕ

        // Внутрішній DTO для відображення колод
        private class DeckItem
        {
            public string DeckTag { get; set; } = string.Empty; 
            public int CardCount { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        // ОНОВЛЕНО: Конструктор приймає IAuthContext
        public CardsView(IServiceProvider serviceProvider, IFlashcardService flashcardService, IAuthContext authContext)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
            _flashcardService = flashcardService;
            _authContext = authContext; // <--- ІНІЦІАЛІЗОВАНО
            
            this.Loaded += CardsView_Loaded;
        }
        
        private void CardsView_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCardsAsync(); 
        }

        private async Task LoadCardsAsync(string? search = null)
        {
             try
            {
                // ВИКОРИСТАННЯ: CurrentUserId замінено на _authContext.CurrentUserId
                var allCards = await _flashcardService.ListAsync(_authContext.CurrentUserId, search, CancellationToken.None);

                // Групуємо картки за першим тегом (імітація колод)
                var groupedDecks = allCards
                    .Where(c => c.Tags.Any())
                    .GroupBy(c => c.Tags.First()) 
                    .Select(g => new DeckItem
                    {
                        DeckTag = g.Key,
                        CardCount = g.Count(),
                        CreatedAt = g.Min(c => c.CreatedAt) 
                    })
                    .OrderByDescending(d => d.CreatedAt)
                    .ToList();
                
                // ... (далі йде існуючий код відображення)
                DecksItemsControl.ItemsSource = groupedDecks;
                
                if (!allCards.Any())
                {
                    NoCardsMessage.Visibility = Visibility.Visible;
                }
                else
                {
                    NoCardsMessage.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception)
            {
                NoCardsMessage.Text = "Помилка завантаження карток.";
                NoCardsMessage.Visibility = Visibility.Visible;
            }
        }
        
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadCardsAsync(SearchTextBox.Text);
        }
        

        private void Deck_Click(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (border != null)
            {
                var deckItem = border.DataContext as DeckItem; 
                
                if (deckItem != null && NavigationService.GetNavigationService(this) != null)
                {
                    var studyView = _serviceProvider.GetRequiredService<StudyView>();
                    NavigationService.GetNavigationService(this).Navigate(studyView);
                }
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.GetNavigationService(this) != null)
            {
                var createCardView = _serviceProvider.GetRequiredService<CreateCardView>();
                NavigationService.GetNavigationService(this).Navigate(createCardView);
            }
        }
    }
}