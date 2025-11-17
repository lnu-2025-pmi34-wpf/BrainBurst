namespace BrainBurst.Presentation.Views
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Navigation;
    using BrainBurst.BLL.Interfaces; // Додаємо для IUserService та IAuthContext

    public partial class EditProfileView : UserControl
    {
        // УСУНЕНО: private const int CurrentUserId = 1;

        private readonly IUserService _userService;
        private readonly IAuthContext _authContext; // <--- ДОДАНО ПОЛЕ

        // ОНОВЛЕНО: Конструктор приймає IAuthContext
        public EditProfileView(IUserService userService, IAuthContext authContext)
        {
            this.InitializeComponent();
            this._userService = userService;
            this._authContext = authContext; // <--- ІНІЦІАЛІЗОВАНО
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
            this.ChangeUsernamePanel.Visibility = Visibility.Visible;
            this.UsernameStatusText.Text = string.Empty;
            this.NewUsernameTextBox.Focus();
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.ChangeUsernamePanel.Visibility == Visibility.Visible)
            {
                string newName = this.NewUsernameTextBox.Text;

                this.UsernameStatusText.Text = string.Empty;
                this.UsernameStatusText.Foreground = Brushes.Red;

                try
                {
                    // ВИКОРИСТАННЯ: CurrentUserId замінено на _authContext.CurrentUserId
                    await this._userService.UpdateProfileAsync(this._authContext.CurrentUserId, newName, CancellationToken.None);

                    this.UsernameStatusText.Text = "Ім'я успішно змінено!";
                    this.UsernameStatusText.Foreground = Brushes.Green;

                    // Оновлюємо ім'я у контексті після успішного збереження
                    if (this._authContext.CurrentUser != null)
                    {
                         this._authContext.CurrentUser.FullName = newName;
                    }
                }
                catch (ArgumentException ex)
                {
                    // Помилка валідації (наприклад, порожнє ім'я)
                    this.UsernameStatusText.Text = ex.Message;
                }
                catch (KeyNotFoundException)
                {
                    // Помилка, якщо користувача не знайдено
                    this.UsernameStatusText.Text = "Помилка. Користувача не знайдено.";
                }
                catch (Exception)
                {
                    // Інші помилки (наприклад, проблеми з БД)
                    this.UsernameStatusText.Text = "Помилка. Не вдалося зберегти зміни.";
                }
            }
        }
    }
}