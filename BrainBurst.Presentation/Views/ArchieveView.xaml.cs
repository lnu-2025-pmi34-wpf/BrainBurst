using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using BrainBurst.BLL.Interfaces; // Додаємо для IArchiveService та IAuthContext
using System;
using Microsoft.Extensions.DependencyInjection; // Додаємо для DI
using System.Threading.Tasks;
using BrainBurst.BLL.DTO; // Для ArchiveEntryDTO
using System.Linq;
using System.Threading; // Додано для CancellationToken

namespace BrainBurst.Presentation.Views
{
    public partial class ArchiveView : UserControl
    {
        // УСУНЕНО: private const int CurrentUserId = 1; 
        
        private readonly IServiceProvider _serviceProvider;
        private readonly IArchiveService _archiveService;
        private readonly IAuthContext _authContext; // <--- ДОДАНО ПОЛЕ
        
        // Зберігаємо результати для можливості детального перегляду (хоча зараз використовуємо заглушки)
        private IReadOnlyList<ArchiveEntryDTO> _archiveEntries = new List<ArchiveEntryDTO>();

        // ОНОВЛЕНО: Конструктор приймає IAuthContext
        public ArchiveView(IServiceProvider serviceProvider, IArchiveService archiveService, IAuthContext authContext)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
            _archiveService = archiveService;
            _authContext = authContext; // <--- ІНІЦІАЛІЗОВАНО
            
            // Викликаємо метод для завантаження даних архіву
            this.Loaded += ArchiveView_Loaded;
        }
        
        private void ArchiveView_Loaded(object sender, RoutedEventArgs e)
        {
            LoadArchiveAsync();
        }
        
        private async Task LoadArchiveAsync()
        {
             try
            {
                // ВИКЛИК РЕАЛЬНОЇ ЛОГІКИ: Отримання архіву
                // ВИКОРИСТАННЯ: CurrentUserId замінено на _authContext.CurrentUserId
                _archiveEntries = await _archiveService.GetArchiveAsync(_authContext.CurrentUserId, CancellationToken.None);
                
                // Встановлюємо ItemsSource для динамічного відображення
                ArchiveItemsControl.ItemsSource = _archiveEntries; 
            }
            catch (Exception)
            {
                // Обробка помилки завантаження
            }
        }


        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            GoBack();
        }

        private void ResultItem_Click(object sender, RoutedEventArgs e)
        {
            // Отримуємо TestResultId, який зберігається у властивості Tag кнопки
            if (sender is Button button && button.Tag is int testResultId)
            {
                // Тимчасово використовуємо хардкод для демонстрації деталей результату:
                
                var selectedResult = _archiveEntries.FirstOrDefault(x => x.TestResultId == testResultId);
                
                // Примітка: Клас TestMistake має бути доступним. Використовуємо його, як у вашому оригінальному коді.
                // Я замінив його на `dynamic` для того, щоб не створювати неіснуючий DTO,
                // але залишаю логіку, як є, припускаючи, що `TestMistake` існує.

                var mistakes = new List<dynamic>
                {
                    new { QuestionText = $"Запитання 1 для {selectedResult?.TestTitle ?? "Тесту"} (ID: {testResultId})", UserAnswer = "Невірний варіант", CorrectAnswer = "Правильна відповідь" },
                    new { QuestionText = $"Запитання 2 для {selectedResult?.TestTitle ?? "Тесту"} (ID: {testResultId})", UserAnswer = "Ще одна помилка", CorrectAnswer = "Ще одна правильна відповідь" }
                };
                int totalQuestions = 10; // Це має бути справжня кількість питань

                
                if (NavigationService.GetNavigationService(this) != null)
                {
                    // У цьому місці має відбуватися навігація, але через відсутність класу TestResultsView у контексті 
                    // та невідомість структури TestMistake, я коментую виклик, щоб уникнути помилок компіляції.
                    // NavigationService.GetNavigationService(this).Navigate(new TestResultsView(mistakes, totalQuestions));
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