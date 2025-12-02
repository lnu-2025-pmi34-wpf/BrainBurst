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
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Логіка взаємодії для View режиму навчання (вивчення флеш-карток).
    /// </summary>
    public partial class StudyView : UserControl
    {
        private readonly IFlashcardService _flashcardService;
        private readonly IAuthContext _authContext;
        private readonly ILogger<StudyView> _logger;

        private List<FlashcardDTO> _flashcards = new List<FlashcardDTO>();
        private int _currentCardIndex = 0;
        private string? _targetTag;

        /// <summary>
        /// Initializes a new instance of the <see cref="StudyView"/> class.
        /// </summary>
        /// <param name="flashcardService">Сервіс для отримання списку флеш-карток.</param>
        /// <param name="authContext">Контекст автентифікації.</param>
        /// <param name="logger">Логер для запису подій.</param>
        public StudyView(IFlashcardService flashcardService, IAuthContext authContext, ILogger<StudyView> logger)
        {
            this.InitializeComponent();
            this._flashcardService = flashcardService;
            this._authContext = authContext;
            this._logger = logger;

            this._logger.LogDebug("StudyView: View ініціалізовано.");

            this.Loaded += this.StudyView_Loaded;
        }

        /// <summary>
        /// Налаштовує режим навчання для конкретної колоди (тегу).
        /// </summary>
        /// <param name="tag">Назва тегу колоди.</param>
        public void Configure(string tag)
        {
            this._targetTag = tag;
            this._logger.LogInformation("Configure: Режим навчання налаштовано для тегу: {Tag}", tag);
        }

        private async void StudyView_Loaded(object sender, RoutedEventArgs e)
        {
            this._logger.LogInformation("StudyView_Loaded: Запуск завантаження карток для навчання (Тег: {Tag})", this._targetTag ?? "Усі");
            try
            {
                await this.LoadCardsAsync();
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "StudyView_Loaded: Критична помилка ініціалізації навчання.");
                this.QuestionText.Text = "Помилка ініціалізації.";
            }
        }

        private async Task LoadCardsAsync()
        {
            try
            {
                // Отримуємо ID поточного користувача з контексту, а не хардкодом
                int userId = this._authContext.CurrentUserId;
                if (userId <= 0)
                {
                    this._logger.LogError("LoadCardsAsync: ID користувача недійсний ({UserId}). Скасування завантаження.", userId);
                    throw new InvalidOperationException("Кориристувач не автентифікований або ID недійсний.");
                }

                // Отримуємо всі картки користувача
                var allCards = await this._flashcardService.ListAsync(userId, null, CancellationToken.None);

                // Фільтруємо за тегом, якщо він був переданий через Configure
                if (!string.IsNullOrEmpty(this._targetTag))
                {
                    this._flashcards = allCards
                        .Where(c => c.Tags.Contains(this._targetTag))
                        .ToList();
                    this._logger.LogDebug("LoadCardsAsync: Відфільтровано {Count} карток за тегом {Tag}", this._flashcards.Count, this._targetTag);
                }
                else
                {
                    this._flashcards = allCards.ToList();
                    this._logger.LogDebug("LoadCardsAsync: Завантажено {Count} усіх карток.", this._flashcards.Count);
                }

                if (this._flashcards.Any())
                {
                    this._currentCardIndex = 0;
                    this.DisplayCard(this._currentCardIndex);
                    this._logger.LogInformation("LoadCardsAsync: Режим навчання розпочато. Всього карток: {Count}", this._flashcards.Count);
                }
                else
                {
                    this.QuestionCard.Visibility = Visibility.Visible;
                    this.AnswerCard.Visibility = Visibility.Collapsed;
                    this.QuestionText.Text = "Картки не знайдено. Створіть нову картку!";
                    this.QuestionTopic.Text = string.Empty;
                    this._logger.LogWarning("LoadCardsAsync: Картки для навчання відсутні.");
                }

                this.AnswerTextBox.Focus();
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "LoadCardsAsync: Критична помилка завантаження карток.");
                this.QuestionText.Text = "Помилка завантаження карток.";
                throw;
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
                    this._logger.LogDebug("AnswerTextBox_KeyDown: Картка {CardId} ПРАВИЛЬНО вивчена.", currentCard.Id);
                    this.ResultIcon.Text = "✅";
                    this.ResultIcon.Foreground = Brushes.Green;
                    this.AnswerCard.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#F0FFF0") !;
                }
                else
                {
                    this._logger.LogDebug("AnswerTextBox_KeyDown: Картка {CardId} НЕПРАВИЛЬНО вивчена.", currentCard.Id);
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
                this._logger.LogWarning("NextCard_Click: Немає карток для переходу.");
                return;
            }

            this._currentCardIndex++;
            if (this._currentCardIndex >= this._flashcards.Count)
            {
                this._currentCardIndex = 0; // Починаємо спочатку, або можна вивести повідомлення про кінець
                this._logger.LogInformation("NextCard_Click: Картки завершено, повтор колоди.");
            }

            this.DisplayCard(this._currentCardIndex);
            this._logger.LogDebug("NextCard_Click: Перехід до картки {Index}/{Total}.", this._currentCardIndex + 1, this._flashcards.Count);
        }

        private void PreviousCard_Click(object sender, RoutedEventArgs e)
        {
            if (!this._flashcards.Any())
            {
                return;
            }

            this._currentCardIndex--;
            if (this._currentCardIndex < 0)
            {
                this._currentCardIndex = this._flashcards.Count - 1;
                this._logger.LogDebug("PreviousCard_Click: Перехід до останньої картки (цикл).");
            }

            this.DisplayCard(this._currentCardIndex);
            this._logger.LogDebug("PreviousCard_Click: Перехід до картки {Index}/{Total}.", this._currentCardIndex + 1, this._flashcards.Count);
        }
    }
}