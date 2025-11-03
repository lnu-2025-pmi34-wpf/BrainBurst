using BrainBurst.Presentation.Views;
using System.Windows;
using System.Windows.Controls;

namespace BrainBurst.Presentation
{
    public partial class ProfileWindow : Window
    {
        public ProfileWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new ProfileView());
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ProfileView());
        }

        private void CardsButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new CardsView());
        }

        private void TestsButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new TestsView());
        }

        private void AwardsButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new AwardsView());
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new SettingsView());
        }
    }
}