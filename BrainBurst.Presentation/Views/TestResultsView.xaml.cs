namespace BrainBurst.Presentation.Views
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Navigation;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Внутрішній клас, що представляє одну помилку (неправильну відповідь) у тесті.
    /// </summary>
#pragma warning disable SA1402
#pragma warning disable SA1649
    public class TestMistake
#pragma warning restore SA1649
#pragma warning restore SA1402
    {
        /// <summary>
        /// Gets or sets отримує або встановлює текст питання.
        /// </summary>
        public string QuestionText { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets отримує або встановлює відповідь, яку надав користувач.
        /// </summary>
        public string UserAnswer { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets отримує або встановлює правильну відповідь.
        /// </summary>
        public string CorrectAnswer { get; set; } = string.Empty;
    }

    /// <summary>
    /// Логіка взаємодії для View відображення детальних результатів пройденого тесту.
    /// </summary>
    public partial class TestResultsView : UserControl
    {
        private readonly ILogger<TestResultsView> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestResultsView"/> class.
        /// </summary>
        /// <param name="logger">Логер для запису подій.</param>
        public TestResultsView(ILogger<TestResultsView> logger)
        {
            this.InitializeComponent();
            this._logger = logger;
            this.ScoreText.Text = "Виникла помилка завантаження результатів";
            this._logger.LogWarning("TestResultsView: Ініціалізовано через порожній конструктор. Дані відсутні.");
        }

        /// <summary>
        /// Встановлює результати тесту після отримання View з контейнера DI.
        /// Цей метод замінює старий конструктор з даними.
        /// </summary>
        /// <param name="mistakes">Список помилок (неправильних відповідей) користувача.</param>
        /// <param name="totalQuestions">Загальна кількість питань у тесті.</param>
        public void InitializeResults(List<TestMistake> mistakes, int totalQuestions)
        {
            this._logger.LogInformation("InitializeResults: Відображення результатів. Всього: {Total}, Помилок: {Mistakes}", totalQuestions, mistakes.Count);

            if (totalQuestions <= 0)
            {
                this.ScoreText.Text = "Немає даних для відображення";
                this.PercentageText.Text = "-";
                this._logger.LogWarning("InitializeResults: Некоректна кількість питань ({Total}).", totalQuestions);
                return;
            }

            int score = totalQuestions - mistakes.Count;
            double percentage = (double)score / totalQuestions * 100;

            this.ScoreText.Text = $"Ваш результат: {score} / {totalQuestions}";
            this.PercentageText.Text = $"{percentage:F0}%";

            this._logger.LogInformation("InitializeResults: Фінальний результат: {Score}/{Total} ({Percent:F0}%)", score, totalQuestions, percentage);

            // Логіка встановлення кольору
            if (percentage < 50)
            {
                this.PercentageText.Foreground = Brushes.Red;
            }
            else if (percentage < 80)
            {
                this.PercentageText.Foreground = Brushes.Orange;
            }
            else
            {
                this.PercentageText.Foreground = Brushes.Green;
            }

            if (mistakes.Count > 0)
            {
                this.MistakesHeader.Visibility = Visibility.Visible;
                this.MistakesItemsControl.ItemsSource = mistakes;
                this._logger.LogDebug("InitializeResults: Показано список помилок.");
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.GetNavigationService(this) != null &&
                NavigationService.GetNavigationService(this).CanGoBack)
            {
                this._logger.LogInformation("BackButton_Click: Повернення до попереднього View.");
                NavigationService.GetNavigationService(this).GoBack();
            }
            else
            {
                this._logger.LogWarning("BackButton_Click: Навігація неможлива.");
            }
        }
    }
}