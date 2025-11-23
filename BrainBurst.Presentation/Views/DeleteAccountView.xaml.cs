namespace BrainBurst.Presentation.Views
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Navigation;
    using BrainBurst.BLL.Interfaces;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Логіка взаємодії для View підтвердження видалення облікового запису.
    /// </summary>
    public partial class DeleteAccountView : UserControl
    {
        private readonly IUserService _userService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IAuthContext _authContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteAccountView"/> class.
        /// </summary>
        /// <param name="userService">Сервіс для видалення облікового запису.</param>
        /// <param name="serviceProvider">Постачальник служб DI (для відкриття MainWindow).</param>
        /// <param name="authContext">Контекст автентифікації для отримання ID поточного користувача та очищення сесії.</param>
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
                await this._userService.DeleteAccountAsync(this._authContext.CurrentUserId, CancellationToken.None);

                this._authContext.ClearContext();

                MainWindow mainWindow = this._serviceProvider.GetRequiredService<MainWindow>();
                Window currentWindow = Window.GetWindow(this);

                mainWindow.Show();

                if (currentWindow != null)
                {
                    currentWindow.Close();
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