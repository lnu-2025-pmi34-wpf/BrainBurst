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
    using BrainBurst.BLL.DTO;
    using BrainBurst.BLL.Interfaces;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Логіка взаємодії для View проходження тесту.
    /// </summary>
    public partial class TestTakingView : UserControl
    {
        private readonly ITestService _testService;
        private readonly IFlashcardService _flashcardService;
        private readonly IAuthContext _authContext;
        private readonly ILogger<TestTakingView> _logger;
        private readonly IServiceProvider _serviceProvider;

        private List<FlashcardDTO> _questions = new List<FlashcardDTO>();
        private List<(int flashcardId, string? userInput)> _userAnswers = new List<(int, string?)>();
        private List<TestMistake> _mistakesList = new List<TestMistake>();

        private int _currentQuestionIndex = 0;
        private int _currentTestId = 0; // ID тесту, який створиться в БД

        /// <summary>
        /// Initializes a new instance of the <see cref="TestTakingView"/> class.
        /// </summary>
        /// <param name="testService">Сервіс для роботи з тестами.</param>
        /// <param name="flashcardService">Сервіс для завантаження карток.</param>
        /// <param name="authContext">Контекст автентифікації.</param>
        /// <param name="logger">Логер для запису подій.</param>
        /// <param name="serviceProvider">Провайдер сервісів для отримання інших View.</param>
        public TestTakingView(
            ITestService testService,
            IFlashcardService flashcardService,
            IAuthContext authContext,
            ILogger<TestTakingView> logger,
            IServiceProvider serviceProvider)
        {
            this.InitializeComponent();
            this._testService = testService;
            this._flashcardService = flashcardService;
            this._authContext = authContext;
            this._logger = logger;
            this._serviceProvider = serviceProvider;

            this._logger.LogDebug("TestTakingView: View ініціалізовано.");
        }

        /// <summary>
        /// Налаштовує та запускає тест по конкретній темі (тегу).
        /// </summary>
        /// <param name="tag">Назва тегу колоди.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task InitializeTestByTagAsync(string tag)
        {
            this._logger.LogInformation("InitializeTestByTagAsync: Запуск ініціалізації тесту для тегу: {Tag}", tag);

            try
            {
                this.QuestionText.Text = "Генерація тесту...";

                // 1. Отримуємо всі картки юзера
                var allCards = await this._flashcardService.ListAsync(this._authContext.CurrentUserId, null, CancellationToken.None);
                this._logger.LogDebug("InitializeTestByTagAsync: Отримано {Count} карток для фільтрації.", allCards.Count);

                // 2. Фільтруємо за тегом
                var filteredCards = allCards
                    .Where(c => c.Tags.Contains(tag))
                    .Select(c => c.Id)
                    .ToList();

                if (!filteredCards.Any())
                {
                    this._logger.LogWarning("InitializeTestByTagAsync: У колоді {Tag} немає карток. Скасування.", tag);
                    MessageBox.Show("У цій колоді немає карток.", "Увага");
                    this.StopTest_Click(this, new RoutedEventArgs());
                    return;
                }

                // 3. Створюємо запис тесту в БД
                var testDto = await this._testService.GenerateFromFlashcardsAsync(
                    this._authContext.CurrentUserId,
                    filteredCards,
                    CancellationToken.None);

                this._currentTestId = testDto.Id;
                this._questions = testDto.Questions.ToList();
                this._currentQuestionIndex = 0;

                this._logger.LogInformation("InitializeTestByTagAsync: Тест ID {TestId} успішно створено з {Count} питань.", this._currentTestId, this._questions.Count);

                // 4. Показуємо перше питання
                this.LoadQuestion(this._currentQuestionIndex);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "InitializeTestByTagAsync: Критична помилка ініціалізації тесту для тегу: {Tag}", tag);
                MessageBox.Show($"Помилка запуску тесту: {ex.Message}", "Помилка");
                this.StopTest_Click(this, new RoutedEventArgs());
            }
        }

        private void LoadQuestion(int index)
        {
            if (index < this._questions.Count)
            {
                var q = this._questions[index];

                // Встановлюємо тему (перший тег або заглушка)
                this.QuestionTopic.Text = q.Tags.FirstOrDefault() ?? "Тест";
                this.AnswerTopic.Text = this.QuestionTopic.Text;

                this.QuestionText.Text = q.Question;
                this.QuestionProgress.Text = $"Питання {index + 1} / {this._questions.Count}";

                this.CorrectAnswerText.Text = q.Answer;

                // Скидаємо стан UI
                this.AnswerCard.Visibility = Visibility.Collapsed;
                this.QuestionCard.Visibility = Visibility.Visible;
                this.AnswerTextBox.Text = string.Empty;
                this.AnswerTextBox.Focus();
            }
        }

        private void AnswerTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && this.QuestionCard.Visibility == Visibility.Visible)
            {
                string userAnswer = this.AnswerTextBox.Text;
                var currentQuestion = this._questions[this._currentQuestionIndex];
                string correctAnswer = currentQuestion.Answer;

                // Зберігаємо відповідь для фінальної відправки
                this._userAnswers.Add((currentQuestion.Id, userAnswer));

                this.UserAnswerText.Text = string.IsNullOrWhiteSpace(userAnswer) ? "[Немає відповіді]" : userAnswer;

                // Перевірка (проста, без case-sensitive)
                if (userAnswer.Trim().Equals(correctAnswer.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    this._logger.LogDebug("AnswerTextBox_KeyDown: Питання {Index} відповідено ПРАВИЛЬНО.", this._currentQuestionIndex + 1);
                    this.ResultIcon.Text = "✅";
                    this.ResultIcon.Foreground = Brushes.Green;
                    this.AnswerCard.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#F0FFF0") !;
                }
                else
                {
                    this._logger.LogWarning(
                        "AnswerTextBox_KeyDown: Питання {Index} відповідено НЕПРАВИЛЬНО. Користувач: '{UserAnswer}', Правильно: '{CorrectAnswer}'",
                        this._currentQuestionIndex + 1,
                        userAnswer,
                        correctAnswer);
                    this.ResultIcon.Text = "❌";
                    this.ResultIcon.Foreground = Brushes.Red;
                    this.AnswerCard.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#FFF0F0") !;

                    // Додаємо у список помилок для відображення в кінці
                    this._mistakesList.Add(new TestMistake
                    {
                        QuestionText = currentQuestion.Question,
                        UserAnswer = userAnswer,
                        CorrectAnswer = correctAnswer,
                    });
                }

                this.QuestionCard.Visibility = Visibility.Collapsed;
                this.AnswerCard.Visibility = Visibility.Visible;
            }
        }

        private async void NextCard_Click(object sender, RoutedEventArgs e)
        {
            this._currentQuestionIndex++;

            if (this._currentQuestionIndex < this._questions.Count)
            {
                this.LoadQuestion(this._currentQuestionIndex);
                this._logger.LogDebug("NextCard_Click: Завантажено питання {Index}.", this._currentQuestionIndex + 1);
            }
            else
            {
                this._logger.LogInformation("NextCard_Click: Кінець тесту. Запуск відправки результатів (TestId: {TestId}).", this._currentTestId);

                // Кінець тесту - відправляємо результати
                try
                {
                    var testResultDto = await this._testService.SubmitAsync(
                        this._currentTestId,
                        this._authContext.CurrentUserId,
                        this._userAnswers,
                        CancellationToken.None);

                    if (NavigationService.GetNavigationService(this) != null)
                    {
                        var totalQuestions = this._questions.Count;

                        this._logger.LogInformation(
                            "NextCard_Click: Результати успішно збережено. Правильно: {CorrectCount}, Неправильно: {MistakesCount}.",
                            testResultDto.CorrectAnswersPercent,
                            this._mistakesList.Count);

                        // Переходимо на екран результатів
                        var resultsView = this._serviceProvider.GetRequiredService<TestResultsView>();

                        // Ініціалізація даними через метод
                        resultsView.InitializeResults(this._mistakesList, totalQuestions);

                        // Переходимо на екран результатів
                        NavigationService.GetNavigationService(this).Navigate(resultsView);
                    }
                }
                catch (Exception ex)
                {
                    this._logger.LogError(ex, "NextCard_Click: Критична помилка при збереженні результатів тесту {TestId}.", this._currentTestId);
                    MessageBox.Show($"Помилка збереження результатів: {ex.Message}", "Помилка");
                    this.StopTest_Click(sender, e);
                }
            }
        }

        private void StopTest_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.GetNavigationService(this)?.CanGoBack == true)
            {
                NavigationService.GetNavigationService(this).GoBack();
            }
        }
    }
}