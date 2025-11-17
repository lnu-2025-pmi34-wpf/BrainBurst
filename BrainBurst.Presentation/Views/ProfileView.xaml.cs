namespace BrainBurst.Presentation.Views
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Navigation;
    using BrainBurst.BLL.Interfaces; // Додаємо для IUserService, IFlashcardService та IAuthContext
    using Microsoft.Extensions.DependencyInjection; // Для DI

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
            this.InitializeComponent();
            this._serviceProvider = serviceProvider;
            this._userService = userService;
            this._flashcardService = flashcardService;
            this._authContext = authContext; // <--- ІНІЦІАЛІЗОВАНО

            // Встановлюємо ім'я з контексту одразу, щоб уникнути затримок
            this.UsernameTextBlock.Text = this._authContext.CurrentUser?.FullName ?? this._authContext.CurrentUser?.Email ?? "Завантаження...";

            this.Loaded += this.ProfileView_Loaded;
        }

        // Завантажуємо дані після завантаження елемента в UI
        private void ProfileView_Loaded(object sender, RoutedEventArgs e)
        {
            this.LoadProfileAsync();
        }

        private async Task LoadProfileAsync()
        {
            try
            {
                // 1. ПЕРЕВІРКА: Використовуємо дані з контексту, якщо вони доступні
                var currentUser = this._authContext.CurrentUser;

                if (currentUser == null)
                {
                    // Якщо контекст пустий (наприклад, після виходу), ми повинні спробувати завантажити
                    // АБО завершити, оскільки користувач має бути аутентифікований.
                    throw new InvalidOperationException("Користувач не автентифікований.");
                }

                this.UsernameTextBlock.Text = currentUser.FullName ?? currentUser.Email;

                // 2. Завантаження кількості карток
                // Якщо користувач щойно зареєстрований, він повинен мати ID > 0.
                if (currentUser.Id > 0)
                {
                    var flashcards = await this._flashcardService.ListAsync(currentUser.Id, null, CancellationToken.None);
                    this.FlashcardsCountTextBlock.Text = $"{flashcards.Count} флешкарток";
                }
                else
                {
                    this.FlashcardsCountTextBlock.Text = "0 флешкарток";
                }

            }
            catch (Exception ex)
            {
                // Якщо помилка трапилась тут, це KeyNotFoundException або DB-помилка.
                this.UsernameTextBlock.Text = "Помилка завантаження профілю";
                this.FlashcardsCountTextBlock.Text = "--- флешкарток";

                // ⚠️ Запустіть програму в Debug, щоб побачити точний InnerException тут!
                // MessageBox.Show($"Debug Error: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        private void EditProfileButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.GetNavigationService(this) != null)
            {
                var editProfileView = this._serviceProvider.GetRequiredService<EditProfileView>();
                NavigationService.GetNavigationService(this).Navigate(editProfileView);
            }
        }

        private void ArchiveButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.GetNavigationService(this) != null)
            {
                var archiveView = this._serviceProvider.GetRequiredService<ArchiveView>();
                NavigationService.GetNavigationService(this).Navigate(archiveView);
            }
        }
    }
}