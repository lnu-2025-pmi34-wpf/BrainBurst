namespace BrainBurst.Presentation
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using BrainBurst.Presentation.Views;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Логіка взаємодії для вікна профілю користувача (ProfileWindow.xaml).
    /// Містить навігацію для різних розділів профілю (Картки, Тести, Нагороди).
    /// </summary>
    public partial class ProfileWindow : Window
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ProfileWindow> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProfileWindow"/> class.
        /// </summary>
        /// <param name="serviceProvider">Постачальник служб DI (для отримання різних View).</param>
        /// <param name="logger">Логер для запису подій.</param>
        public ProfileWindow(IServiceProvider serviceProvider, ILogger<ProfileWindow> logger)
        {
            this.InitializeComponent();
            this._serviceProvider = serviceProvider;

            this._logger = logger;

            this._logger.LogInformation("ProfileWindow: Вікно профілю ініціалізовано.");

            try
            {
                this._logger.LogDebug("ProfileWindow: Перша навігація до ProfileView.");
                this.MainFrame.Navigate(this._serviceProvider.GetRequiredService<ProfileView>());
            }
            catch (Exception ex)
            {
                this._logger.LogCritical(ex, "ProfileWindow: Критична помилка при завантаженні початкового ProfileView.");
                MessageBox.Show($"Критична помилка при завантаженні профілю: {ex.Message} (Inner: {ex.InnerException?.Message})", "Критична помилка");
                Application.Current.Shutdown();
            }
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            this._logger.LogInformation("ProfileButton_Click: Навігація до ProfileView.");
            this.MainFrame.Navigate(this._serviceProvider.GetRequiredService<ProfileView>());
        }

        private void CardsButton_Click(object sender, RoutedEventArgs e)
        {
            this._logger.LogInformation("CardsButton_Click: Навігація до CardsView.");
            this.MainFrame.Navigate(this._serviceProvider.GetRequiredService<CardsView>());
        }

        private void TestsButton_Click(object sender, RoutedEventArgs e)
        {
            this._logger.LogInformation("TestsButton_Click: Навігація до TestsView.");
            this.MainFrame.Navigate(this._serviceProvider.GetRequiredService<TestsView>());
        }

        private void AwardsButton_Click(object sender, RoutedEventArgs e)
        {
            this._logger.LogInformation("AwardsButton_Click: Навігація до AwardsView.");
            this.MainFrame.Navigate(this._serviceProvider.GetRequiredService<AwardsView>());
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            this._logger.LogInformation("SettingsButton_Click: Навігація до SettingsView.");
            this.MainFrame.Navigate(this._serviceProvider.GetRequiredService<SettingsView>());
        }
    }
}