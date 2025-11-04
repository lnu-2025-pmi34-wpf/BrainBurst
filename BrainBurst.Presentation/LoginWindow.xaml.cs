using System.Windows;
using BrainBurst.BLL.Interfaces; // Додаємо для IAuthService
using System;
using System.Threading.Tasks;
using System.Windows.Media;

namespace BrainBurst.Presentation
{
    public partial class LoginWindow : Window
    {
        private readonly IAuthService _authService;
        private readonly IServiceProvider _serviceProvider; // Потрібен для ProfileWindow

        // Оновлюємо конструктор для отримання залежностей
        public LoginWindow(IAuthService authService, IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _authService = authService;
            _serviceProvider = serviceProvider;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text;
            string password = PasswordInputBox.Password;

            StatusText.Text = "";
            StatusText.Foreground = Brushes.Red;

            try
            {
                // ВИКЛИК РЕАЛЬНОЇ ЛОГІКИ АУТЕНТИФІКАЦІЇ
                await _authService.LoginAsync(email, password, CancellationToken.None);

                // УСПІШНИЙ ВХІД
                this.DialogResult = true;
                this.Close();
            }
            catch (ArgumentException ex)
            {
                // Помилка валідації (наприклад, некоректний email/пароль)
                StatusText.Text = ex.Message;
            }
            catch (KeyNotFoundException ex)
            {
                // Помилка, коли користувача не знайдено
                StatusText.Text = ex.Message;
            }
            catch (Exception)
            {
                // Інші помилки (наприклад, проблеми з БД або мережею)
                StatusText.Text = "Непередбачена помилка входу. Спробуйте пізніше.";
            }
        }
    }
}