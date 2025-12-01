namespace BrainBurst.Presentation.Views
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Navigation;
    using BrainBurst.BLL.Interfaces;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Логіка взаємодії для View зміни пароля користувача.
    /// </summary>
    public partial class ChangePasswordView : UserControl
    {
        private readonly IUserService _userService;
        private readonly IAuthContext _authContext;
        private readonly ILogger<ChangePasswordView> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangePasswordView"/> class.
        /// </summary>
        /// <param name="userService">Сервіс для зміни даних користувача.</param>
        /// <param name="authContext">Контекст автентифікації для отримання ID поточного користувача.</param>
        /// <param name="logger">Логер для запису подій.</param>
        public ChangePasswordView(IUserService userService, IAuthContext authContext, ILogger<ChangePasswordView> logger)
        {
            this.InitializeComponent();
            this._userService = userService;
            this._authContext = authContext;
            this._logger = logger;

            this._logger.LogDebug("ChangePasswordView: View ініціалізовано.");
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
            string oldPassword = this.OldPasswordBox.Password;
            string newPassword = this.NewPasswordBox.Password;
            string confirmPassword = this.ConfirmPasswordBox.Password;
            int userId = this._authContext.CurrentUserId;

            this._logger.LogInformation("SaveButton_Click: Спроба зміни пароля для користувача {UserId}.", userId);

            this.StatusText.Text = string.Empty;
            this.StatusText.Foreground = Brushes.Red;

            if (newPassword != confirmPassword)
            {
                this.StatusText.Text = "Нові паролі не співпадають.";
                this._logger.LogWarning("SaveButton_Click: Відхилено. Нові паролі не співпадають для {UserId}.", userId);
                return;
            }

            try
            {
                await this._userService.ChangePasswordAsync(this._authContext.CurrentUserId, oldPassword, newPassword, CancellationToken.None);

                this.StatusText.Text = "Пароль успішно змінено!";
                this.StatusText.Foreground = Brushes.Green;

                this.OldPasswordBox.Password = string.Empty;
                this.NewPasswordBox.Password = string.Empty;
                this.ConfirmPasswordBox.Password = string.Empty;

                this._logger.LogInformation("SaveButton_Click: Пароль користувача {UserId} успішно змінено.", userId);
            }
            catch (ArgumentException ex)
            {
                this.StatusText.Text = ex.Message;
                this._logger.LogWarning(ex, "SaveButton_Click: Помилка зміни пароля (неправильні дані) для {UserId}.", userId);
            }
            catch (KeyNotFoundException ex)
            {
                this.StatusText.Text = "Помилка. Користувача не знайдено.";
                this._logger.LogError(ex, "SaveButton_Click: Критична помилка. Користувача {UserId} не знайдено.", userId);
            }
            catch (Exception ex)
            {
                this.StatusText.Text = "Помилка. Не вдалося змінити пароль.";
                this._logger.LogError(ex, "SaveButton_Click: Непередбачена помилка при зміні пароля {UserId}.", userId);
            }
        }
    }
}