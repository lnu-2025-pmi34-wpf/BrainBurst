using System.Windows;
using System.Windows.Controls;
using System.Windows.Media; 
using System.Windows.Navigation; 
using BrainBurst.BLL.Interfaces; // Додаємо для IUserService та IAuthContext
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic; // Додано для KeyNotFoundException

namespace BrainBurst.Presentation.Views
{
    public partial class ChangePasswordView : UserControl
    {
        // УСУНЕНО: private const int CurrentUserId = 1;

        private readonly IUserService _userService;
        private readonly IAuthContext _authContext; // <--- ДОДАНО ПОЛЕ

        // ОНОВЛЕНО: Конструктор приймає IAuthContext
        public ChangePasswordView(IUserService userService, IAuthContext authContext)
        {
            InitializeComponent();
            _userService = userService;
            _authContext = authContext; // <--- ІНІЦІАЛІЗОВАНО
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
            string oldPassword = OldPasswordBox.Password;
            string newPassword = NewPasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;

            StatusText.Text = "";
            StatusText.Foreground = Brushes.Red;

            if (newPassword != confirmPassword)
            {
                StatusText.Text = "Нові паролі не співпадають.";
                return;
            }

            try
            {
                // ВИКЛИК РЕАЛЬНОЇ ЛОГІКИ ЗМІНИ ПАРОЛЮ
                // ВИКОРИСТАННЯ: CurrentUserId замінено на _authContext.CurrentUserId
                await _userService.ChangePasswordAsync(_authContext.CurrentUserId, oldPassword, newPassword, CancellationToken.None);

                // Успіх
                StatusText.Text = "Пароль успішно змінено!";
                StatusText.Foreground = Brushes.Green;

                OldPasswordBox.Password = "";
                NewPasswordBox.Password = "";
                ConfirmPasswordBox.Password = "";
            }
            catch (ArgumentException ex)
            {
                // Помилка валідації або невірний старий пароль
                StatusText.Text = ex.Message;
            }
            catch (KeyNotFoundException)
            {
                StatusText.Text = "Помилка. Користувача не знайдено.";
            }
            catch (Exception)
            {
                StatusText.Text = "Помилка. Не вдалося змінити пароль.";
            }
        }
    }
}