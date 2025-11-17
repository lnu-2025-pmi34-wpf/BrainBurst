namespace BrainBurst.Presentation
{
    using System;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Media;
    using BrainBurst.BLL.Interfaces;

    public partial class LoginWindow : Window
    {
        private readonly IAuthService _authService;
        private readonly IServiceProvider _serviceProvider; // Потрібен для ProfileWindow

        // Оновлюємо конструктор для отримання залежностей
        public LoginWindow(IAuthService authService, IServiceProvider serviceProvider)
        {
            this.InitializeComponent();
            this._authService = authService;
            this._serviceProvider = serviceProvider;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            string email = this.EmailTextBox.Text;
            string password = this.PasswordInputBox.Password;

            this.StatusText.Text = string.Empty;
            this.StatusText.Foreground = Brushes.Red;

            try
            {
                // ВИКЛИК РЕАЛЬНОЇ ЛОГІКИ АУТЕНТИФІКАЦІЇ
                await this._authService.LoginAsync(email, password, CancellationToken.None);

                // УСПІШНИЙ ВХІД
                this.DialogResult = true;
                this.Close();
            }
            catch (ArgumentException ex)
            {
                // Помилка валідації (наприклад, некоректний email/пароль)
                this.StatusText.Text = ex.Message;
            }
            catch (KeyNotFoundException ex)
            {
                // Помилка, коли користувача не знайдено
                this.StatusText.Text = ex.Message;
            }
            catch (Exception)
            {
                // Інші помилки (наприклад, проблеми з БД або мережею)
                this.StatusText.Text = "Непередбачена помилка входу. Спробуйте пізніше.";
            }
        }
    }
}