using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Media;
namespace BrainBurst.Presentation.Views
{
    public partial class EditProfileView : UserControl
    {
        public EditProfileView()
        {
            InitializeComponent();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.GetNavigationService(this).CanGoBack)
            {
                NavigationService.GetNavigationService(this).GoBack();
            }
        }

        private void ChangeUsernameButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeUsernamePanel.Visibility = Visibility.Visible;
            UsernameStatusText.Text = "";
            NewUsernameTextBox.Focus();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ChangeUsernamePanel.Visibility == Visibility.Visible)
            {
                string newName = NewUsernameTextBox.Text;

                bool success = true; 

                if (success)
                {
                    UsernameStatusText.Text = "Ім'я успішно змінено!";
                    UsernameStatusText.Foreground = Brushes.Green;
                }
                else
                {
                    UsernameStatusText.Text = "Помилка. Не вдалося змінити ім'я.";
                    UsernameStatusText.Foreground = Brushes.Red;
                }
            }
        }
    }
}