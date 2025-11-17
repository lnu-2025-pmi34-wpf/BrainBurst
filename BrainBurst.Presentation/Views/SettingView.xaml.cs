namespace BrainBurst.Presentation.Views
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Navigation;
    using BrainBurst.Presentation.Views;
    using Microsoft.Extensions.DependencyInjection; // Додаємо для DI

    public partial class SettingsView : UserControl
    {
        private readonly IServiceProvider _serviceProvider;

        // Конструктор вже був оновлений на кроці 15, додаємо using для DI
        public SettingsView(IServiceProvider serviceProvider)
        {
            this.InitializeComponent();
            this._serviceProvider = serviceProvider;
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            // Отримуємо MainWindow з DI-контейнера
            MainWindow mainWindow = this._serviceProvider.GetRequiredService<MainWindow>();
            Window currentWindow = Window.GetWindow(this);

            mainWindow.Show();

            if (currentWindow != null)
            {
                currentWindow.Close();
            }
        }

        private void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.GetNavigationService(this) != null)
            {
                // ВИПРАВЛЕНО: Створюємо ChangePasswordView через DI
                var changePasswordView = this._serviceProvider.GetRequiredService<ChangePasswordView>();
                NavigationService.GetNavigationService(this).Navigate(changePasswordView);
            }
        }

        private void DeleteAccountButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.GetNavigationService(this) != null)
            {
                // ВИПРАВЛЕНО: Створюємо DeleteAccountView через DI
                var deleteAccountView = this._serviceProvider.GetRequiredService<DeleteAccountView>();
                NavigationService.GetNavigationService(this).Navigate(deleteAccountView);
            }
        }
    }
}