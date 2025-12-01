namespace BrainBurst.Presentation.Views
{
    using System;
    using System.Collections.Generic;
    using System.IO;
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
    using Microsoft.Extensions.Logging;
    using Microsoft.Win32;

    /// <summary>
    /// Клас для елемента списку колод у вікні створення.
    /// </summary>
    public class GenerationDeckItem
    {
        public int FlashcardId { get; set; }

        public string TagsStr { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }

    public partial class CreateTestView : UserControl
    {
        private readonly IFlashcardService _flashcardService;
        private readonly ITestService _testService;
        private readonly ITestGenerationService _testGenerationService;
        private readonly IAuthContext _authContext;
        private readonly ILogger<CreateTestView> _logger;

        private IReadOnlyList<FlashcardDTO> _allCards = Array.Empty<FlashcardDTO>();

        public CreateTestView(
            IFlashcardService flashcardService,
            ITestService testService,
            ITestGenerationService testGenerationService,
            IAuthContext authContext,
            ILogger<CreateTestView> logger)
        {
            this.InitializeComponent();
            this._flashcardService = flashcardService;
            this._testService = testService;
            this._testGenerationService = testGenerationService;
            this._authContext = authContext;
            this._logger = logger;

            this._logger.LogDebug("CreateTestView: View ініціалізовано.");

            this.Loaded += this.CreateTestView_Loaded;
        }

        private async void CreateTestView_Loaded(object sender, RoutedEventArgs e)
        {
            this._logger.LogInformation("CreateTestView_Loaded: Початок завантаження доступних колод.");

            try
            {
                await this.LoadDecksAsync();
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "CreateTestView_Loaded: Критична помилка під час завантаження колод.");
                this.StatusText.Text = $"Помилка: {ex.Message}";
                this.StatusText.Foreground = Brushes.Red;
            }
        }

        private async Task LoadDecksAsync()
        {
            this._logger.LogDebug("LoadDecksAsync: Запуск отримання карток користувача {UserId}.", this._authContext.CurrentUserId);

            this.StatusText.Text = "Завантаження...";
            this.StatusText.Foreground = Brushes.Gray;

            try
            {
                this._allCards = await this._flashcardService.ListAsync(this._authContext.CurrentUserId, null, CancellationToken.None);

                var decks = this._allCards
                    .Where(c => c.Tags != null && c.Tags.Any())
                    .GroupBy(c => c.Tags.First())
                    .Select(g => new GenerationDeckItem
                    {
                        FlashcardId = g.First().Id,
                        TagsStr = g.Key,
                        CreatedAt = g.Min(c => c.CreatedAt),
                    })
                    .OrderBy(d => d.TagsStr)
                    .ToList();

                this.DeckListBox.ItemsSource = decks;

                if (decks.Any())
                {
                    this.StatusText.Text = "";
                    this._logger.LogInformation("LoadDecksAsync: Знайдено {DeckCount} колод для створення тесту.", decks.Count);
                }
                else
                {
                    this.StatusText.Text = "Картки не знайдено. Створіть їх спочатку.";
                    this.StatusText.Foreground = Brushes.Red;
                    this._logger.LogWarning("LoadDecksAsync: Картки не знайдені, показано повідомлення про відсутність.");
                }
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "LoadDecksAsync: Критична помилка під час отримання або групування карток.");
                throw;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.GetNavigationService(this)?.CanGoBack == true)
            {
                NavigationService.GetNavigationService(this).GoBack();
            }
        }

        private async void GenerateTest_Click(object sender, RoutedEventArgs e)
        {
            this._logger.LogInformation("GenerateTest_Click: Запущено створення тесту з вибраних колод.");

            try
            {
                var selectedDecks = this.DeckListBox.SelectedItems.Cast<GenerationDeckItem>().ToList();

                if (!selectedDecks.Any())
                {
                    this.StatusText.Text = "Оберіть хоча б одну колоду.";
                    this.StatusText.Foreground = Brushes.Red;
                    this._logger.LogWarning("GenerateTest_Click: Не обрано жодної колоди.");
                    return;
                }

                this.StatusText.Text = "Створення тесту...";

                var selectedTags = selectedDecks.Select(d => d.TagsStr).ToList();
                this._logger.LogDebug("GenerateTest_Click: Вибрані теги: {Tags}", string.Join(", ", selectedTags));

                var flashcardIds = this._allCards
                    .Where(c => c.Tags.Any() && selectedTags.Contains(c.Tags.First()))
                    .Select(c => c.Id)
                    .ToList();

                this._logger.LogDebug("GenerateTest_Click: Зібрано {Count} ID карток для тесту.", flashcardIds.Count);

                await this._testService.GenerateFromFlashcardsAsync(this._authContext.CurrentUserId, flashcardIds, CancellationToken.None);

                this.StatusText.Text = "Тест успішно створено!";
                this.StatusText.Foreground = Brushes.Green;
                this._logger.LogInformation("GenerateTest_Click: Тест успішно створено з {Count} карток.", flashcardIds.Count);

                await Task.Delay(1000);
                this.BackButton_Click(sender, e);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "GenerateTest_Click: Критична помилка під час генерації тесту з колод.");
                this.StatusText.Text = $"Помилка: {ex.Message}";
                this.StatusText.Foreground = Brushes.Red;
            }
        }

        private async void ImportFile_Click(object sender, RoutedEventArgs e)
        {
            this._logger.LogInformation("ImportFile_Click: Запущено імпорт файлу для AI генерації тесту.");

            // Перевіряємо, чи введена назва тесту
            string testName = this.TestNameTextBox.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(testName))
            {
                this.StatusText.Text = "Спочатку введіть назву тесту.";
                this.StatusText.Foreground = Brushes.Orange;
                this._logger.LogWarning("ImportFile_Click: Не введена назва тесту.");
                return;
            }

            var openFileDialog = new OpenFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                Title = "Оберіть текстовий файл",
            };

            if (openFileDialog.ShowDialog() == true)
            {
                this._logger.LogDebug("ImportFile_Click: Користувач обрав файл: {FileName}", openFileDialog.FileName);

                try
                {
                    this.StatusText.Text = "Генерація питань...";
                    string text = await File.ReadAllTextAsync(openFileDialog.FileName);

                    this._logger.LogDebug("ImportFile_Click: Зчитування тексту довжиною {TextLength}.", text.Length);

                    var newCards = await this._testGenerationService.CreateFlashcardsFromTextAsync(
                        this._authContext.CurrentUserId,
                        text,
                        new List<string> { testName },
                        CancellationToken.None);

                    this.StatusText.Text = $"Згенеровано {newCards.Count} карток з тегом '{testName}'!";
                    this.StatusText.Foreground = Brushes.Green;
                    this._logger.LogInformation("ImportFile_Click: AI успішно згенерував {Count} карток.", newCards.Count);

                    await this.LoadDecksAsync();
                }
                catch (IOException ex)
                {
                    this._logger.LogError(ex, "ImportFile_Click: Помилка доступу до файлу або читання.");
                    this.StatusText.Text = $"Помилка файлу: {ex.Message}";
                    this.StatusText.Foreground = Brushes.Red;
                }
                catch (Exception ex)
                {
                    this._logger.LogError(ex, "ImportFile_Click: Критична помилка AI генерації або збереження.");
                    this.StatusText.Text = $"Помилка: {ex.Message}";
                    this.StatusText.Foreground = Brushes.Red;
                }
            }
        }
    }
}