namespace BrainBurst.Presentation.Views
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Navigation;
    using BrainBurst.BLL.Interfaces;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Логіка взаємодії для відображення списку флеш-карток та групування їх у колоди.
    /// </summary>
    public partial class CardsView : UserControl
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IFlashcardService _flashcardService;
        private readonly IAuthContext _authContext;
        private readonly ILogger<CardsView> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CardsView"/> class.
        /// </summary>
        /// <param name="serviceProvider">Постачальник служб DI (для навігації).</param>
        /// <param name="flashcardService">Сервіс для доступу до флеш-карток.</param>
        /// <param name="authContext">Контекст автентифікації для отримання ID користувача.</param>
        /// <param name="logger">Логер для запису подій.</param>
        public CardsView(IServiceProvider serviceProvider, IFlashcardService flashcardService, IAuthContext authContext, ILogger<CardsView> logger)
        {
            this.InitializeComponent();
            this._serviceProvider = serviceProvider;
            this._flashcardService = flashcardService;
            this._authContext = authContext;
            this._logger = logger;

            this._logger.LogDebug("CardsView: View ініціалізовано.");

            this.IsVisibleChanged += this.CardsView_IsVisibleChanged;
        }

        private async void CardsView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == true)
            {
                this._logger.LogInformation("CardsView_IsVisibleChanged: View став видимим. Запуск завантаження карток.");
                await this.LoadCardsAsync();
            }
        }

        private async Task LoadCardsAsync(string? search = null)
        {
            this._logger.LogDebug("LoadCardsAsync: Початок завантаження карток. Пошуковий запит: {SearchQuery}", search ?? "відсутній");

            try
            {
                var allCards = await this._flashcardService.ListAsync(this._authContext.CurrentUserId, search, CancellationToken.None);

                this._logger.LogDebug("LoadCardsAsync: Отримано {Count} карток з FlashcardService.", allCards.Count);

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

                this._logger.LogInformation("LoadCardsAsync: Згруповано {DeckCount} колод.", groupedDecks.Count);

                this.DecksItemsControl.ItemsSource = groupedDecks;

                if (!allCards.Any())
                {
                    this.NoCardsMessage.Visibility = Visibility.Visible;
                    this._logger.LogInformation("LoadCardsAsync: Картки відсутні.");
                }
                else
                {
                    this.NoCardsMessage.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "LoadCardsAsync: Критична помилка під час завантаження або групування карток.");

                this.NoCardsMessage.Text = "Помилка завантаження.";
                this.NoCardsMessage.Visibility = Visibility.Visible;
            }
        }

        private async void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this._logger.LogDebug("SearchTextBox_TextChanged: Запущено пошук за текстом: {SearchText}", this.SearchTextBox.Text);
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
                    this._logger.LogInformation("Deck_Click: Користувач клікнув на колоду: {DeckTag}", deckItem.DeckTag);
                    try
                    {
                        var studyView = this._serviceProvider.GetRequiredService<StudyView>();

                        // Передаємо назву тегу (колоди), щоб завантажити правильні картки
                        studyView.Configure(deckItem.DeckTag);

                        NavigationService.GetNavigationService(this).Navigate(studyView);
                    }
                    catch (Exception ex)
                    {
                        this._logger.LogError(ex, "Deck_Click: Помилка переходу до StudyView для тегу: {DeckTag}", deckItem.DeckTag);
                        MessageBox.Show($"Помилка переходу до навчання: {ex.Message}", "Помилка");
                    }
                }
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            this._logger.LogInformation("AddButton_Click: Перехід до створення картки.");

            try
            {
                if (NavigationService.GetNavigationService(this) != null)
                {
                    var createCardView = this._serviceProvider.GetRequiredService<CreateCardView>();
                    NavigationService.GetNavigationService(this).Navigate(createCardView);
                }
                else
                {
                    this._logger.LogWarning("AddButton_Click: NavigationService недоступний.");
                }
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "AddButton_Click: Критична помилка при переході до створення картки.");
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