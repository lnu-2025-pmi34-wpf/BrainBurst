namespace BrainBurst.Presentation.Views
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using BrainBurst.BLL.DTO;
    using BrainBurst.BLL.Interfaces;

    /// <summary>
    /// Логіка взаємодії для View режиму навчання (вивчення флеш-карток).
    /// </summary>
    public partial class StudyView : UserControl
    {
        // Примітка: CurrentUserId має бути замінено на this._authContext.CurrentUserId
        private const int CurrentUserId = 1;

        private readonly IFlashcardService _flashcardService;
        private List<FlashcardDTO> _flashcards = new List<FlashcardDTO>();
        private int _currentCardIndex = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="StudyView"/> class.
        /// </summary>
        /// <param name="flashcardService">Сервіс для отримання списку флеш-карток.</param>
        public StudyView(IFlashcardService flashcardService)
        {
            this.InitializeComponent();
            this._flashcardService = flashcardService;

            this.Loaded += this.StudyView_Loaded;
        }

        private async void StudyView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await this.LoadCardsAsync();
            }
            catch (Exception)
            {
            }
        }

        private async Task LoadCardsAsync()
        {
            try
            {
                this._flashcards = (await this._flashcardService.ListAsync(CurrentUserId, null, CancellationToken.None)).ToList();

                if (this._flashcards.Any())
                {
                    this._currentCardIndex = 0;
                    this.DisplayCard(this._currentCardIndex);
                }
                else
                {
                    this.QuestionText.Text = "Картки не знайдено. Створіть нову картку!";
                }

                this.AnswerTextBox.Focus();
            }
            catch (Exception)
            {
                this.QuestionText.Text = "Помилка завантаження карток.";
            }
        }

        private void DisplayCard(int index)
        {
            if (index >= 0 && index < this._flashcards.Count)
            {
                var card = this._flashcards[index];

                this.QuestionText.Text = card.Question;

                this.QuestionTopic.Text = card.Tags.FirstOrDefault() ?? "Загальна колода";
                this.AnswerTopic.Text = this.QuestionTopic.Text;

                this.AnswerTextBox.Text = string.Empty;
                this.QuestionCard.Visibility = Visibility.Visible;
                this.AnswerCard.Visibility = Visibility.Collapsed;

                this.AnswerTextBox.Focus();
            }
        }

        private void AnswerTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && this._flashcards.Any() && this.QuestionCard.Visibility == Visibility.Visible)
            {
                string userAnswer = this.AnswerTextBox.Text;
                var currentCard = this._flashcards[this._currentCardIndex];
                string currentCorrectAnswer = currentCard.Answer;

                this.UserAnswerText.Text = userAnswer;
                this.CorrectAnswerText.Text = currentCorrectAnswer;

                if (userAnswer.Trim().Equals(currentCorrectAnswer.Trim(), StringComparison.OrdinalIgnoreCase))
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
                }

                this.QuestionCard.Visibility = Visibility.Collapsed;
                this.AnswerCard.Visibility = Visibility.Visible;
            }
        }

        private void NextCard_Click(object sender, RoutedEventArgs e)
        {
            if (!this._flashcards.Any())
            {
                return;
            }

            this._currentCardIndex++;
            if (this._currentCardIndex >= this._flashcards.Count)
            {
                this._currentCardIndex = 0;
            }

            this.DisplayCard(this._currentCardIndex);
        }
    }
}