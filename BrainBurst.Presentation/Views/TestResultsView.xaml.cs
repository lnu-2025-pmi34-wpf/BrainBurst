namespace BrainBurst.Presentation.Views
{
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Navigation;

    public class TestMistake
    {
        public string QuestionText { get; set; }

        public string UserAnswer { get; set; }

        public string CorrectAnswer { get; set; }
    }

    public partial class TestResultsView : UserControl
    {
        public TestResultsView()
        {
            this.InitializeComponent();
            this.ScoreText.Text = "Виникла помилка завантаження результатів";
        }

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
            double percentage = ((double)score / totalQuestions) * 100;

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