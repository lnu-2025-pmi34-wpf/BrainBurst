namespace BrainBurst.Presentation.Views
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Navigation;
    using BrainBurst.BLL.DTO;
    using BrainBurst.BLL.Interfaces;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Логіка взаємодії для відображення архіву пройдених тестів користувача.
    /// </summary>
    public partial class ArchiveView : UserControl
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IArchiveService _archiveService;
        private readonly IAuthContext _authContext;

        private IReadOnlyList<ArchiveEntryDTO> _archiveEntries = new List<ArchiveEntryDTO>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ArchiveView"/> class.
        /// </summary>
        /// <param name="serviceProvider">Постачальник служб DI.</param>
        /// <param name="archiveService">Сервіс для отримання архівних даних.</param>
        /// <param name="authContext">Контекст автентифікації для отримання ID поточного користувача.</param>
        public ArchiveView(IServiceProvider serviceProvider, IArchiveService archiveService, IAuthContext authContext)
        {
            this.InitializeComponent();
            this._serviceProvider = serviceProvider;
            this._archiveService = archiveService;
            this._authContext = authContext;

            this.Loaded += this.ArchiveView_Loaded;
        }

        private async void ArchiveView_Loaded(object sender, RoutedEventArgs e) // <--- ДОДАНО async void
        {
            try
            {
                await this.LoadArchiveAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не вдалося завантажити архів: {ex.Message}", "Помилка");
            }
        }

        private async Task LoadArchiveAsync()
        {
            this._archiveEntries = await this._archiveService.GetArchiveAsync(this._authContext.CurrentUserId, CancellationToken.None);
            this.ArchiveItemsControl.ItemsSource = this._archiveEntries;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.GoBack();
        }

        private void ResultItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int testResultId)
            {
                var selectedResult = this._archiveEntries.FirstOrDefault(x => x.TestResultId == testResultId);

                var mistakes = new List<dynamic>
                {
                    new { QuestionText = $"Запитання 1 для {selectedResult?.TestTitle ?? "Тесту"} (ID: {testResultId})", UserAnswer = "Невірний варіант", CorrectAnswer = "Правильна відповідь" },
                    new { QuestionText = $"Запитання 2 для {selectedResult?.TestTitle ?? "Тесту"} (ID: {testResultId})", UserAnswer = "Ще одна помилка", CorrectAnswer = "Ще одна правильна відповідь" },
                };

                // int totalQuestions = 10;
                if (NavigationService.GetNavigationService(this) != null)
                {
                }
            }
        }

        private void GoBack()
        {
            if (NavigationService.GetNavigationService(this) != null &&
                NavigationService.GetNavigationService(this).CanGoBack)
            {
                NavigationService.GetNavigationService(this).GoBack();
            }
        }
    }
}