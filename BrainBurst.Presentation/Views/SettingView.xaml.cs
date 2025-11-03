using System.Windows;
using System.Windows.Controls;
using BrainBurst.Presentation.Views;
using System.Windows.Navigation;

namespace BrainBurst.Presentation.Views
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
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
                NavigationService.GetNavigationService(this).Navigate(new ChangePasswordView());
            }
        }

        private void DeleteAccountButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.GetNavigationService(this) != null)
            {
                NavigationService.GetNavigationService(this).Navigate(new DeleteAccountView());
            }
        }
    }
}