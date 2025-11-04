using System.Windows;
using Microsoft.Extensions.DependencyInjection; // Додаємо для доступу до DI

namespace BrainBurst.Presentation
{
    public partial class MainWindow : Window
    {
        // Видаляємо створення вікон вручну, отримуємо їх через DI
        private readonly IServiceProvider _serviceProvider;

        public MainWindow(IServiceProvider serviceProvider) // Отримуємо IServiceProvider
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
        }

        // ... (LoginButton_Click та RegisterButton_Click)
        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            // Отримуємо вікно з DI
            RegistrationWindow registrationWindow = _serviceProvider.GetRequiredService<RegistrationWindow>();
            this.Hide();

            bool? result = registrationWindow.ShowDialog();

            if (result == true)
            {
                // Отримуємо ProfileWindow з DI
                ProfileWindow profileWindow = _serviceProvider.GetRequiredService<ProfileWindow>();
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
            // Отримуємо вікно з DI
            LoginWindow loginWindow = _serviceProvider.GetRequiredService<LoginWindow>();
            this.Hide();

            bool? result = loginWindow.ShowDialog();

            if (result == true)
            {
                // Отримуємо ProfileWindow з DI
                ProfileWindow profileWindow = _serviceProvider.GetRequiredService<ProfileWindow>();
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