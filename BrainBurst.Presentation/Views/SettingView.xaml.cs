namespace BrainBurst.Presentation.Views
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Navigation;
    using BrainBurst.Presentation.Views;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Логіка взаємодії для View налаштувань користувача.
    /// Містить кнопки для зміни пароля, видалення акаунту та виходу із системи.
    /// </summary>
    public partial class SettingsView : UserControl
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsView"/> class.
        /// </summary>
        /// <param name="serviceProvider">Постачальник служб DI (для навігації та доступу до MainWindow).</param>
        public SettingsView(IServiceProvider serviceProvider)
        {
            this.InitializeComponent();
            this._serviceProvider = serviceProvider;
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
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
                var changePasswordView = this._serviceProvider.GetRequiredService<ChangePasswordView>();
                NavigationService.GetNavigationService(this).Navigate(changePasswordView);
            }
        }

        private void DeleteAccountButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.GetNavigationService(this) != null)
            {
                var deleteAccountView = this._serviceProvider.GetRequiredService<DeleteAccountView>();
                NavigationService.GetNavigationService(this).Navigate(deleteAccountView);
            }
        }
    }
}