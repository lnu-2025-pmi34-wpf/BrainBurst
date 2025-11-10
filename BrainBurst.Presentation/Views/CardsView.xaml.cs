using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using BrainBurst.BLL.Interfaces;
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
        private readonly IServiceProvider _serviceProvider;
        private readonly IFlashcardService _flashcardService;
        private readonly IAuthContext _authContext;

        private class DeckItem
        {
            public string DeckTag { get; set; } = string.Empty; 
            public int CardCount { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        public CardsView(IServiceProvider serviceProvider, IFlashcardService flashcardService, IAuthContext authContext)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
            _flashcardService = flashcardService;
            _authContext = authContext;
            
            // Змінюємо підписку: тепер реагуємо на зміну видимості, а не лише на перше завантаження
            this.IsVisibleChanged += CardsView_IsVisibleChanged;
        }

        // Цей метод спрацьовує щоразу, коли сторінка стає видимою (наприклад, при поверненні назад)
        private void CardsView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == true)
            {
                // Якщо сторінка стала видимою, перезавантажуємо дані
                LoadCardsAsync();
            }
        }

        // Старий метод Loaded більше не потрібен, його можна видалити
        // private void CardsView_Loaded(object sender, RoutedEventArgs e) { ... }

        private async Task LoadCardsAsync(string? search = null)
        {
             try
            {
                var allCards = await _flashcardService.ListAsync(_authContext.CurrentUserId, search, CancellationToken.None);

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
            catch (Exception ex)
            {
                // MessageBox.Show($"Помилка завантаження карток: {ex.Message}", "Помилка");
                NoCardsMessage.Text = "Помилка завантаження.";
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
                    try 
                    {
                        var studyView = _serviceProvider.GetRequiredService<StudyView>();
                        NavigationService.GetNavigationService(this).Navigate(studyView);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Помилка переходу до навчання: {ex.Message}", "Помилка");
                    }
                }
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (NavigationService.GetNavigationService(this) != null)
                {
                    var createCardView = _serviceProvider.GetRequiredService<CreateCardView>();
                    NavigationService.GetNavigationService(this).Navigate(createCardView);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Критична помилка при переході до створення картки:\n\n{ex.Message}\n\nInner Exception: {ex.InnerException?.Message}", "Знайдено помилку!");
            }
        }
    }
}