using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;

namespace BrainBurst.Presentation.Views
{
    public partial class TestTakingView : UserControl
    {
        // === "Фейкова" база даних питань ===
        private List<Question> testQuestions;
        private int currentQuestionIndex = 0;

        // === СПИСОК ДЛЯ ЗБЕРІГАННЯ ПОМИЛОК ===
        private List<TestMistake> mistakesList;

        // Внутрішній клас для зберігання питання
        private class Question
        {
            public string Topic { get; set; }
            public string Text { get; set; }
            public string Answer { get; set; }
        }

        public TestTakingView()
        {
            InitializeComponent();

            // Створюємо НОВИЙ список помилок для цього тесту
            mistakesList = new List<TestMistake>();

            LoadDummyQuestions();
            LoadQuestion(currentQuestionIndex);
        }

        private void LoadDummyQuestions()
        {
            testQuestions = new List<Question>
            {
                new Question { Topic = "Математика", Text = "Скільки буде 2 + 2?", Answer = "4" },
                new Question { Topic = "Географія", Text = "Столиця України?", Answer = "київ" },
                new Question { Topic = "Історія", Text = "В якому році хрестили Русь?", Answer = "988" }
            };
        }

        private void LoadQuestion(int index)
        {
            if (index < testQuestions.Count)
            {
                Question q = testQuestions[index];
                QuestionTopic.Text = q.Topic;
                QuestionText.Text = q.Text;
                QuestionProgress.Text = $"Питання {index + 1} / {testQuestions.Count}";
                AnswerTopic.Text = q.Topic;
                CorrectAnswerText.Text = q.Answer;

                AnswerCard.Visibility = Visibility.Collapsed;
                QuestionCard.Visibility = Visibility.Visible;
                AnswerTextBox.Text = "";
                AnswerTextBox.Focus();
            }
        }

        private void AnswerTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && QuestionCard.Visibility == Visibility.Visible)
            {
                string userAnswer = AnswerTextBox.Text;
                Question currentQuestion = testQuestions[currentQuestionIndex];
                string correctAnswer = currentQuestion.Answer;

                UserAnswerText.Text = string.IsNullOrWhiteSpace(userAnswer) ? "[Немає відповіді]" : userAnswer;

                if (userAnswer.ToLower().Trim() == correctAnswer.ToLower().Trim())
                {
                    // ПРАВИЛЬНО
                    ResultIcon.Text = "✅";
                    ResultIcon.Foreground = Brushes.Green;
                    AnswerCard.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#F0FFF0"));
                }
                else
                {
                    // НЕПРАВИЛЬНО
                    ResultIcon.Text = "❌";
                    ResultIcon.Foreground = Brushes.Red;
                    AnswerCard.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFF0F0"));

                    // === ДОДАЄМО ПОМИЛКУ ДО СПИСКУ ===
                    mistakesList.Add(new TestMistake
                    {
                        QuestionText = currentQuestion.Text,
                        UserAnswer = userAnswer,
                        CorrectAnswer = correctAnswer
                    });
                }

                QuestionCard.Visibility = Visibility.Collapsed;
                AnswerCard.Visibility = Visibility.Visible;
            }
        }

        private void NextCard_Click(object sender, RoutedEventArgs e)
        {
            currentQuestionIndex++;

            if (currentQuestionIndex < testQuestions.Count)
            {
                LoadQuestion(currentQuestionIndex);
            }
            else
            {
                // Тест закінчено, переходимо на сторінку результатів
                if (NavigationService.GetNavigationService(this) != null)
                {
                    // === ПЕРЕДАЄМО СПИСОК ПОМИЛОК І ЗАГАЛЬНУ КІЛЬКІСТЬ ===
                    NavigationService.GetNavigationService(this).Navigate(new TestResultsView(mistakesList, testQuestions.Count));
                }
            }
        }

        private void StopTest_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.GetNavigationService(this).CanGoBack)
            {
                NavigationService.GetNavigationService(this).GoBack();
            }
        }
    }
}