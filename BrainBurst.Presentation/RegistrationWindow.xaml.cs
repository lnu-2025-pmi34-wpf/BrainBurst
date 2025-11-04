using System.Windows;
using BrainBurst.BLL.Interfaces; // Додаємо для IAuthService
using System;
using System.Threading.Tasks;
using System.Windows.Media;

namespace BrainBurst.Presentation
{
    public partial class RegistrationWindow : Window
    {
        private readonly IAuthService _authService;

        // Оновлюємо конструктор для отримання залежностей
        public RegistrationWindow(IAuthService authService)
        {
            InitializeComponent();
            _authService = authService;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

       private async void Register_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text;
            string fullName = FullNameTextBox.Text;
            string password = PasswordInputBox.Password;

            StatusText.Text = "";
            StatusText.Foreground = Brushes.Red;

            try
            {
                // ВИКЛИК РЕАЛЬНОЇ ЛОГІКИ РЕЄСТРАЦІЇ
                await _authService.RegisterAsync(email, password, fullName, CancellationToken.None);

                // УСПІШНА РЕЄСТРАЦІЯ
                this.DialogResult = true;
                this.Close();
            }
            catch (ArgumentException ex)
            {
                // Помилка валідації (наприклад, некоректний email, короткий пароль, email вже існує)
                StatusText.Text = ex.Message;
            }
            catch (Exception ex) // <-- Додаємо змінну ex
            {
                // Інші помилки (наприклад, проблеми з БД або мережею)
                StatusText.Text = $"Непередбачена помилка реєстрації: {ex.InnerException?.Message ?? ex.Message}. Спробуйте пізніше.";
            }
        }
    }
}