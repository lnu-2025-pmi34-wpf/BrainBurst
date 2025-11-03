using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace BrainBurst.Presentation.Views
{
    public partial class ProfileView : UserControl
    {
        public ProfileView()
        {
            InitializeComponent();
        }

        private void EditProfileButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.GetNavigationService(this) != null)
            {
                NavigationService.GetNavigationService(this).Navigate(new EditProfileView());
            }
        }

        private void ArchiveButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.GetNavigationService(this) != null)
            {
                NavigationService.GetNavigationService(this).Navigate(new ArchiveView());
            }
        }
    }
}