namespace BrainBurst.Presentation.Views
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using BrainBurst.BLL.Interfaces;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Логіка взаємодії для відображення таблиці лідерів (рейтингу) користувачів.
    /// </summary>
    public partial class AwardsView : UserControl
    {
        private const int TopN = 10;
        private readonly IUserService _userService;
        private readonly ILogger<AwardsView> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AwardsView"/> class.
        /// </summary>
        /// <param name="userService">Сервіс для отримання даних користувачів та таблиці лідерів.</param>
        /// <param name="logger">Логер для запису подій.</param>
        public AwardsView(IUserService userService, ILogger<AwardsView> logger)
        {
            this.InitializeComponent();
            this._userService = userService;
            this._logger = logger;

            this._logger.LogDebug("AwardsView: View ініціалізовано.");
            this.Loaded += this.AwardsView_Loaded;
        }

        private async void AwardsView_Loaded(object sender, RoutedEventArgs e)
        {
            this._logger.LogInformation("AwardsView_Loaded: Запуск завантаження таблиці лідерів.");
            try
            {
                await this.LoadLeaderboardAsync();
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "AwardsView_Loaded: Критична помилка при завантаженні таблиці лідерів.");
                MessageBox.Show($"Помилка завантаження таблиці лідерів: {ex.Message}", "Помилка");
            }
        }

        private async Task LoadLeaderboardAsync()
        {
            this._logger.LogDebug("LoadLeaderboardAsync: Виклик UserService для отримання топ-{TopN} користувачів.", TopN);

            try
            {
                var leaderboard = await this._userService.GetLeaderboardAsync(TopN, CancellationToken.None);
                this.LeaderboardItemsControl.ItemsSource = leaderboard;

                this._logger.LogInformation("LoadLeaderboardAsync: Успішно завантажено {Count} записів таблиці лідерів.", leaderboard.Count);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "LoadLeaderboardAsync: Критична помилка під час отримання даних рейтингу.");
                throw;
            }
        }
    }
}