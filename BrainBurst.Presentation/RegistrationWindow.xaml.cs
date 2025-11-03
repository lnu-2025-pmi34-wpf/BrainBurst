using System.Windows;

namespace BrainBurst.Presentation
{
    public partial class RegistrationWindow : Window
    {
        public RegistrationWindow()
        {
            InitializeComponent();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            bool isRegistrationSuccessful = true;
            if (isRegistrationSuccessful)
            {
                this.DialogResult = true;
                this.Close();
            }
            else
            {
            
            }
        }
    }
}