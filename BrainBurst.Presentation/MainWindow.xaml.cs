namespace BrainBurst.Presentation
{
    using System.Windows;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Логіка взаємодії для головного вікна додатку (MainWindow.xaml).
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MainWindow> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        /// <param name="serviceProvider">Постачальник служб DI (для отримання інших вікон).</param>
        /// <param name="logger">Логер для запису подій.</param>
        public MainWindow(IServiceProvider serviceProvider, ILogger<MainWindow> logger)
        {
            this.InitializeComponent();
            this._serviceProvider = serviceProvider;
            this._logger = logger;

            this._logger.LogInformation("MainWindow: Головне вікно-оболонка ініціалізовано.");
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            this._logger.LogInformation("RegisterButton_Click: Запущено процес реєстрації.");

            RegistrationWindow registrationWindow = this._serviceProvider.GetRequiredService<RegistrationWindow>();
            this.Hide();

            bool? result = registrationWindow.ShowDialog();

            if (result == true)
            {
                this._logger.LogInformation("RegisterButton_Click: Реєстрація успішна (DialogResult = True). Перехід до профілю.");
                ProfileWindow profileWindow = this._serviceProvider.GetRequiredService<ProfileWindow>();
                profileWindow.Show();

                this.Close();
                this._logger.LogDebug("RegisterButton_Click: MainWindow закрито.");
            }
            else
            {
                this._logger.LogInformation("RegisterButton_Click: Реєстрація скасована або невдала. Повернення до MainWindow.");
                this.Show();
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            this._logger.LogInformation("LoginButton_Click: Запущено процес входу.");

            LoginWindow loginWindow = this._serviceProvider.GetRequiredService<LoginWindow>();
            this.Hide();

            bool? result = loginWindow.ShowDialog();

            if (result == true)
            {
                this._logger.LogInformation("LoginButton_Click: Вхід успішний (DialogResult = True). Перехід до профілю.");

                ProfileWindow profileWindow = this._serviceProvider.GetRequiredService<ProfileWindow>();
                profileWindow.Show();

                this.Close();
                this._logger.LogDebug("LoginButton_Click: MainWindow закрито.");
            }
            else
            {
                this._logger.LogInformation("LoginButton_Click: Вхід скасовано або невдалий. Повернення до MainWindow.");
                this.Show();
            }
        }
    }
}