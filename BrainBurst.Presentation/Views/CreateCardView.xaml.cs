using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using BrainBurst.BLL.Interfaces; // Додано для IAuthContext
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;

namespace BrainBurst.Presentation.Views
{
    public partial class CreateCardView : UserControl
    {
        // УСУНЕНО: private const int CurrentUserId = 1; 

        private readonly IFlashcardService _flashcardService;
        private readonly IAuthContext _authContext; // <--- ДОДАНО ПОЛЕ

        // ОНОВЛЕНО: Конструктор приймає IAuthContext
        public CreateCardView(IFlashcardService flashcardService, IAuthContext authContext)
        {
            InitializeComponent();
            _flashcardService = flashcardService;
            _authContext = authContext; // <--- ІНІЦІАЛІЗОВАНО
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.GetNavigationService(this).CanGoBack)
            {
                NavigationService.GetNavigationService(this).GoBack();
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string question = QuestionTextBox.Text;
            string answer = AnswerTextBox.Text;
            string tagsInput = TagsTextBox.Text;

            // Парсимо теги: розділяємо за комою, видаляємо пробіли та пусті рядки
            var tags = tagsInput.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                 .Select(t => t.Trim())
                                 .Where(t => !string.IsNullOrWhiteSpace(t));

            StatusText.Text = "";
            StatusText.Foreground = Brushes.Red;
            
            try
            {
                // ВИКОРИСТАННЯ: CurrentUserId замінено на _authContext.CurrentUserId
                await _flashcardService.CreateAsync(_authContext.CurrentUserId, question, answer, tags, CancellationToken.None);

                // Успіх
                StatusText.Foreground = Brushes.Green;
                StatusText.Text = "Картку успішно збережено!";

                // Очищаємо поля після успішного збереження
                QuestionTextBox.Text = "";
                AnswerTextBox.Text = "";
                TagsTextBox.Text = "";
            }
            catch (ArgumentException ex)
            {
                // Помилка валідації (наприклад, порожнє питання чи відповідь)
                StatusText.Text = ex.Message;
            }
            catch (Exception)
            {
                StatusText.Text = "Непередбачена помилка. Не вдалося зберегти картку.";
            }
        }
    }
}