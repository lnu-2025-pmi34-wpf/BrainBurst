using System.Windows;

namespace BrainBurst.Presentation
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            bool isLoginSuccessful = true;

            if (isLoginSuccessful)
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