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

    /// <summary>
    /// Логіка взаємодії для View відображення основного профілю користувача.
    /// </summary>
    public partial class ProfileView : UserControl
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IUserService _userService;
        private readonly IFlashcardService _flashcardService;
        private readonly IAuthContext _authContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProfileView"/> class.
        /// </summary>
        /// <param name="serviceProvider">Постачальник служб DI (для навігації).</param>
        /// <param name="userService">Сервіс для доступу до даних користувача.</param>
        /// <param name="flashcardService">Сервіс для доступу до флеш-карток (для підрахунку).</param>
        /// <param name="authContext">Контекст автентифікації для отримання ID поточного користувача.</param>
        public ProfileView(IServiceProvider serviceProvider, IUserService userService, IFlashcardService flashcardService, IAuthContext authContext)
        {
            this.InitializeComponent();
            this._serviceProvider = serviceProvider;
            this._userService = userService;
            this._flashcardService = flashcardService;
            this._authContext = authContext;

            this.UsernameTextBlock.Text = this._authContext.CurrentUser?.FullName ?? this._authContext.CurrentUser?.Email ?? "Завантаження...";

            this.Loaded += this.ProfileView_Loaded;
        }

        private async void ProfileView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await this.LoadProfileAsync();
            }
            catch (Exception)
            {
            }
        }

        private async Task LoadProfileAsync()
        {
            try
            {
                var currentUser = this._authContext.CurrentUser;

                if (currentUser == null)
                {
                    throw new InvalidOperationException("Користувач не автентифікований.");
                }

                this.UsernameTextBlock.Text = currentUser.FullName ?? currentUser.Email;

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
            catch (Exception)
            {
                this.UsernameTextBlock.Text = "Помилка завантаження профілю";
                this.FlashcardsCountTextBlock.Text = "--- флешкарток";
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