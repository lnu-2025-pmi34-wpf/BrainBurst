namespace BrainBurst.Presentation.Views
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using BrainBurst.BLL.Interfaces;

    /// <summary>
    /// Логіка взаємодії для відображення таблиці лідерів (рейтингу) користувачів.
    /// </summary>
    public partial class AwardsView : UserControl
    {
        private const int TopN = 10;
        private readonly IUserService _userService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AwardsView"/> class.
        /// </summary>
        /// <param name="userService">Сервіс для отримання даних користувачів та таблиці лідерів.</param>
        public AwardsView(IUserService userService)
        {
            this.InitializeComponent();
            this._userService = userService;

            this.Loaded += this.AwardsView_Loaded;
        }

        private async void AwardsView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await this.LoadLeaderboardAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження таблиці лідерів: {ex.Message}", "Помилка");
            }
        }

        private async Task LoadLeaderboardAsync()
        {
            var leaderboard = await this._userService.GetLeaderboardAsync(TopN, CancellationToken.None);
            this.LeaderboardItemsControl.ItemsSource = leaderboard;
        }
    }
}