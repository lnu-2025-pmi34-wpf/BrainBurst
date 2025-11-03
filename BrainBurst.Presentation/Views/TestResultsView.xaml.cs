using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;

namespace BrainBurst.Presentation.Views
{
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
            InitializeComponent();
            ScoreText.Text = "Виникла помилка завантаження результатів";
        }

        public TestResultsView(List<TestMistake> mistakes, int totalQuestions)
        {
            InitializeComponent();

            if (totalQuestions <= 0)
            {
                ScoreText.Text = "Немає даних для відображення";
                PercentageText.Text = "-";
                return;
            }

            int score = totalQuestions - mistakes.Count;
            double percentage = ((double)score / totalQuestions) * 100;

            ScoreText.Text = $"Ваш результат: {score} / {totalQuestions}";
            PercentageText.Text = $"{percentage:F0}%";

            if (percentage < 50)
            {
                PercentageText.Foreground = Brushes.Red;
            }
            else if (percentage < 80)
            {
                PercentageText.Foreground = Brushes.Orange;
            }
            else
            {
                PercentageText.Foreground = Brushes.Green;
            }

            if (mistakes.Count > 0)
            {
                MistakesHeader.Visibility = Visibility.Visible;
                MistakesItemsControl.ItemsSource = mistakes;
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