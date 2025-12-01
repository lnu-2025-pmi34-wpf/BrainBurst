namespace BrainBurst.Presentation.Views
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Navigation;
    using BrainBurst.BLL.Interfaces;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Логіка взаємодії для View відображення основного профілю користувача.
    /// </summary>
    public partial class ProfileView : UserControl
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IUserService _userService;
        private readonly IFlashcardService _flashcardService;
        private readonly IAuthContext _authContext;
        private readonly ILogger<ProfileView> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProfileView"/> class.
        /// </summary>
        /// <param name="serviceProvider">Постачальник служб DI (для навігації).</param>
        /// <param name="userService">Сервіс для доступу до даних користувача.</param>
        /// <param name="flashcardService">Сервіс для доступу до флеш-карток (для підрахунку).</param>
        /// <param name="authContext">Контекст автентифікації для отримання ID поточного користувача.</param>
        /// <param name="logger">Логер для запису подій.</param>
        public ProfileView(IServiceProvider serviceProvider, IUserService userService, IFlashcardService flashcardService, IAuthContext authContext, ILogger<ProfileView> logger)
        {
            this.InitializeComponent();
            this._serviceProvider = serviceProvider;
            this._userService = userService;
            this._flashcardService = flashcardService;
            this._authContext = authContext;
            this._logger = logger;

            this._logger.LogDebug("ProfileView: View ініціалізовано.");

            this.UsernameTextBlock.Text = this._authContext.CurrentUser?.FullName ?? this._authContext.CurrentUser?.Email ?? "Завантаження...";

            this.Loaded += this.ProfileView_Loaded;
        }

        private async void ProfileView_Loaded(object sender, RoutedEventArgs e)
        {
            this._logger.LogDebug("ProfileView_Loaded: Запуск асинхронного завантаження профілю.");

            try
            {
                await this.LoadProfileAsync();
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "ProfileView_Loaded: Непередбачена помилка при завантаженні профілю.");
            }
        }

        private async Task LoadProfileAsync()
        {
            try
            {
                var currentUser = this._authContext.CurrentUser;

                if (currentUser == null)
                {
                    this._logger.LogWarning("LoadProfileAsync: Користувач не автентифікований. Неможливо завантажити профіль.");
                    throw new InvalidOperationException("Користувач не автентифікований.");
                }

                this.UsernameTextBlock.Text = currentUser.FullName ?? currentUser.Email;
                this._logger.LogDebug("LoadProfileAsync: Ім'я користувача встановлено: {Username}", this.UsernameTextBlock.Text);

                if (currentUser.Id > 0)
                {
                    this._logger.LogDebug("LoadProfileAsync: Запит на підрахунок флеш-карток для ID {UserId}", currentUser.Id);

                    var flashcards = await this._flashcardService.ListAsync(currentUser.Id, null, CancellationToken.None);
                    this.FlashcardsCountTextBlock.Text = $"{flashcards.Count} флешкарток";

                    this._logger.LogInformation("LoadProfileAsync: Знайдено {Count} флеш-карток.", flashcards.Count);
                }
                else
                {
                    this._logger.LogWarning("LoadProfileAsync: ID користувача некоректний ({UserId}). Кількість карток встановлено в 0.", currentUser.Id);
                    this.FlashcardsCountTextBlock.Text = "0 флешкарток";
                }
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "LoadProfileAsync: Критична помилка під час отримання даних профілю.");
                this.UsernameTextBlock.Text = "Помилка завантаження профілю";
                this.FlashcardsCountTextBlock.Text = "--- флешкарток";
                throw;
            }
        }

        private void EditProfileButton_Click(object sender, RoutedEventArgs e)
        {
            this._logger.LogInformation("EditProfileButton_Click: Навігація до EditProfileView.");

            if (NavigationService.GetNavigationService(this) != null)
            {
                var editProfileView = this._serviceProvider.GetRequiredService<EditProfileView>();
                NavigationService.GetNavigationService(this).Navigate(editProfileView);
            }
            else
            {
                this._logger.LogWarning("EditProfileButton_Click: NavigationService недоступний. Неможливо перейти до EditProfileView.");
            }
        }

        private void ArchiveButton_Click(object sender, RoutedEventArgs e)
        {
            this._logger.LogInformation("ArchiveButton_Click: Навігація до ArchiveView.");

            if (NavigationService.GetNavigationService(this) != null)
            {
                var archiveView = this._serviceProvider.GetRequiredService<ArchiveView>();
                NavigationService.GetNavigationService(this).Navigate(archiveView);
            }
            else
            {
                this._logger.LogWarning("ArchiveButton_Click: NavigationService недоступний. Неможливо перейти до ArchiveView.");
            }
        }
    }
}