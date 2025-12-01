namespace BrainBurst.Presentation
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Media;
    using BrainBurst.BLL.Interfaces;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Логіка взаємодії для вікна реєстрації користувача (RegistrationWindow.xaml).
    /// </summary>
    public partial class RegistrationWindow : Window
    {
        private readonly IAuthService _authService;
        private readonly IAuthContext _authContext;
        private readonly ILogger<RegistrationWindow> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrationWindow"/> class.
        /// </summary>
        /// <param name="authService">Сервіс для виконання логіки реєстрації.</param>
        /// <param name="authContext">Контекст для встановлення поточного користувача після успішної реєстрації.</param>
        /// <param name="logger">Логер для запису подій.</param>
        public RegistrationWindow(IAuthService authService, IAuthContext authContext, ILogger<RegistrationWindow> logger)
        {
            this.InitializeComponent();
            this._authService = authService;
            this._authContext = authContext;
            this._logger = logger;

            this._logger.LogInformation("RegistrationWindow: Вікно реєстрації ініціалізовано.");
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

            this._logger.LogInformation("Register_Click: Спроба реєстрації для email: {Email}", email);

            try
            {
                var userDto = await this._authService.RegisterAsync(email, password, fullName, CancellationToken.None);

                this._authContext.SetCurrentUser(userDto);

                this.DialogResult = true;
                this.Close();

                this._logger.LogInformation("Register_Click: Користувач {Email} успішно зареєстрований.", email);
            }
            catch (ArgumentException ex)
            {
                this._logger.LogWarning(ex, "Register_Click: Помилка реєстрації для {Email}: {Message}", email, ex.Message);
                this.StatusText.Text = ex.Message;
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Register_Click: Непередбачена критична помилка реєстрації для {Email}", email);  
                this.StatusText.Text = $"Непередбачена помилка реєстрації: {ex.InnerException?.Message ?? ex.Message}. Спробуйте пізніше.";
            }
        }
    }
}