using System.Windows;

namespace BrainBurst.Presentation
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            RegistrationWindow registrationWindow = new RegistrationWindow();
            this.Hide();

            bool? result = registrationWindow.ShowDialog();

            if (result == true)
            {
                ProfileWindow profileWindow = new ProfileWindow();
                profileWindow.Show();

                this.Close();
            }
            else
            {
                this.Show();
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            this.Hide();

            bool? result = loginWindow.ShowDialog();

            if (result == true)
            {
                ProfileWindow profileWindow = new ProfileWindow();
                profileWindow.Show();

                this.Close();
            }
            else
            {
                this.Show();
            }
        }
    }
}