using System.Windows;
using System.Windows.Controls;
using System.Windows.Input; 
using System.Windows.Media;

namespace BrainBurst.Presentation.Views
{
    public partial class StudyView : UserControl
    {
        private string currentCorrectAnswer = "Множина вершин і ребер";

        public StudyView()
        {
            InitializeComponent();
            AnswerTextBox.Focus();
        }

        private void AnswerTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string userAnswer = AnswerTextBox.Text;

                UserAnswerText.Text = userAnswer;
                CorrectAnswerText.Text = currentCorrectAnswer;

                if (userAnswer.ToLower().Contains("вершин")) 
                {
                    ResultIcon.Text = "✅";
                    ResultIcon.Foreground = Brushes.Green;
                    AnswerCard.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#F0FFF0"));
                }
                else
                {
                    ResultIcon.Text = "❌";
                    ResultIcon.Foreground = Brushes.Red;
                    AnswerCard.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFF0F0"));
                }

                QuestionCard.Visibility = Visibility.Collapsed;
                AnswerCard.Visibility = Visibility.Visible;
            }
        }

        private void NextCard_Click(object sender, RoutedEventArgs e)
        {
            AnswerCard.Visibility = Visibility.Collapsed;
            QuestionCard.Visibility = Visibility.Visible;

            AnswerTextBox.Text = "";

            AnswerTextBox.Focus();
        }
    }
}