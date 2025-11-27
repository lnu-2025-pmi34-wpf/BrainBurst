namespace BrainBurst.Presentation
{
    using System;
    using System.Threading;
    using System.Windows;
    using System.Windows.Media;
    using BrainBurst.BLL.Interfaces;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Логіка взаємодії для вікна входу користувача (LoginWindow.xaml).
    /// </summary>
    public partial class LoginWindow : Window
    {
        private readonly IAuthService _authService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IAuthContext _authContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginWindow"/> class.
        /// </summary>
        /// <param name="authService">Сервіс для автентифікації.</param>
        /// <param name="serviceProvider">Постачальник служб DI.</param>
        /// <param name="authContext">Контекст для встановлення поточного користувача.</param>
        public LoginWindow(IAuthService authService, IServiceProvider serviceProvider, IAuthContext authContext)
        {
            this.InitializeComponent();
            this._authService = authService;
            this._serviceProvider = serviceProvider;
            this._authContext = authContext;
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

            try
            {
                // Виконуємо вхід
                var userDto = await this._authService.LoginAsync(email, password, CancellationToken.None);

                // Встановлюємо поточного користувача в контекст
                this._authContext.SetCurrentUser(userDto);

                // Отримуємо та показуємо вікно профілю
                var profileWindow = this._serviceProvider.GetRequiredService<ProfileWindow>();
                profileWindow.Show();

                // Закриваємо вікно входу
                this.Close();
            }
            catch (ArgumentException ex)
            {
                this.StatusText.Text = ex.Message;
            }
            catch (KeyNotFoundException ex)
            {
                this.StatusText.Text = ex.Message;
            }
            catch (Exception ex)
            {
                this.StatusText.Text = "Непередбачена помилка входу. Спробуйте пізніше.";
            }
        }
    }
}