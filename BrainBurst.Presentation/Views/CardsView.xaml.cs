using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace BrainBurst.Presentation.Views
{
    public partial class CardsView : UserControl
    {
        public CardsView()
        {
            InitializeComponent();
        }

        private void Deck_Click(object sender, MouseButtonEventArgs e)
        {
            if (NavigationService.GetNavigationService(this) != null)
            {
                NavigationService.GetNavigationService(this).Navigate(new StudyView());
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.GetNavigationService(this) != null)
            {
                NavigationService.GetNavigationService(this).Navigate(new CreateCardView());
            }
        }
    }
}