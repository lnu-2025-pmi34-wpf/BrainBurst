namespace BrainBurst.Presentation.Views
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Navigation;
    using BrainBurst.BLL.DTO;
    using BrainBurst.BLL.Interfaces;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Логіка взаємодії для View створення нового тесту з наявних флеш-карток.
    /// </summary>
    public partial class CreateTestView : UserControl
    {
        private readonly IFlashcardService _flashcardService;
        private readonly ITestService _testService;
        private readonly IAuthContext _authContext;
        private IReadOnlyList<FlashcardDTO> allCards = Array.Empty<FlashcardDTO>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateTestView"/> class.
        /// </summary>
        /// <param name="flashcardService">Сервіс для отримання списку карток.</param>
        /// <param name="testService">Сервіс для генерації тесту.</param>
        /// <param name="authContext">Контекст автентифікації для отримання ID користувача.</param>
        public CreateTestView(IFlashcardService flashcardService, ITestService testService, IAuthContext authContext)
        {
            this.InitializeComponent();
            this._flashcardService = flashcardService;
            this._testService = testService;
            this._authContext = authContext;

            this.Loaded += this.CreateTestView_Loaded;
        }

        private async void CreateTestView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await this.LoadDecksAsync();
            }
            catch
            {
            }
        }

        private async Task LoadDecksAsync()
        {
            try
            {
                this.StatusText.Text = "Завантаження карток...";
                this.StatusText.Foreground = Brushes.Gray;

                this.allCards = await this._flashcardService.ListAsync(this._authContext.CurrentUserId, null, CancellationToken.None);

                var decks = this.allCards
                    .Where(c => c.Tags.Any())
                    .GroupBy(c => c.Tags.First())
                    .Select(g => new DeckItem
                    {
                        FlashcardId = g.First().Id,
                        TagsStr = g.Key,
                        CreatedAt = g.Min(c => c.CreatedAt),
                    })
                    .OrderBy(d => d.TagsStr)
                    .ToList();

                this.DeckListBox.ItemsSource = decks;

                this.StatusText.Text = decks.Any() ? string.Empty : "Картки не знайдено. Створіть нову картку, щоб мати можливість генерувати тест.";
                this.StatusText.Foreground = Brushes.Red;
            }
            catch (Exception)
            {
                this.StatusText.Text = "Помилка завантаження карток.";
                this.StatusText.Foreground = Brushes.Red;
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
            var selectedDecks = this.DeckListBox.SelectedItems.Cast<DeckItem>().ToList();

            if (!selectedDecks.Any())
            {
                this.StatusText.Text = "Оберіть хоча б одну колоду карток.";
                this.StatusText.Foreground = Brushes.Red;
                return;
            }

            this.StatusText.Text = "Створення тесту...";
            this.StatusText.Foreground = Brushes.Gray;

            try
            {
                var selectedTags = selectedDecks.Select(d => d.TagsStr).ToList();
                var flashcardIds = this.allCards
                    .Where(c => c.Tags.Any() && selectedTags.Contains(c.Tags.First()))
                    .Select(c => c.Id)
                    .ToList();

                if (!flashcardIds.Any())
                {
                    this.StatusText.Text = "Не вдалося знайти картки для обраних колод.";
                    this.StatusText.Foreground = Brushes.Red;
                    return;
                }

                await this._testService.GenerateFromFlashcardsAsync(this._authContext.CurrentUserId, flashcardIds, CancellationToken.None);

                this.StatusText.Text = "Тест успішно згенеровано!";
                this.StatusText.Foreground = Brushes.Green;

                if (NavigationService.GetNavigationService(this).CanGoBack)
                {
                    NavigationService.GetNavigationService(this).GoBack();
                }
            }
            catch (ArgumentException ex)
            {
                this.StatusText.Text = ex.Message;
                this.StatusText.Foreground = Brushes.Red;
            }
            catch (Exception)
            {
                this.StatusText.Text = "Непередбачена помилка при генерації тесту.";
                this.StatusText.Foreground = Brushes.Red;
            }
        }

        private class DeckItem
        {
            public int FlashcardId { get; set; }

            public string TagsStr { get; set; } = string.Empty;

            public DateTime CreatedAt { get; set; }
        }
    }
}