using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Input;

namespace BrainBurst.Presentation.Views
{
    public partial class TestsView : UserControl
    {
        public TestsView()
        {
            InitializeComponent();
        }

        private void Test_Click(object sender, MouseButtonEventArgs e)
        {
            if (NavigationService.GetNavigationService(this) != null)
            {
                NavigationService.GetNavigationService(this).Navigate(new TestTakingView());
            }
        }
        private void CreateTest_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.GetNavigationService(this) != null)
            {
                NavigationService.GetNavigationService(this).Navigate(new CreateTestView());
            }
        }
    }
}