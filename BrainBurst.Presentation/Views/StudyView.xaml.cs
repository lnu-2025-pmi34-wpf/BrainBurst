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
    using BrainBurst.BLL.DTO;
    using BrainBurst.BLL.Interfaces;

    /// <summary>
    /// Логіка взаємодії для View режиму навчання (вивчення флеш-карток).
    /// </summary>
    public partial class StudyView : UserControl
    {
        private readonly IFlashcardService _flashcardService;
        private readonly IAuthContext _authContext;
        
        private List<FlashcardDTO> _flashcards = new List<FlashcardDTO>();
        private int _currentCardIndex = 0;
        private string? _targetTag;

        /// <summary>
        /// Initializes a new instance of the <see cref="StudyView"/> class.
        /// </summary>
        /// <param name="flashcardService">Сервіс для отримання списку флеш-карток.</param>
        /// <param name="authContext">Контекст автентифікації.</param>
        public StudyView(IFlashcardService flashcardService, IAuthContext authContext)
        {
            this.InitializeComponent();
            this._flashcardService = flashcardService;
            this._authContext = authContext;

            this.Loaded += this.StudyView_Loaded;
        }

        /// <summary>
        /// Налаштовує режим навчання для конкретної колоди (тегу).
        /// </summary>
        /// <param name="tag">Назва тегу колоди.</param>
        public void Configure(string tag)
        {
            this._targetTag = tag;
        }

        private async void StudyView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await this.LoadCardsAsync();
            }
            catch (Exception)
            {
                this.QuestionText.Text = "Помилка ініціалізації.";
            }
        }

        private async Task LoadCardsAsync()
        {
            try
            {
                // Отримуємо ID поточного користувача з контексту, а не хардкодом
                int userId = this._authContext.CurrentUserId;

                // Отримуємо всі картки користувача
                var allCards = await this._flashcardService.ListAsync(userId, null, CancellationToken.None);

                // Фільтруємо за тегом, якщо він був переданий через Configure
                if (!string.IsNullOrEmpty(this._targetTag))
                {
                    this._flashcards = allCards
                        .Where(c => c.Tags.Contains(this._targetTag))
                        .ToList();
                }
                else
                {
                    this._flashcards = allCards.ToList();
                }

                if (this._flashcards.Any())
                {
                    this._currentCardIndex = 0;
                    this.DisplayCard(this._currentCardIndex);
                }
                else
                {
                    this.QuestionCard.Visibility = Visibility.Visible;
                    this.AnswerCard.Visibility = Visibility.Collapsed;
                    this.QuestionText.Text = "Картки не знайдено. Створіть нову картку!";
                    this.QuestionTopic.Text = string.Empty;
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

                // Якщо ми вчимо конкретну колоду, показуємо її назву, інакше перший тег
                this.QuestionTopic.Text = this._targetTag ?? card.Tags.FirstOrDefault() ?? "Загальна колода";
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

                this.UserAnswerText.Text = string.IsNullOrWhiteSpace(userAnswer) ? "[Порожньо]" : userAnswer;
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
                this._currentCardIndex = 0; // Починаємо спочатку, або можна вивести повідомлення про кінець
            }

            this.DisplayCard(this._currentCardIndex);
        }
    }
}