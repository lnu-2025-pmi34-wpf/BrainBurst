namespace BrainBurst.Presentation.Views
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Navigation;
    using BrainBurst.BLL.Interfaces;
    using BrainBurst.Presentation.Views;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Логіка взаємодії для View налаштувань користувача.
    /// Містить кнопки для зміни пароля, видалення акаунту та виходу із системи.
    /// </summary>
    public partial class SettingsView : UserControl
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IAuthContext _authContext;
        private readonly ILogger<SettingsView> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsView"/> class.
        /// </summary>
        /// <param name="serviceProvider">Постачальник служб DI.</param>
        /// <param name="authContext">Контекст автентифікації для очищення сеансу користувача.</param>
        /// <param name="logger">Логер для запису подій.</param>
        public SettingsView(IServiceProvider serviceProvider, IAuthContext authContext, ILogger<SettingsView> logger)
        {
            this.InitializeComponent();
            this._serviceProvider = serviceProvider;
            this._authContext = authContext;
            this._logger = logger;

            this._logger.LogDebug("SettingsView: View ініціалізовано.");
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            this._logger.LogInformation("LogoutButton_Click: Користувач ініціював вихід із системи.");

            try
            {
                // Очищуємо контекст автентифікації
                this._authContext.ClearContext();
                this._logger.LogInformation("LogoutButton_Click: Контекст автентифікації очищено.");

                // Отримуємо поточне вікно
                Window currentWindow = Window.GetWindow(this);

                // Показуємо вікно входу
                var mainWindow = this._serviceProvider.GetRequiredService<MainWindow>();
                mainWindow.Show();

                this._logger.LogInformation("LogoutButton_Click: Показано MainWindow.");

                // Закриваємо поточне вікно
                if (currentWindow != null && currentWindow != mainWindow)
                {
                    currentWindow.Close();
                    this._logger.LogInformation("LogoutButton_Click: Попереднє вікно закрито.");
                }
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "LogoutButton_Click: Критична помилка під час виходу з системи.");
                MessageBox.Show($"Помилка при виході: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            this._logger.LogInformation("ChangePasswordButton_Click: Навігація до ChangePasswordView.");
            if (NavigationService.GetNavigationService(this) != null)
            {
                var changePasswordView = this._serviceProvider.GetRequiredService<ChangePasswordView>();
                NavigationService.GetNavigationService(this).Navigate(changePasswordView);
            }
            else
            {
                this._logger.LogWarning("ChangePasswordButton_Click: NavigationService недоступний.");
            }
        }

        private void DeleteAccountButton_Click(object sender, RoutedEventArgs e)
        {
            this._logger.LogCritical("DeleteAccountButton_Click: Навігація до DeleteAccountView.");

            if (NavigationService.GetNavigationService(this) != null)
            {
                var deleteAccountView = this._serviceProvider.GetRequiredService<DeleteAccountView>();
                NavigationService.GetNavigationService(this).Navigate(deleteAccountView);
            }
            else
            {
                this._logger.LogError("DeleteAccountButton_Click: NavigationService недоступний. Невдала спроба навігації.");
            }
        }
    }
}