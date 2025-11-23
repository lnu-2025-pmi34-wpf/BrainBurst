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

    /// <summary>
    /// Логіка взаємодії для View створення нової флеш-картки.
    /// </summary>
    public partial class CreateCardView : UserControl
    {
        private readonly IFlashcardService _flashcardService;
        private readonly IAuthContext _authContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateCardView"/> class.
        /// </summary>
        /// <param name="flashcardService">Сервіс для створення флеш-карток.</param>
        /// <param name="authContext">Контекст автентифікації для отримання ID творця.</param>
        public CreateCardView(IFlashcardService flashcardService, IAuthContext authContext)
        {
            this.InitializeComponent();
            this._flashcardService = flashcardService;
            this._authContext = authContext;
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
            string? tagsInput = this.TagsTextBox.Text?.Trim();

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
                await this._flashcardService.CreateAsync(this._authContext.CurrentUserId, question, answer, tags, CancellationToken.None);

                this.StatusText.Foreground = Brushes.Green;
                this.StatusText.Text = "Картку успішно збережено!";

                this.QuestionTextBox.Text = string.Empty;
                this.AnswerTextBox.Text = string.Empty;
            }
            catch (ArgumentException ex)
            {
                this.StatusText.Text = ex.Message;
            }
            catch (Exception ex)
            {
                this.StatusText.Text = $"Помилка: {ex.Message}";
            }
        }
    }
}