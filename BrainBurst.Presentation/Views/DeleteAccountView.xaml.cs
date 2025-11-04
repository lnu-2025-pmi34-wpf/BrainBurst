using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using BrainBurst.BLL.Interfaces; // Додаємо для IUserService та IAuthContext
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System.Threading;

namespace BrainBurst.Presentation.Views
{
    public partial class DeleteAccountView : UserControl
    {
        private readonly IUserService _userService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IAuthContext _authContext; 

        public DeleteAccountView(IUserService userService, IServiceProvider serviceProvider, IAuthContext authContext)
        {
            InitializeComponent();
            _userService = userService;
            _serviceProvider = serviceProvider;
            _authContext = authContext; 
        }

        private async void YesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // ВИКОРИСТАННЯ: Видалення акаунту поточного користувача
                await _userService.DeleteAccountAsync(_authContext.CurrentUserId, CancellationToken.None);
                
                // Після успішного видалення очищуємо контекст
                _authContext.ClearContext(); // <--- ВИПРАВЛЕНО: Викликаємо ClearContext()

                // Перенаправляємо на головне вікно 
                MainWindow mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                Window currentWindow = Window.GetWindow(this);
                
                mainWindow.Show();

                if (currentWindow != null)
                {
                    currentWindow.Close(); // Закриваємо ProfileWindow
                }
            }
            catch (Exception ex)
            {
                GoBack();
                MessageBox.Show($"Помилка при видаленні акаунту: {ex.Message}");
            }
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            GoBack();
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