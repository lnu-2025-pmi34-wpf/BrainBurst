namespace BrainBurst.Presentation
{
    using System;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Media;
    using BrainBurst.BLL.Interfaces;

    /// <summary>
    /// Логіка взаємодії для вікна реєстрації користувача (RegistrationWindow.xaml).
    /// </summary>
    public partial class RegistrationWindow : Window
    {
        private readonly IAuthService _authService;
        private readonly IAuthContext _authContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrationWindow"/> class.
        /// </summary>
        /// <param name="authService">Сервіс для виконання логіки реєстрації.</param>
        /// <param name="authContext">Контекст для встановлення поточного користувача після успішної реєстрації.</param>
        public RegistrationWindow(IAuthService authService, IAuthContext authContext)
        {
            this.InitializeComponent();
            this._authService = authService;
            this._authContext = authContext;
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
                var userDto = await this._authService.RegisterAsync(email, password, fullName, CancellationToken.None);

                this._authContext.SetCurrentUser(userDto);

                this.DialogResult = true;
                this.Close();
            }
            catch (ArgumentException ex)
            {
                this.StatusText.Text = ex.Message;
            }
            catch (Exception ex)
            {
                this.StatusText.Text = $"Непередбачена помилка реєстрації: {ex.InnerException?.Message ?? ex.Message}. Спробуйте пізніше.";
            }
        }
    }
}