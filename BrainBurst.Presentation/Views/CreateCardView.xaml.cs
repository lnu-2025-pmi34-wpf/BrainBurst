namespace BrainBurst.Presentation.Views
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Navigation;
    using BrainBurst.BLL.Interfaces;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Логіка взаємодії для View створення нової флеш-картки.
    /// </summary>
    public partial class CreateCardView : UserControl
    {
        private readonly IFlashcardService _flashcardService;
        private readonly IAuthContext _authContext;
        private readonly ILogger<CreateCardView> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateCardView"/> class.
        /// </summary>
        /// <param name="flashcardService">Сервіс для створення флеш-карток.</param>
        /// <param name="authContext">Контекст автентифікації для отримання ID творця.</param>
        /// <param name="logger">Логер для запису подій.</param>
        public CreateCardView(IFlashcardService flashcardService, IAuthContext authContext, ILogger<CreateCardView> logger)
        {
            this.InitializeComponent();
            this._flashcardService = flashcardService;
            this._authContext = authContext;
            this._logger = logger;

            this._logger.LogInformation("CreateCardView: View ініціалізовано.");
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.GetNavigationService(this).CanGoBack)
            {
                this._logger.LogInformation("BackButton_Click: Повернення до попереднього View.");
                NavigationService.GetNavigationService(this).GoBack();
            }
            else
            {
                this._logger.LogWarning("BackButton_Click: Навігація неможлива (немає попередньої сторінки).");
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string question = this.QuestionTextBox.Text;
            string answer = this.AnswerTextBox.Text;
            string? tagsInput = this.TagsTextBox.Text?.Trim();

            this._logger.LogInformation("SaveButton_Click: Запущено збереження картки. Питання: {Question}", question.Length > 30 ? question[..30] + "..." : question);                                                                                                                             

            IEnumerable<string> tags;
            if (string.IsNullOrWhiteSpace(tagsInput))
            {
                tags = Array.Empty<string>();
            }
            else
            {
                tags = tagsInput.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(t => t.Trim())
                                .Where(t => !string.IsNullOrWhiteSpace(t))
                                .ToList();
            }

            this.StatusText.Text = string.Empty;
            this.StatusText.Foreground = Brushes.Red;

            try
            {
                int creatorId = this._authContext.CurrentUserId;

                this._logger.LogDebug("SaveButton_Click: Виклик FlashcardService. CreateAsync.");
                await this._flashcardService.CreateAsync(this._authContext.CurrentUserId, question, answer, tags, CancellationToken.None);

                this.StatusText.Foreground = Brushes.Green;
                this.StatusText.Text = "Картку успішно збережено!";

                this.QuestionTextBox.Text = string.Empty;
                this.AnswerTextBox.Text = string.Empty;

                this._logger.LogInformation("SaveButton_Click: Картка успішно збережена користувачем {CreatorId}.", creatorId);
            }
            catch (ArgumentException ex)
            {
                this._logger.LogWarning(ex, "SaveButton_Click: Помилка валідації даних картки.");
                this.StatusText.Text = ex.Message;
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "SaveButton_Click: Критична помилка під час збереження картки.");
                this.StatusText.Text = $"Помилка: {ex.Message}";
            }
        }
    }
}