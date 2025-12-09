namespace BrainBurst.Presentation.Views
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Navigation;
    using BrainBurst.BLL.DTO;
    using BrainBurst.BLL.Interfaces;
    using BrainBurst.DAL.Abstractions;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Логіка взаємодії для відображення архіву пройдених тестів користувача.
    /// </summary>
    public partial class ArchiveView : UserControl
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IArchiveService _archiveService;
        private readonly IAuthContext _authContext;
        private readonly ILogger<ArchiveView> _logger;

        private IReadOnlyList<ArchiveEntryDTO> _archiveEntries = new List<ArchiveEntryDTO>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ArchiveView"/> class.
        /// </summary>
        /// <param name="serviceProvider">Постачальник служб DI.</param>
        /// <param name="archiveService">Сервіс для отримання архівних даних.</param>
        /// <param name="authContext">Контекст автентифікації для отримання ID поточного користувача.</param>
        /// <param name="logger">Логер для запису подій.</param>
        public ArchiveView(IServiceProvider serviceProvider, IArchiveService archiveService, IAuthContext authContext, ILogger<ArchiveView> logger)
        {
            this.InitializeComponent();
            this._serviceProvider = serviceProvider;
            this._archiveService = archiveService;
            this._authContext = authContext;
            this._logger = logger;

            this._logger.LogDebug("ArchiveView: View ініціалізовано.");
            this.Loaded += this.ArchiveView_Loaded;
        }

        private async void ArchiveView_Loaded(object sender, RoutedEventArgs e) // <--- ДОДАНО async void
        {
            this._logger.LogInformation("ArchiveView_Loaded: Запуск завантаження архіву тестів.");

            try
            {
                await this.LoadArchiveAsync();
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "ArchiveView_Loaded: Критична помилка при завантаженні архіву.");
                MessageBox.Show($"Не вдалося завантажити архів: {ex.Message}", "Помилка");
            }
        }

        private async Task LoadArchiveAsync()
        {
            try
            {
                this._archiveEntries = await this._archiveService.GetArchiveAsync(this._authContext.CurrentUserId, CancellationToken.None);
                this.ArchiveItemsControl.ItemsSource = this._archiveEntries;

                this._logger.LogInformation("LoadArchiveAsync: Успішно завантажено {Count} архівних записів.", this._archiveEntries.Count);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "LoadArchiveAsync: Критична помилка під час отримання даних архіву.");
                throw;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.GoBack();
        }

        private async void ResultItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int testResultId)
            {
                this._logger.LogInformation("ResultItem_Click: Користувач клікнув на результат тесту ID: {TestResultId}", testResultId);
                try
                {
                    // 1. Знаходимо вибраний елемент у списку (для заголовка/дати)
                    var selectedResult = this._archiveEntries.FirstOrDefault(x => x.TestResultId == testResultId);

                    if (selectedResult == null)
                    {
                        this._logger.LogWarning("ResultItem_Click: Результат тесту ID {TestResultId} не знайдено в локальному списку.", testResultId);
                        return;
                    }

                    // 2. Отримуємо репозиторії через DI для завантаження деталей
                    var resultRepo = this._serviceProvider.GetRequiredService<ITestResultRepository>();
                    var flashcardService = this._serviceProvider.GetRequiredService<IFlashcardService>();

                    // 3. Завантажуємо повну історію результатів, щоб знайти деталі відповідей
                    // (Оптимізація: в ідеалі додати метод GetByIdAsync в ITestResultRepository, але поки використовуємо GetByUserAsync)
                    var allResults = await resultRepo.GetByUserAsync(this._authContext.CurrentUserId, CancellationToken.None);
                    var fullResultEntity = allResults.FirstOrDefault(r => r.TestResultId == testResultId);

                    if (fullResultEntity == null)
                    {
                        MessageBox.Show("Деталі тесту не знайдено в базі даних.", "Помилка");
                        return;
                    }

                    // 4. Формуємо список помилок на основі QuestionResults
                    var mistakes = new List<TestMistake>();
                    int totalQuestions = fullResultEntity.QuestionResults.Count;

                    // Знаходимо тільки неправильні відповіді
                    var wrongAnswers = fullResultEntity.QuestionResults.Where(q => !q.IsCorrect).ToList();

                    foreach (var wrong in wrongAnswers)
                    {
                        // Підвантажуємо текст питання та правильну відповідь з сервісу карток
                        var card = await flashcardService.GetAsync(wrong.FlashcardId, CancellationToken.None);

                        mistakes.Add(new TestMistake
                        {
                            QuestionText = card?.Question ?? "[Картка видалена]",
                            UserAnswer = wrong.UserInput,
                            CorrectAnswer = card?.Answer ?? "[Невідомо]",
                        });
                    }

                    this._logger.LogDebug("ResultItem_Click: Сформовано {MistakesCount} помилок для відображення.", mistakes.Count);

                    if (NavigationService.GetNavigationService(this) != null)
                    {
                        this._logger.LogDebug("ResultItem_Click: Навігація до TestResultsView.");

                        // 5. Отримуємо View через DI
                        var resultsView = this._serviceProvider.GetRequiredService<TestResultsView>();

                        // 6. Ініціалізуємо даними
                        resultsView.InitializeResults(mistakes, totalQuestions);

                        // 7. Виконуємо навігацію
                        NavigationService.GetNavigationService(this).Navigate(resultsView);
                    }
                }
                catch (Exception ex)
                {
                    this._logger.LogError(ex, "ResultItem_Click: Критична помилка під час обробки кліку на результат ID: {TestResultId}", testResultId);
                }
            }
        }

        private void GoBack()
        {
            if (NavigationService.GetNavigationService(this) != null &&
                NavigationService.GetNavigationService(this).CanGoBack)
            {
                this._logger.LogDebug("GoBack: Навігація назад.");
                NavigationService.GetNavigationService(this).GoBack();
            }
            else
            {
                this._logger.LogWarning("GoBack: Неможливо повернутися назад (NavigationService не готовий).");
            }
        }
    }
}