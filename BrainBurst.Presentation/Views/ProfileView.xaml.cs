using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System;
using Microsoft.Extensions.DependencyInjection; // Для DI
using BrainBurst.BLL.Interfaces; // Додаємо для IUserService, IFlashcardService та IAuthContext
using System.Threading.Tasks;
using System.Threading;

namespace BrainBurst.Presentation.Views
{
    public partial class ProfileView : UserControl
    {
        // УСУНЕНО: private const int CurrentUserId = 1; 

        private readonly IServiceProvider _serviceProvider;
        private readonly IUserService _userService;
        private readonly IFlashcardService _flashcardService;
        private readonly IAuthContext _authContext; // <--- ДОДАНО ПОЛЕ

        // ОНОВЛЕНО: Конструктор приймає IAuthContext
        public ProfileView(IServiceProvider serviceProvider, IUserService userService, IFlashcardService flashcardService, IAuthContext authContext) 
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
            _userService = userService;
            _flashcardService = flashcardService;
            _authContext = authContext; // <--- ІНІЦІАЛІЗОВАНО
            
            // Встановлюємо ім'я з контексту одразу, щоб уникнути затримок
            UsernameTextBlock.Text = _authContext.CurrentUser?.FullName ?? _authContext.CurrentUser?.Email ?? "Завантаження...";
            
            this.Loaded += ProfileView_Loaded;
        }
        
        // Завантажуємо дані після завантаження елемента в UI
        private void ProfileView_Loaded(object sender, RoutedEventArgs e)
        {
            LoadProfileAsync();
        }

        private async Task LoadProfileAsync()
        {
            try
            {
                // 1. ПЕРЕВІРКА: Використовуємо дані з контексту, якщо вони доступні
                var currentUser = _authContext.CurrentUser;
                
                if (currentUser == null)
                {
                    // Якщо контекст пустий (наприклад, після виходу), ми повинні спробувати завантажити
                    // АБО завершити, оскільки користувач має бути аутентифікований.
                    throw new InvalidOperationException("Користувач не автентифікований.");
                }

                UsernameTextBlock.Text = currentUser.FullName ?? currentUser.Email;

                // 2. Завантаження кількості карток
                // Якщо користувач щойно зареєстрований, він повинен мати ID > 0.
                if (currentUser.Id > 0)
                {
                    var flashcards = await _flashcardService.ListAsync(currentUser.Id, null, CancellationToken.None);
                    FlashcardsCountTextBlock.Text = $"{flashcards.Count} флешкарток";
                }
                else
                {
                    FlashcardsCountTextBlock.Text = "0 флешкарток";
                }
                
            }
            catch (Exception ex)
            {
                // Якщо помилка трапилась тут, це KeyNotFoundException або DB-помилка.
                UsernameTextBlock.Text = "Помилка завантаження профілю";
                FlashcardsCountTextBlock.Text = "--- флешкарток";
                
                // ⚠️ Запустіть програму в Debug, щоб побачити точний InnerException тут!
                // MessageBox.Show($"Debug Error: {ex.InnerException?.Message ?? ex.Message}");
            }
        }
        
        private void EditProfileButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.GetNavigationService(this) != null)
            {
                var editProfileView = _serviceProvider.GetRequiredService<EditProfileView>(); 
                NavigationService.GetNavigationService(this).Navigate(editProfileView);
            }
        }

        private void ArchiveButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.GetNavigationService(this) != null)
            {
                var archiveView = _serviceProvider.GetRequiredService<ArchiveView>(); 
                NavigationService.GetNavigationService(this).Navigate(archiveView);
            }
        }
    }
}