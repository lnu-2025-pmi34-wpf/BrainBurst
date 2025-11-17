namespace BrainBurst.Presentation
{
    using System;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Media;
    using BrainBurst.BLL.Interfaces;

    public partial class RegistrationWindow : Window
    {
        private readonly IAuthService _authService;
        private readonly IAuthContext _authContext; // <--- ДОДАНО ПОЛЕ

        // Оновлюємо конструктор для отримання залежностей
        public RegistrationWindow(IAuthService authService, IAuthContext authContext) // <--- ДОДАНО IAuthContext
        {
            this.InitializeComponent();
            this._authService = authService;
            this._authContext = authContext; // <--- ІНІЦІАЛІЗАЦІЯ
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private async void Register_Click(object sender, RoutedEventArgs e)
        {
            string email = this.EmailTextBox.Text;
            string fullName = this.FullNameTextBox.Text;
            string password = this.PasswordInputBox.Password;

            this.StatusText.Text = string.Empty;
            this.StatusText.Foreground = Brushes.Red;

            try
            {
                // ВИКЛИК РЕАЛЬНОЇ ЛОГІКИ РЕЄСТРАЦІЇ
                var userDto = await this._authService.RegisterAsync(email, password, fullName, CancellationToken.None); // <--- ЗБЕРЕЖЕНО DTO

                // ВСТАНОВЛЕННЯ КОНТЕКСТУ ПІСЛЯ УСПІШНОЇ РЕЄСТРАЦІЇ
                this._authContext.SetCurrentUser(userDto); // <--- ВИКЛИК SetCurrentUser

                // УСПІШНА РЕЄСТРАЦІЯ
                this.DialogResult = true;
                this.Close();
            }
            catch (ArgumentException ex)
            {
                // Помилка валідації (наприклад, некоректний email, короткий пароль, email вже існує)
                this.StatusText.Text = ex.Message;
            }
            catch (Exception ex) // <-- Додано змінну ex, щоб показати внутрішню помилку
            {
                // Інші помилки (наприклад, проблеми з БД або мережею)
                // ТЕПЕР ПОКАЗУЄ ВНУТРІШНЮ ПОМИЛКУ БД
                this.StatusText.Text = $"Непередбачена помилка реєстрації: {ex.InnerException?.Message ?? ex.Message}. Спробуйте пізніше.";
            }
        }
    }
}