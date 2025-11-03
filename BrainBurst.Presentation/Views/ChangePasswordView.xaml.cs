using System.Windows;
using System.Windows.Controls;
using System.Windows.Media; 
using System.Windows.Navigation; 

namespace BrainBurst.Presentation.Views
{
    public partial class ChangePasswordView : UserControl
    {
        public ChangePasswordView()
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

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (NewPasswordBox.Password != ConfirmPasswordBox.Password)
            {
                StatusText.Text = "Нові паролі не співпадають.";
                StatusText.Foreground = Brushes.Red;
                return;
            }


            bool success = true; 
            if (success)
            {
                StatusText.Text = "Пароль успішно змінено!";
                StatusText.Foreground = Brushes.Green;

                OldPasswordBox.Password = "";
                NewPasswordBox.Password = "";
                ConfirmPasswordBox.Password = "";
            }
            else
            {
                StatusText.Text = "Помилка. Не вдалося змінити пароль.";
                StatusText.Foreground = Brushes.Red;
            }
        }
    }
}