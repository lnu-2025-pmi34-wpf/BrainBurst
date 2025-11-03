using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace BrainBurst.Presentation.Views
{
    public partial class CreateTestView : UserControl
    {
        public CreateTestView()
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

        private void GenerateTest_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.GetNavigationService(this).CanGoBack)
            {
                NavigationService.GetNavigationService(this).GoBack();
            }
        }
    }
}