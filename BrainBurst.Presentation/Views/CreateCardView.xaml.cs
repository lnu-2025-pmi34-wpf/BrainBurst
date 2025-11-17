namespace BrainBurst.Presentation.Views
{
    using System;
    using System.Collections.Generic; // Додано для List<T>
    using System.Linq;
    using System.Threading; // Додано для CancellationToken
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Navigation;
    using BrainBurst.BLL.Interfaces; // Додано для IAuthContext

    public partial class CreateCardView : UserControl
    {
        private readonly IFlashcardService _flashcardService;
        private readonly IAuthContext _authContext; // <--- ДОДАНО ПОЛЕ

        // ОНОВЛЕНО: Конструктор приймає IAuthContext
        public CreateCardView(IFlashcardService flashcardService, IAuthContext authContext)
        {
            this.InitializeComponent();
            this._flashcardService = flashcardService;
            this._authContext = authContext; // <--- ІНІЦІАЛІЗОВАНО
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
            string question = this.QuestionTextBox.Text;
            string answer = this.AnswerTextBox.Text;
            string tagsInput = this.TagsTextBox.Text?.Trim(); // Одразу обрізаємо пробіли з країв

            // Нова, більш надійна логіка парсингу тегів
            IEnumerable<string> tags;
            if (string.IsNullOrWhiteSpace(tagsInput))
            {
                tags = Array.Empty<string>();
            }
            else
            {
                // Розділяємо за комами, крапками з комою або навіть пробілами, якщо хочете (тут тільки коми для початку)
                tags = tagsInput.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(t => t.Trim())
                                .Where(t => !string.IsNullOrWhiteSpace(t))
                                .ToList(); // Матеріалізуємо список одразу
            }

            this.StatusText.Text = string.Empty;
            this.StatusText.Foreground = Brushes.Red;

            try
            {
                // ВИКОРИСТАННЯ: CurrentUserId замінено на _authContext.CurrentUserId
                await this._flashcardService.CreateAsync(this._authContext.CurrentUserId, question, answer, tags, CancellationToken.None);

                // Успіх
                this.StatusText.Foreground = Brushes.Green;
                this.StatusText.Text = "Картку успішно збережено!";

                // Очищаємо поля після успішного збереження
                this.QuestionTextBox.Text = string.Empty;
                this.AnswerTextBox.Text = string.Empty;
                // TagsTextBox.Text = ""; // Можна не очищати тему, якщо користувач хоче створити кілька карток підряд в одну тему
            }
            catch (ArgumentException ex)
            {
                // Помилка валідації (наприклад, порожнє питання чи відповідь)
                this.StatusText.Text = ex.Message;
            }
            catch (Exception ex)
            {
                // Інші помилки (наприклад, проблеми з БД або мережею)
                // ТЕПЕР ПОКАЗУЄ ВНУТРІШНЮ ПОМИЛКУ БД
                this.StatusText.Text = $"Помилка: {ex.Message}";
            }
        }
    }
}