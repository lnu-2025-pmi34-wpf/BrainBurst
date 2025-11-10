using System.Windows;
using BrainBurst.BLL.Interfaces; 
using System;
using System.Threading.Tasks;
using System.Windows.Media;

namespace BrainBurst.Presentation
{
    public partial class RegistrationWindow : Window
    {
        private readonly IAuthService _authService;
        private readonly IAuthContext _authContext; // <--- ДОДАНО ПОЛЕ

        // Оновлюємо конструктор для отримання залежностей
        public RegistrationWindow(IAuthService authService, IAuthContext authContext) // <--- ДОДАНО IAuthContext
        {
            InitializeComponent();
            _authService = authService;
            _authContext = authContext; // <--- ІНІЦІАЛІЗАЦІЯ
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
                var userDto = await _authService.RegisterAsync(email, password, fullName, CancellationToken.None); // <--- ЗБЕРЕЖЕНО DTO

                // ВСТАНОВЛЕННЯ КОНТЕКСТУ ПІСЛЯ УСПІШНОЇ РЕЄСТРАЦІЇ
                _authContext.SetCurrentUser(userDto); // <--- ВИКЛИК SetCurrentUser

                // УСПІШНА РЕЄСТРАЦІЯ
                this.DialogResult = true;
                this.Close();
            }
            catch (ArgumentException ex)
            {
                // Помилка валідації (наприклад, некоректний email, короткий пароль, email вже існує)
                StatusText.Text = ex.Message;
            }
            catch (Exception ex) // <-- Додано змінну ex, щоб показати внутрішню помилку
            {
                // Інші помилки (наприклад, проблеми з БД або мережею)
                // ТЕПЕР ПОКАЗУЄ ВНУТРІШНЮ ПОМИЛКУ БД
                StatusText.Text = $"Непередбачена помилка реєстрації: {ex.InnerException?.Message ?? ex.Message}. Спробуйте пізніше.";
            }
        }
    }
}