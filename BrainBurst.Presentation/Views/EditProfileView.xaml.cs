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
    using Serilog.Core;

    /// <summary>
    /// Логіка взаємодії для View редагування профілю користувача.
    /// </summary>
    public partial class EditProfileView : UserControl
    {
        private readonly IUserService _userService;
        private readonly IAuthContext _authContext;
        private readonly ILogger<EditProfileView> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="EditProfileView"/> class.
        /// </summary>
        /// <param name="userService">Сервіс для оновлення даних користувача.</param>
        /// <param name="authContext">Контекст автентифікації для отримання ID поточного користувача.</param>
        /// <param name="logger">Логер для запису подій.</param>
        public EditProfileView(IUserService userService, IAuthContext authContext, ILogger<EditProfileView> logger)
        {
            this.InitializeComponent();
            this._userService = userService;
            this._authContext = authContext;
            this._logger = logger;

            this._logger.LogDebug("EditProfileView: View ініціалізовано.");
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

        private void ChangeUsernameButton_Click(object sender, RoutedEventArgs e)
        {
            this._logger.LogDebug("ChangeUsernameButton_Click: Активовано панель зміни імені.");
            this.ChangeUsernamePanel.Visibility = Visibility.Visible;
            this.UsernameStatusText.Text = string.Empty;
            this.NewUsernameTextBox.Focus();
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.ChangeUsernamePanel.Visibility == Visibility.Visible)
            {
                string newName = this.NewUsernameTextBox.Text;
                int userId = this._authContext.CurrentUserId;

                this._logger.LogInformation("SaveButton_Click: Спроба змінити ім'я користувача {UserId} на '{NewName}'", userId, newName);

                this.UsernameStatusText.Text = string.Empty;
                this.UsernameStatusText.Foreground = Brushes.Red;

                try
                {
                    await this._userService.UpdateProfileAsync(this._authContext.CurrentUserId, newName, CancellationToken.None);

                    this.UsernameStatusText.Text = "Ім'я успішно змінено!";
                    this.UsernameStatusText.Foreground = Brushes.Green;

                    if (this._authContext.CurrentUser != null)
                    {
                        this._authContext.CurrentUser.FullName = newName;
                    }

                    this._logger.LogInformation("SaveButton_Click: Ім'я користувача {UserId} успішно оновлено.", userId);
                }
                catch (ArgumentException ex)
                {
                    this.UsernameStatusText.Text = ex.Message;
                    this._logger.LogWarning(ex, "SaveButton_Click: Помилка валідації імені для {UserId}.", userId);
                }
                catch (KeyNotFoundException ex)
                {
                    this.UsernameStatusText.Text = "Помилка. Користувача не знайдено.";
                    this._logger.LogError(ex, "SaveButton_Click: Критична помилка. Користувача {UserId} не знайдено в БД.", userId);
                }
                catch (Exception ex)
                {
                    this.UsernameStatusText.Text = "Помилка. Не вдалося зберегти зміни.";
                    this._logger.LogError(ex, "SaveButton_Click: Непередбачена помилка при збереженні профілю {UserId}.", userId);
                }
            }
        }
    }
}