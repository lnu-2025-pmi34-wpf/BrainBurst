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
    using BrainBurst.BLL.Interfaces;

    /// <summary>
    /// Логіка взаємодії для View редагування профілю користувача.
    /// </summary>
    public partial class EditProfileView : UserControl
    {
        private readonly IUserService _userService;
        private readonly IAuthContext _authContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="EditProfileView"/> class.
        /// </summary>
        /// <param name="userService">Сервіс для оновлення даних користувача.</param>
        /// <param name="authContext">Контекст автентифікації для отримання ID поточного користувача.</param>
        public EditProfileView(IUserService userService, IAuthContext authContext)
        {
            this.InitializeComponent();
            this._userService = userService;
            this._authContext = authContext;
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
                    await this._userService.UpdateProfileAsync(this._authContext.CurrentUserId, newName, CancellationToken.None);

                    this.UsernameStatusText.Text = "Ім'я успішно змінено!";
                    this.UsernameStatusText.Foreground = Brushes.Green;

                    if (this._authContext.CurrentUser != null)
                    {
                        this._authContext.CurrentUser.FullName = newName;
                    }
                }
                catch (ArgumentException ex)
                {
                    this.UsernameStatusText.Text = ex.Message;
                }
                catch (KeyNotFoundException)
                {
                    this.UsernameStatusText.Text = "Помилка. Користувача не знайдено.";
                }
                catch (Exception)
                {
                    this.UsernameStatusText.Text = "Помилка. Не вдалося зберегти зміни.";
                }
            }
        }
    }
}