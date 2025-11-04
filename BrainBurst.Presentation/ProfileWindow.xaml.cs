using BrainBurst.Presentation.Views;
using System.Windows;
using System.Windows.Controls;
using System;
using Microsoft.Extensions.DependencyInjection; // Додаємо для DI
using Microsoft.Extensions.DependencyInjection; // Додаємо для DI
namespace BrainBurst.Presentation
{
    public partial class ProfileWindow : Window
    {
        private readonly IServiceProvider _serviceProvider;

        // Отримуємо IServiceProvider через DI
      public ProfileWindow(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
            
            try
            {
                // Запускаємо стартовий View, отримуючи його з DI
                MainFrame.Navigate(_serviceProvider.GetRequiredService<ProfileView>());
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
            MainFrame.Navigate(_serviceProvider.GetRequiredService<ProfileView>());
        }

        private void CardsButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(_serviceProvider.GetRequiredService<CardsView>());
        }

        private void TestsButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(_serviceProvider.GetRequiredService<TestsView>());
        }

        private void AwardsButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(_serviceProvider.GetRequiredService<AwardsView>());
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            // Виправляємо помилку CS7036: тепер SettingsView створюється через DI
            MainFrame.Navigate(_serviceProvider.GetRequiredService<SettingsView>());
        }
    }
}