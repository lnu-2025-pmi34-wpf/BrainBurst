namespace BrainBurst.Presentation.Views
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Navigation;
    using BrainBurst.BLL.Interfaces;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Логіка взаємодії для відображення списку флеш-карток та групування їх у колоди.
    /// </summary>
    public partial class CardsView : UserControl
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IFlashcardService _flashcardService;
        private readonly IAuthContext _authContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="CardsView"/> class.
        /// </summary>
        /// <param name="serviceProvider">Постачальник служб DI (для навігації).</param>
        /// <param name="flashcardService">Сервіс для доступу до флеш-карток.</param>
        /// <param name="authContext">Контекст автентифікації для отримання ID користувача.</param>
        public CardsView(IServiceProvider serviceProvider, IFlashcardService flashcardService, IAuthContext authContext)
        {
            this.InitializeComponent();
            this._serviceProvider = serviceProvider;
            this._flashcardService = flashcardService;
            this._authContext = authContext;

            this.IsVisibleChanged += this.CardsView_IsVisibleChanged;
        }

        private async void CardsView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == true)
            {
                await this.LoadCardsAsync();
            }
        }

        private async Task LoadCardsAsync(string? search = null)
        {
            try
            {
                var allCards = await this._flashcardService.ListAsync(this._authContext.CurrentUserId, search, CancellationToken.None);

                var groupedDecks = allCards
                    .Where(c => c.Tags.Any())
                    .GroupBy(c => c.Tags.First())
                    .Select(g => new DeckItem
                    {
                        DeckTag = g.Key,
                        CardCount = g.Count(),
                        CreatedAt = g.Min(c => c.CreatedAt),
                    })
                    .OrderByDescending(d => d.CreatedAt)
                    .ToList();

                this.DecksItemsControl.ItemsSource = groupedDecks;

                if (!allCards.Any())
                {
                    this.NoCardsMessage.Visibility = Visibility.Visible;
                }
                else
                {
                    this.NoCardsMessage.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception)
            {
                this.NoCardsMessage.Text = "Помилка завантаження.";
                this.NoCardsMessage.Visibility = Visibility.Visible;
            }
        }

        private async void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            await this.LoadCardsAsync(this.SearchTextBox.Text);
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
                        var studyView = this._serviceProvider.GetRequiredService<StudyView>();
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
                    var createCardView = this._serviceProvider.GetRequiredService<CreateCardView>();
                    NavigationService.GetNavigationService(this).Navigate(createCardView);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Критична помилка при переході до створення картки:\n\n{ex.Message}\n\nInner Exception: {ex.InnerException?.Message}", "Знайдено помилку!");
            }
        }

        private class DeckItem
        {
            public string DeckTag { get; set; } = string.Empty;

            public int CardCount { get; set; }

            public DateTime CreatedAt { get; set; }
        }
    }
}