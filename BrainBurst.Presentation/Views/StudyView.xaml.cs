using System.Windows;
using System.Windows.Controls;
using System.Windows.Input; 
using System.Windows.Media;
using BrainBurst.BLL.Interfaces; // Для IFlashcardService
using BrainBurst.BLL.DTO; // Для FlashcardDTO
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace BrainBurst.Presentation.Views
{
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
            InitializeComponent();
            _flashcardService = flashcardService;
            
            // Завантажуємо дані після завантаження елемента в UI
            this.Loaded += StudyView_Loaded; 
        }

        private void StudyView_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCardsAsync();
        }

        private async Task LoadCardsAsync()
        {
            try
            {
                // Завантажуємо всі картки користувача
                _flashcards = (await _flashcardService.ListAsync(CurrentUserId, null, CancellationToken.None)).ToList();
                
                if (_flashcards.Any())
                {
                    _currentCardIndex = 0;
                    DisplayCard(_currentCardIndex);
                }
                else
                {
                    // Обробка випадку, коли карток немає
                    QuestionText.Text = "Картки не знайдено. Створіть нову картку!";
                }
                
                AnswerTextBox.Focus();
            }
            catch (Exception)
            {
                QuestionText.Text = "Помилка завантаження карток.";
            }
        }

        private void DisplayCard(int index)
        {
            if (index >= 0 && index < _flashcards.Count)
            {
                var card = _flashcards[index];
                
                // Встановлюємо текст
                QuestionText.Text = card.Question; 
                
                // Встановлюємо тему (перший тег або дефолт)
                QuestionTopic.Text = card.Tags.FirstOrDefault() ?? "Загальна колода";
                AnswerTopic.Text = QuestionTopic.Text;
                
                // Скидаємо UI до режиму "питання"
                AnswerTextBox.Text = "";
                QuestionCard.Visibility = Visibility.Visible;
                AnswerCard.Visibility = Visibility.Collapsed;
                
                AnswerTextBox.Focus();
            }
        }

        private void AnswerTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && _flashcards.Any() && QuestionCard.Visibility == Visibility.Visible)
            {
                string userAnswer = AnswerTextBox.Text;
                var currentCard = _flashcards[_currentCardIndex];
                string currentCorrectAnswer = currentCard.Answer; // Беремо справжню відповідь

                UserAnswerText.Text = userAnswer;
                CorrectAnswerText.Text = currentCorrectAnswer;

                // Проста перевірка відповіді (регістронезалежне порівняння)
                if (userAnswer.Trim().Equals(currentCorrectAnswer.Trim(), StringComparison.OrdinalIgnoreCase)) 
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
            if (!_flashcards.Any()) return;
            
            _currentCardIndex++;
            if (_currentCardIndex >= _flashcards.Count)
            {
                // Повертаємося до початку колоди
                _currentCardIndex = 0; 
            }
            
            DisplayCard(_currentCardIndex);
        }
    }
}