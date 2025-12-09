namespace BrainBurst.Presentation
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Windows;
    using System.Windows.Media;
    using BrainBurst.BLL.Interfaces;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Логіка взаємодії для вікна входу користувача (LoginWindow.xaml).
    /// </summary>
    public partial class LoginWindow : Window
    {
        private readonly IAuthService _authService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IAuthContext _authContext;
        private readonly ILogger<LoginWindow> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginWindow"/> class.
        /// </summary>
        /// <param name="authService">Сервіс для автентифікації.</param>
        /// <param name="serviceProvider">Постачальник служб DI.</param>
        /// <param name="authContext">Контекст для встановлення поточного користувача.</param>
        /// <param name="logger">Логер для запису подій. </param>
        public LoginWindow(IAuthService authService, IServiceProvider serviceProvider, IAuthContext authContext, ILogger<LoginWindow> logger)
        {
            this.InitializeComponent();
            this._authService = authService;
            this._serviceProvider = serviceProvider;
            this._authContext = authContext;
            this._logger = logger;

            this._logger.LogInformation("LoginWindow: Вікно входу ініціалізовано.");
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Закриваємо вікно входу і повертаємось на головну сторінку вибору
            this.Close();
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            string email = this.EmailTextBox.Text;
            string password = this.PasswordInputBox.Password;

            this.StatusText.Text = string.Empty;
            this.StatusText.Foreground = Brushes.Red;

            this._logger.LogInformation("Login_Click: Спроба входу для користувача {Email}", email);
            try
            {
                // Виконуємо вхід
                var userDto = await this._authService.LoginAsync(email, password, CancellationToken.None);

                this._logger.LogInformation("Login_Click: Користувач {Email} успішно авторизований. Перехід до профілю.", email);

                // Встановлюємо поточного користувача в контекст
                this._authContext.SetCurrentUser(userDto);

                this.DialogResult = true;
                this.Close();
            }
            catch (ArgumentException ex)
            {
                this._logger.LogWarning(ex, "Login_Click: Помилка валідації входу для {Email}: {Message}", email, ex.Message);
                this.StatusText.Text = ex.Message;
            }
            catch (KeyNotFoundException ex)
            {
                this._logger.LogWarning(ex, "Login_Click: Користувача {Email} не знайдено (KeyNotFoundException).", email);
                this.StatusText.Text = ex.Message;
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Login_Click: Непередбачена критична помилка входу для {Email}", email);
                this.StatusText.Text = "Непередбачена помилка входу. Спробуйте пізніше.";
            }
        }
    }
}