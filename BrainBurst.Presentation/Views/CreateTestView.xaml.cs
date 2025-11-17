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
    using BrainBurst.BLL.Interfaces; // Додано для IAuthContext
    using Microsoft.Extensions.DependencyInjection;

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
            this.InitializeComponent();
            this._flashcardService = flashcardService;
            this._testService = testService;
            this._authContext = authContext; // <--- ІНІЦІАЛІЗОВАНО

            // Завантажуємо дані при завантаженні UI
            this.Loaded += this.CreateTestView_Loaded;
        }

        private void CreateTestView_Loaded(object sender, RoutedEventArgs e)
        {
            this.LoadDecksAsync();
        }

        // Завантажує всі картки та групує їх, імітуючи колоди
        private async Task LoadDecksAsync()
        {
            try
            {
                this.StatusText.Text = "Завантаження карток...";
                this.StatusText.Foreground = Brushes.Gray;

                // ВИКОРИСТАННЯ: CurrentUserId замінено на _authContext.CurrentUserId
                this.AllCards = await this._flashcardService.ListAsync(this._authContext.CurrentUserId, null, CancellationToken.None);

                // Групуємо картки за першим тегом (імітація колод)
                var decks = this.AllCards
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
                // 1. Отримуємо ID всіх карток, що належать обраним "колодам"
                var selectedTags = selectedDecks.Select(d => d.TagsStr).ToList();
                var flashcardIds = this.AllCards
                    .Where(c => c.Tags.Any() && selectedTags.Contains(c.Tags.First()))
                    .Select(c => c.Id)
                    .ToList();

                if (!flashcardIds.Any())
                {
                    this.StatusText.Text = "Не вдалося знайти картки для обраних колод.";
                    this.StatusText.Foreground = Brushes.Red;
                    return;
                }

                // 2. Генеруємо тест
                // ВИКОРИСТАННЯ: CurrentUserId замінено на _authContext.CurrentUserId
                await this._testService.GenerateFromFlashcardsAsync(this._authContext.CurrentUserId, flashcardIds, CancellationToken.None);

                // 3. Успіх: повертаємось на попередній екран (TestsView).
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
    }
}