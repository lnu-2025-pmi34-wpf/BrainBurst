namespace BrainBurst.Presentation
{
    using System.Windows;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Логіка взаємодії для головного вікна додатку (MainWindow.xaml).
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        /// <param name="serviceProvider">Постачальник служб DI (для отримання інших вікон).</param>
        public MainWindow(IServiceProvider serviceProvider)
        {
            this.InitializeComponent();
            this._serviceProvider = serviceProvider;
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            RegistrationWindow registrationWindow = this._serviceProvider.GetRequiredService<RegistrationWindow>();
            this.Hide();

            bool? result = registrationWindow.ShowDialog();

            if (result == true)
            {
                ProfileWindow profileWindow = this._serviceProvider.GetRequiredService<ProfileWindow>();
                profileWindow.Show();

                this.Close();
            }
            else
            {
                this.Show();
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = this._serviceProvider.GetRequiredService<LoginWindow>();
            this.Hide();

            bool? result = loginWindow.ShowDialog();

            if (result == true)
            {
                ProfileWindow profileWindow = this._serviceProvider.GetRequiredService<ProfileWindow>();
                profileWindow.Show();

                this.Close();
            }
            else
            {
                this.Show();
            }
        }
    }
}