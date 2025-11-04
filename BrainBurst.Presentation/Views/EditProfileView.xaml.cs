using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Media;
using BrainBurst.BLL.Interfaces; // Додаємо для IUserService та IAuthContext
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

namespace BrainBurst.Presentation.Views
{
    public partial class EditProfileView : UserControl
    {
        // УСУНЕНО: private const int CurrentUserId = 1; 

        private readonly IUserService _userService;
        private readonly IAuthContext _authContext; // <--- ДОДАНО ПОЛЕ

        // ОНОВЛЕНО: Конструктор приймає IAuthContext
        public EditProfileView(IUserService userService, IAuthContext authContext) 
        {
            InitializeComponent();
            _userService = userService;
            _authContext = authContext; // <--- ІНІЦІАЛІЗОВАНО
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

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ChangeUsernamePanel.Visibility == Visibility.Visible)
            {
                string newName = NewUsernameTextBox.Text;

                UsernameStatusText.Text = "";
                UsernameStatusText.Foreground = Brushes.Red;

                try
                {
                    // ВИКОРИСТАННЯ: CurrentUserId замінено на _authContext.CurrentUserId
                    await _userService.UpdateProfileAsync(_authContext.CurrentUserId, newName, CancellationToken.None);

                    UsernameStatusText.Text = "Ім'я успішно змінено!";
                    UsernameStatusText.Foreground = Brushes.Green;
                    
                    // Оновлюємо ім'я у контексті після успішного збереження
                    if (_authContext.CurrentUser != null)
                    {
                         _authContext.CurrentUser.FullName = newName;
                    }
                }
                catch (ArgumentException ex)
                {
                    // Помилка валідації (наприклад, порожнє ім'я)
                    UsernameStatusText.Text = ex.Message;
                }
                catch (KeyNotFoundException)
                {
                    // Помилка, якщо користувача не знайдено 
                    UsernameStatusText.Text = "Помилка. Користувача не знайдено.";
                }
                catch (Exception)
                {
                    // Інші помилки (наприклад, проблеми з БД)
                    UsernameStatusText.Text = "Помилка. Не вдалося зберегти зміни.";
                }
            }
        }
    }
}