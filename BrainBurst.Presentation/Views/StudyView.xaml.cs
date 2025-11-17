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
    using BrainBurst.BLL.DTO; // Для FlashcardDTO
    using BrainBurst.BLL.Interfaces; // Для IFlashcardService

    public partial class StudyView : UserControl
    {
        // 🚨 ТИМЧАСОВО: Використовуємо фіктивний ID
        private const int CurrentUserId = 1;

        private readonly IFlashcardService _flashcardService;
        private List<FlashcardDTO> _flashcards = new List<FlashcardDTO>();
        private int _currentCardIndex = 0;

        // Оновлюємо конструктор для DI
        public StudyView(IFlashcardService flashcardService)
        {
            this.InitializeComponent();
            this._flashcardService = flashcardService;

            // Завантажуємо дані після завантаження елемента в UI
            this.Loaded += this.StudyView_Loaded;
        }

        private void StudyView_Loaded(object sender, RoutedEventArgs e)
        {
            this.LoadCardsAsync();
        }

        private async Task LoadCardsAsync()
        {
            try
            {
                // Завантажуємо всі картки користувача
                this._flashcards = (await this._flashcardService.ListAsync(CurrentUserId, null, CancellationToken.None)).ToList();

                if (this._flashcards.Any())
                {
                    this._currentCardIndex = 0;
                    this.DisplayCard(this._currentCardIndex);
                }
                else
                {
                    // Обробка випадку, коли карток немає
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

                // Встановлюємо текст
                this.QuestionText.Text = card.Question;

                // Встановлюємо тему (перший тег або дефолт)
                this.QuestionTopic.Text = card.Tags.FirstOrDefault() ?? "Загальна колода";
                this.AnswerTopic.Text = this.QuestionTopic.Text;

                // Скидаємо UI до режиму "питання"
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
                string currentCorrectAnswer = currentCard.Answer; // Беремо справжню відповідь

                this.UserAnswerText.Text = userAnswer;
                this.CorrectAnswerText.Text = currentCorrectAnswer;

                // Проста перевірка відповіді (регістронезалежне порівняння)
                if (userAnswer.Trim().Equals(currentCorrectAnswer.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    this.ResultIcon.Text = "✅";
                    this.ResultIcon.Foreground = Brushes.Green;
                    this.AnswerCard.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#F0FFF0"));
                }
                else
                {
                    this.ResultIcon.Text = "❌";
                    this.ResultIcon.Foreground = Brushes.Red;
                    this.AnswerCard.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFF0F0"));
                }

                this.QuestionCard.Visibility = Visibility.Collapsed;
                this.AnswerCard.Visibility = Visibility.Visible;
            }
        }

        private void NextCard_Click(object sender, RoutedEventArgs e)
        {
            if (!this._flashcards.Any()) return;

            this._currentCardIndex++;
            if (this._currentCardIndex >= this._flashcards.Count)
            {
                // Повертаємося до початку колоди
                this._currentCardIndex = 0;
            }

            this.DisplayCard(this._currentCardIndex);
        }
    }
}