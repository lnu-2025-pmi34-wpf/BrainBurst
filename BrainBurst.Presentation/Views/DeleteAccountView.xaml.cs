namespace BrainBurst.Presentation.Views
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Navigation;
    using BrainBurst.BLL.Interfaces; // Додаємо для IUserService та IAuthContext
    using Microsoft.Extensions.DependencyInjection;

    public partial class DeleteAccountView : UserControl
    {
        private readonly IUserService _userService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IAuthContext _authContext;

        public DeleteAccountView(IUserService userService, IServiceProvider serviceProvider, IAuthContext authContext)
        {
            this.InitializeComponent();
            this._userService = userService;
            this._serviceProvider = serviceProvider;
            this._authContext = authContext;
        }

        private async void YesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // ВИКОРИСТАННЯ: Видалення акаунту поточного користувача
                await this._userService.DeleteAccountAsync(this._authContext.CurrentUserId, CancellationToken.None);

                // Після успішного видалення очищуємо контекст
                this._authContext.ClearContext(); // <--- ВИПРАВЛЕНО: Викликаємо ClearContext()

                // Перенаправляємо на головне вікно
                MainWindow mainWindow = this._serviceProvider.GetRequiredService<MainWindow>();
                Window currentWindow = Window.GetWindow(this);

                mainWindow.Show();

                if (currentWindow != null)
                {
                    currentWindow.Close(); // Закриваємо ProfileWindow
                }
            }
            catch (Exception ex)
            {
                this.GoBack();
                MessageBox.Show($"Помилка при видаленні акаунту: {ex.Message}");
            }
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            this.GoBack();
        }

        private void GoBack()
        {
            if (NavigationService.GetNavigationService(this) != null &&
                NavigationService.GetNavigationService(this).CanGoBack)
            {
                NavigationService.GetNavigationService(this).GoBack();
            }
        }
    }
}