namespace BrainBurst.Presentation
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using BrainBurst.Presentation.Views;
    using Microsoft.Extensions.DependencyInjection; // Додаємо для DI
    using Microsoft.Extensions.DependencyInjection; // Додаємо для DI

    public partial class ProfileWindow : Window
    {
        private readonly IServiceProvider _serviceProvider;

        // Отримуємо IServiceProvider через DI
        public ProfileWindow(IServiceProvider serviceProvider)
        {
            this.InitializeComponent();
            this._serviceProvider = serviceProvider;

            try
            {
                // Запускаємо стартовий View, отримуючи його з DI
                this.MainFrame.Navigate(this._serviceProvider.GetRequiredService<ProfileView>());
            }
            catch (Exception ex)
            {
                // ВИВЕДЕННЯ КРИТИЧНОЇ ПОМИЛКИ В ДЕБАГ КОНСОЛЬ
                MessageBox.Show($"Критична помилка при завантаженні профілю: {ex.Message} (Inner: {ex.InnerException?.Message})", "Критична помилка");
                Application.Current.Shutdown(); // Закриваємо додаток після повідомлення
            }
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            this.MainFrame.Navigate(this._serviceProvider.GetRequiredService<ProfileView>());
        }

        private void CardsButton_Click(object sender, RoutedEventArgs e)
        {
            this.MainFrame.Navigate(this._serviceProvider.GetRequiredService<CardsView>());
        }

        private void TestsButton_Click(object sender, RoutedEventArgs e)
        {
            this.MainFrame.Navigate(this._serviceProvider.GetRequiredService<TestsView>());
        }

        private void AwardsButton_Click(object sender, RoutedEventArgs e)
        {
            this.MainFrame.Navigate(this._serviceProvider.GetRequiredService<AwardsView>());
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            // Виправляємо помилку CS7036: тепер SettingsView створюється через DI
            this.MainFrame.Navigate(this._serviceProvider.GetRequiredService<SettingsView>());
        }
    }
}