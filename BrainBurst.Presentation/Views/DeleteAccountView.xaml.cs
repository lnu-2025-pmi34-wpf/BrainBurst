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
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Логіка взаємодії для View підтвердження видалення облікового запису.
    /// </summary>
    public partial class DeleteAccountView : UserControl
    {
        private readonly IUserService _userService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IAuthContext _authContext;
        private readonly ILogger<DeleteAccountView> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteAccountView"/> class.
        /// </summary>
        /// <param name="userService">Сервіс для видалення облікового запису.</param>
        /// <param name="serviceProvider">Постачальник служб DI (для відкриття MainWindow).</param>
        /// <param name="authContext">Контекст автентифікації для отримання ID поточного користувача та очищення сесії.</param>
        /// <param name="logger">Логер для запису подій.</param>
        public DeleteAccountView(IUserService userService, IServiceProvider serviceProvider, IAuthContext authContext, ILogger<DeleteAccountView> logger)
        {
            this.InitializeComponent();
            this._userService = userService;
            this._serviceProvider = serviceProvider;
            this._authContext = authContext;
            this._logger = logger;

            this._logger.LogDebug("DeleteAccountView: View ініціалізовано.");
        }

        private async void YesButton_Click(object sender, RoutedEventArgs e)
        {
            int userId = this._authContext.CurrentUserId;
            this._logger.LogCritical("YesButton_Click: КОРИСТУВАЧ {UserId} ПІДТВЕРДИВ ВИДАЛЕННЯ ОБЛІКОВОГО ЗАПИСУ. Запущено незворотну операцію.", userId);

            try
            {
                await this._userService.DeleteAccountAsync(this._authContext.CurrentUserId, CancellationToken.None);

                this._logger.LogWarning("YesButton_Click: Обліковий запис {UserId} УСПІШНО ВИДАЛЕНО. Очищення контексту.", userId);

                this._authContext.ClearContext();

                MainWindow mainWindow = this._serviceProvider.GetRequiredService<MainWindow>();
                Window currentWindow = Window.GetWindow(this);

                mainWindow.Show();
                this._logger.LogInformation("YesButton_Click: Показано MainWindow. Завершення поточної оболонки.");

                if (currentWindow != null)
                {
                    currentWindow.Close();
                }
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "YesButton_Click: КРИТИЧНА ПОМИЛКА при видаленні облікового запису {UserId}.", userId);

                this.GoBack();
                MessageBox.Show($"Помилка при видаленні акаунту: {ex.Message}");
            }
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            this._logger.LogInformation("NoButton_Click: Користувач СКАСУВАВ видалення облікового запису.");
            this.GoBack();
        }

        private void GoBack()
        {
            if (NavigationService.GetNavigationService(this) != null &&
                NavigationService.GetNavigationService(this).CanGoBack)
            {
                this._logger.LogDebug("GoBack: Навігація назад.");
                NavigationService.GetNavigationService(this).GoBack();
            }
            else
            {
                this._logger.LogWarning("GoBack: Неможливо повернутися назад (NavigationService не готовий).");
            }
        }
    }
}