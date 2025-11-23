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

    /// <summary>
    /// Логіка взаємодії для View зміни пароля користувача.
    /// </summary>
    public partial class ChangePasswordView : UserControl
    {
        private readonly IUserService _userService;
        private readonly IAuthContext _authContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangePasswordView"/> class.
        /// </summary>
        /// <param name="userService">Сервіс для зміни даних користувача.</param>
        /// <param name="authContext">Контекст автентифікації для отримання ID поточного користувача.</param>
        public ChangePasswordView(IUserService userService, IAuthContext authContext)
        {
            this.InitializeComponent();
            this._userService = userService;
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
            string oldPassword = this.OldPasswordBox.Password;
            string newPassword = this.NewPasswordBox.Password;
            string confirmPassword = this.ConfirmPasswordBox.Password;

            this.StatusText.Text = string.Empty;
            this.StatusText.Foreground = Brushes.Red;

            if (newPassword != confirmPassword)
            {
                this.StatusText.Text = "Нові паролі не співпадають.";
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
            }
            catch (ArgumentException ex)
            {
                this.StatusText.Text = ex.Message;
            }
            catch (KeyNotFoundException)
            {
                this.StatusText.Text = "Помилка. Користувача не знайдено.";
            }
            catch (Exception)
            {
                this.StatusText.Text = "Помилка. Не вдалося змінити пароль.";
            }
        }
    }
}