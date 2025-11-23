namespace BrainBurst.Presentation
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using BrainBurst.Presentation.Views;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Логіка взаємодії для вікна профілю користувача (ProfileWindow.xaml).
    /// Містить навігацію для різних розділів профілю (Картки, Тести, Нагороди).
    /// </summary>
    public partial class ProfileWindow : Window
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProfileWindow"/> class.
        /// </summary>
        /// <param name="serviceProvider">Постачальник служб DI (для отримання різних View).</param>
        public ProfileWindow(IServiceProvider serviceProvider)
        {
            this.InitializeComponent();
            this._serviceProvider = serviceProvider;

            try
            {
                this.MainFrame.Navigate(this._serviceProvider.GetRequiredService<ProfileView>());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Критична помилка при завантаженні профілю: {ex.Message} (Inner: {ex.InnerException?.Message})", "Критична помилка");
                Application.Current.Shutdown();
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
            this.MainFrame.Navigate(this._serviceProvider.GetRequiredService<SettingsView>());
        }
    }
}