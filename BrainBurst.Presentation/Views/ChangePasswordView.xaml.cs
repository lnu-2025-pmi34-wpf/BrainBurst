namespace BrainBurst.Presentation.Views
{
    using System;
    using System.Collections.Generic; // Додано для KeyNotFoundException
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Navigation;
    using BrainBurst.BLL.Interfaces; // Додаємо для IUserService та IAuthContext

    public partial class ChangePasswordView : UserControl
    {
        // УСУНЕНО: private const int CurrentUserId = 1;

        private readonly IUserService _userService;
        private readonly IAuthContext _authContext;

        // ОНОВЛЕНО: Конструктор приймає IAuthContext
        public ChangePasswordView(IUserService userService, IAuthContext authContext)
        {
            this.InitializeComponent();
            this._userService = userService;
            this._authContext = authContext; // <--- ІНІЦІАЛІЗОВАНО
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
                // ВИКЛИК РЕАЛЬНОЇ ЛОГІКИ ЗМІНИ ПАРОЛЮ
                // ВИКОРИСТАННЯ: CurrentUserId замінено на _authContext.CurrentUserId
                await this._userService.ChangePasswordAsync(this._authContext.CurrentUserId, oldPassword, newPassword, CancellationToken.None);

                // Успіх
                this.StatusText.Text = "Пароль успішно змінено!";
                this.StatusText.Foreground = Brushes.Green;

                this.OldPasswordBox.Password = string.Empty;
                this.NewPasswordBox.Password = string.Empty;
                this.ConfirmPasswordBox.Password = string.Empty;
            }
            catch (ArgumentException ex)
            {
                // Помилка валідації або невірний старий пароль
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