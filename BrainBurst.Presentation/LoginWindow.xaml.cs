namespace BrainBurst.Presentation
{
    using System;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Media;
    using BrainBurst.BLL.Interfaces;

    /// <summary>
    /// Логіка взаємодії для вікна входу користувача (LoginWindow.xaml).
    /// </summary>
    public partial class LoginWindow : Window
    {
        private readonly IAuthService _authService;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginWindow"/> class.
        /// </summary>
        /// <param name="authService">Сервіс для автентифікації.</param>
        /// <param name="serviceProvider">Постачальник служб DI.</param>
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
                await this._authService.LoginAsync(email, password, CancellationToken.None);

                this.DialogResult = true;
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
            catch (Exception)
            {
                this.StatusText.Text = "Непередбачена помилка входу. Спробуйте пізніше.";
            }
        }
    }
}