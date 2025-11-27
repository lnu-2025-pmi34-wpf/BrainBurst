namespace BrainBurst.Presentation.Views
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Navigation;
    using BrainBurst.BLL.Interfaces;
    using BrainBurst.Presentation.Views;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Логіка взаємодії для View налаштувань користувача.
    /// Містить кнопки для зміни пароля, видалення акаунту та виходу із системи.
    /// </summary>
    public partial class SettingsView : UserControl
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IAuthContext _authContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsView"/> class.
        /// </summary>
        /// <param name="serviceProvider">Постачальник служб DI.</param>
        /// <param name="authContext">Контекст автентифікації для очищення сеансу користувача.</param>
        public SettingsView(IServiceProvider serviceProvider, IAuthContext authContext)
        {
            this.InitializeComponent();
            this._serviceProvider = serviceProvider;
            this._authContext = authContext;
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Очищуємо контекст автентифікації
                this._authContext.ClearContext();

                // Отримуємо поточне вікно
                Window currentWindow = Window.GetWindow(this);

                // Показуємо вікно входу
                var loginWindow = this._serviceProvider.GetRequiredService<LoginWindow>();
                loginWindow.Show();

                // Закриваємо поточне вікно
                if (currentWindow != null && currentWindow != loginWindow)
                {
                    currentWindow.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при виході: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
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