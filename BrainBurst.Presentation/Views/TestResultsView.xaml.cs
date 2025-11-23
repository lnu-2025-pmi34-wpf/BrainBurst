namespace BrainBurst.Presentation.Views
{
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Navigation;

    /// <summary>
    /// Внутрішній клас, що представляє одну помилку (неправильну відповідь) у тесті.
    /// </summary>
#pragma warning disable SA1402 // File may only contain a single type
#pragma warning disable SA1649 // File name should match first type name
    public class TestMistake
#pragma warning restore SA1649 // File name should match first type name
#pragma warning restore SA1402 // File may only contain a single type
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
        /// <summary>
        /// Initializes a new instance of the <see cref="TestResultsView"/> class.
        /// </summary>
        public TestResultsView()
        {
            this.InitializeComponent();
            this.ScoreText.Text = "Виникла помилка завантаження результатів";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestResultsView"/> class.
        /// </summary>
        /// <param name="mistakes">Список помилок (неправильних відповідей) користувача.</param>
        /// <param name="totalQuestions">Загальна кількість питань у тесті.</param>
        public TestResultsView(List<TestMistake> mistakes, int totalQuestions)
        {
            this.InitializeComponent();

            if (totalQuestions <= 0)
            {
                this.ScoreText.Text = "Немає даних для відображення";
                this.PercentageText.Text = "-";
                return;
            }

            int score = totalQuestions - mistakes.Count;
            double percentage = (double)score / totalQuestions * 100;

            this.ScoreText.Text = $"Ваш результат: {score} / {totalQuestions}";
            this.PercentageText.Text = $"{percentage:F0}%";

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
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.GetNavigationService(this) != null &&
                NavigationService.GetNavigationService(this).CanGoBack)
            {
                NavigationService.GetNavigationService(this).GoBack();
            }
        }
    }
}