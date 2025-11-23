#pragma warning disable SA1200 // Псевдоніми (alias) мають бути поза namespace
using TestMistake = BrainBurst.Presentation.Views.TestMistake;
#pragma warning restore SA1200

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
    using BrainBurst.BLL.Interfaces;

    /// <summary>
    /// Логіка взаємодії для View проходження тесту.
    /// Керує відображенням питань, збором відповідей та надсиланням результатів.
    /// </summary>
    public partial class TestTakingView : UserControl
    {
        private const int MockTestId = 1001;

        private readonly ITestService _testService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IAuthContext _authContext;
        private readonly List<(int flashcardId, string? userInput)> userAnswers = new ();

        private List<Question> testQuestions = new List<Question>();
        private int currentQuestionIndex = 0;

        private List<TestMistake> mistakesList;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestTakingView"/> class.
        /// </summary>
        /// <param name="testService">Сервіс для надсилання результатів тесту.</param>
        /// <param name="serviceProvider">Постачальник служб DI (для навігації).</param>
        /// <param name="authContext">Контекст автентифікації.</param>
        public TestTakingView(ITestService testService, IServiceProvider serviceProvider, IAuthContext authContext)
        {
            this.InitializeComponent();

            this._testService = testService;
            this._serviceProvider = serviceProvider;
            this._authContext = authContext;

            this.mistakesList = new List<TestMistake>();

            this.LoadDummyQuestions();
            this.LoadQuestion(this.currentQuestionIndex);
        }

        private void LoadDummyQuestions()
        {
            this.testQuestions = new List<Question>
            {
                new Question { FlashcardId = 10, Topic = "Математика", Text = "Скільки буде 2 + 2?", Answer = "4" },
                new Question { FlashcardId = 11, Topic = "Географія", Text = "Столиця України?", Answer = "київ" },
                new Question { FlashcardId = 12, Topic = "Історія", Text = "В якому році хрестили Русь?", Answer = "988" },
            };
        }

        private void LoadQuestion(int index)
        {
            if (index < this.testQuestions.Count)
            {
                Question q = this.testQuestions[index];
                this.QuestionTopic.Text = q.Topic;
                this.QuestionText.Text = q.Text;
                this.QuestionProgress.Text = $"Питання {index + 1} / {this.testQuestions.Count}";
                this.AnswerTopic.Text = q.Topic;
                this.CorrectAnswerText.Text = q.Answer;

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
                Question currentQuestion = this.testQuestions[this.currentQuestionIndex];
                string correctAnswer = currentQuestion.Answer;

                this.userAnswers.Add((currentQuestion.FlashcardId, userAnswer));

                this.UserAnswerText.Text = string.IsNullOrWhiteSpace(userAnswer) ? "[Немає відповіді]" : userAnswer;

                if (userAnswer.ToLower().Trim() == correctAnswer.ToLower().Trim())
                {
                    this.ResultIcon.Text = "✅";
                    this.ResultIcon.Foreground = Brushes.Green;
                    this.AnswerCard.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#F0FFF0") !;
                }
                else
                {
                    this.ResultIcon.Text = "❌";
                    this.ResultIcon.Foreground = Brushes.Red;
                    this.AnswerCard.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#FFF0F0") !;

                    this.mistakesList.Add(new TestMistake
                    {
                        QuestionText = currentQuestion.Text,
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
            this.currentQuestionIndex++;

            if (this.currentQuestionIndex < this.testQuestions.Count)
            {
                this.LoadQuestion(this.currentQuestionIndex);
            }
            else
            {
                try
                {
                    var testResultDto = await this._testService.SubmitAsync(
                        MockTestId,
                        this._authContext.CurrentUserId,
                        this.userAnswers,
                        CancellationToken.None);

                    if (NavigationService.GetNavigationService(this) != null)
                    {
                        var totalQuestions = this.testQuestions.Count;
                        var finalMistakes = testResultDto.Questions
                            .Where(q => !q.IsCorrect)
                            .Select(q => new TestMistake
                            {
                                QuestionText = this.testQuestions.First(t => t.FlashcardId == q.FlashcardId).Text,
                                UserAnswer = q.UserInput,
                                CorrectAnswer = this.testQuestions.First(t => t.FlashcardId == q.FlashcardId).Answer,
                            }).ToList();

                        NavigationService.GetNavigationService(this).Navigate(new TestResultsView(finalMistakes, totalQuestions));
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка надсилання результатів: {ex.Message}", "Помилка");
                    this.StopTest_Click(sender, e);
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

        private class Question
        {
            public int FlashcardId { get; set; }

            public string Topic { get; set; } = string.Empty;

            public string Text { get; set; } = string.Empty;

            public string Answer { get; set; } = string.Empty;
        }
    }
}