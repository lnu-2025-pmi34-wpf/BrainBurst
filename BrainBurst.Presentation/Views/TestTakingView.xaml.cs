using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using BrainBurst.BLL.Interfaces; 
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

// Додаємо using для TestMistake з того ж простору імен
using TestMistake = BrainBurst.Presentation.Views.TestMistake; 

namespace BrainBurst.Presentation.Views
{
    public partial class TestTakingView : UserControl
    {
        // 🚨 ТИМЧАСОВО: Фіктивний ID для тесту
        private const int MockTestId = 1001; 

        private readonly ITestService _testService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IAuthContext _authContext; 
        
        // --- ВИДАЛЕНО ЛОКАЛЬНЕ ВИЗНАЧЕННЯ TestMistake, ВИКОРИСТОВУЄТЬСЯ З TestResultsView.xaml.cs ---

        private List<Question> testQuestions;
        private int currentQuestionIndex = 0;

        private List<TestMistake> mistakesList;
        // Список для зберігання відповідей користувача для надсилання
        private readonly List<(int flashcardId, string userInput)> userAnswers = new();

        // Внутрішня структура для питань
        private class Question
        {
            public int FlashcardId { get; set; } // ID картки для SubmitAsync
            public string Topic { get; set; }
            public string Text { get; set; }
            public string Answer { get; set; }
        }

        public TestTakingView(ITestService testService, IServiceProvider serviceProvider, IAuthContext authContext)
        {
            InitializeComponent();
            
            _testService = testService;
            _serviceProvider = serviceProvider;
            _authContext = authContext;

            mistakesList = new List<TestMistake>();

            LoadDummyQuestions(); // Завантажуємо хардкодовані питання
            LoadQuestion(currentQuestionIndex);
        }

        private void LoadDummyQuestions()
        {
            // Використовуємо реальну структуру з FlashcardId
            testQuestions = new List<Question>
            {
                new Question { FlashcardId = 10, Topic = "Математика", Text = "Скільки буде 2 + 2?", Answer = "4" },
                new Question { FlashcardId = 11, Topic = "Географія", Text = "Столиця України?", Answer = "київ" },
                new Question { FlashcardId = 12, Topic = "Історія", Text = "В якому році хрестили Русь?", Answer = "988" }
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

                // 1. Додаємо відповідь користувача до списку
                userAnswers.Add((currentQuestion.FlashcardId, userAnswer));

                UserAnswerText.Text = string.IsNullOrWhiteSpace(userAnswer) ? "[Немає відповіді]" : userAnswer;
                
                // 2. Логіка порівняння та збору помилок
                if (userAnswer.ToLower().Trim() == correctAnswer.ToLower().Trim())
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

                    // Зберігаємо помилку для UI TestResultsView
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

        private async void NextCard_Click(object sender, RoutedEventArgs e)
        {
            currentQuestionIndex++;

            if (currentQuestionIndex < testQuestions.Count)
            {
                LoadQuestion(currentQuestionIndex);
            }
            else
            {
                // Кінець тесту
                try
                {
                    // 3. НАДСИЛАННЯ РЕЗУЛЬТАТІВ ЧЕРЕЗ СЕРВІС
                    var testResultDto = await _testService.SubmitAsync(
                        MockTestId, 
                        _authContext.CurrentUserId,
                        userAnswers, 
                        CancellationToken.None);

                    // 4. Перехід до результатів
                    if (NavigationService.GetNavigationService(this) != null)
                    {
                        var totalQuestions = testQuestions.Count;
                        var finalMistakes = testResultDto.Questions
                            .Where(q => !q.IsCorrect)
                            .Select(q => new TestMistake
                            {
                                QuestionText = testQuestions.First(t => t.FlashcardId == q.FlashcardId).Text,
                                UserAnswer = q.UserInput,
                                CorrectAnswer = testQuestions.First(t => t.FlashcardId == q.FlashcardId).Answer,
                            }).ToList();
                            
                        // Тепер тип finalMistakes коректно відповідає очікуваному типу TestResultsView
                        NavigationService.GetNavigationService(this).Navigate(new TestResultsView(finalMistakes, totalQuestions));
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка надсилання результатів: {ex.Message}", "Помилка");
                    // Якщо сталася помилка, просто повертаємося
                    StopTest_Click(sender, e);
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