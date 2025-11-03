using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace BrainBurst.Presentation.Views
{
    public partial class DeleteAccountView : UserControl
    {
        public DeleteAccountView()
        {
            InitializeComponent();
        }
        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            GoBack();
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            GoBack();
        }

        private void GoBack()
        {
            if (NavigationService.GetNavigationService(this) != null &&
                NavigationService.GetNavigationService(this).CanGoBack)
            {
                NavigationService.GetNavigationService(this).GoBack();
            }
        }
    }
}