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
        
        private IReadOnlyList<FlashcardDTO> _allCards = Array.Empty<FlashcardDTO>();

        public CreateTestView(
            IFlashcardService flashcardService, 
            ITestService testService, 
            ITestGenerationService testGenerationService,
            IAuthContext authContext)
        {
            this.InitializeComponent();
            this._flashcardService = flashcardService;
            this._testService = testService;
            this._testGenerationService = testGenerationService;
            this._authContext = authContext;

            this.Loaded += this.CreateTestView_Loaded;
        }

        private async void CreateTestView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await this.LoadDecksAsync();
            }
            catch (Exception ex)
            {
                this.StatusText.Text = $"Помилка: {ex.Message}";
                this.StatusText.Foreground = Brushes.Red;
            }
        }

        private async Task LoadDecksAsync()
        {
            this.StatusText.Text = "Завантаження...";
            this.StatusText.Foreground = Brushes.Gray;

            this._allCards = await this._flashcardService.ListAsync(this._authContext.CurrentUserId, null, CancellationToken.None);

            var decks = this._allCards
                .Where(c => c.Tags != null && c.Tags.Any())
                .GroupBy(c => c.Tags.First())
                .Select(g => new GenerationDeckItem // Використовуємо нове ім'я класу
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
            }
            else
            {
                this.StatusText.Text = "Картки не знайдено. Створіть їх спочатку.";
                this.StatusText.Foreground = Brushes.Red;
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
            try
            {
                // Приводимо до нового типу
                var selectedDecks = this.DeckListBox.SelectedItems.Cast<GenerationDeckItem>().ToList();

                if (!selectedDecks.Any())
                {
                    this.StatusText.Text = "Оберіть хоча б одну колоду.";
                    this.StatusText.Foreground = Brushes.Red;
                    return;
                }

                this.StatusText.Text = "Створення тесту...";
                
                var selectedTags = selectedDecks.Select(d => d.TagsStr).ToList();
                
                var flashcardIds = this._allCards
                    .Where(c => c.Tags.Any() && selectedTags.Contains(c.Tags.First()))
                    .Select(c => c.Id)
                    .ToList();

                await this._testService.GenerateFromFlashcardsAsync(this._authContext.CurrentUserId, flashcardIds, CancellationToken.None);

                this.StatusText.Text = "Тест успішно створено!";
                this.StatusText.Foreground = Brushes.Green;

                await Task.Delay(1000);
                this.BackButton_Click(sender, e);
            }
            catch (Exception ex)
            {
                this.StatusText.Text = $"Помилка: {ex.Message}";
                this.StatusText.Foreground = Brushes.Red;
            }
        }

        private async void ImportFile_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                Title = "Оберіть текстовий файл"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    this.StatusText.Text = "Генерація питань...";
                    string text = await File.ReadAllTextAsync(openFileDialog.FileName);

                    var newCards = await this._testGenerationService.CreateFlashcardsFromTextAsync(
                        this._authContext.CurrentUserId, 
                        text, 
                        CancellationToken.None);

                    this.StatusText.Text = $"Згенеровано {newCards.Count} карток!";
                    this.StatusText.Foreground = Brushes.Green;

                    await this.LoadDecksAsync();
                }
                catch (Exception ex)
                {
                    this.StatusText.Text = $"Помилка файлу: {ex.Message}";
                    this.StatusText.Foreground = Brushes.Red;
                }
            }
        }
    }
}