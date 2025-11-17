namespace BrainBurst.Presentation.Views
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using BrainBurst.BLL.Interfaces;

    public partial class AwardsView : UserControl
    {
        private const int TopN = 10;
        private readonly IUserService _userService;

        // Оновлений конструктор для DI
        public AwardsView(IUserService userService)
        {
            this.InitializeComponent();
            this._userService = userService;

            // Завантажуємо дані після того, як елемент завантажиться у вікно
            this.Loaded += this.AwardsView_Loaded;
        }

        // Викликаємо асинхронну логіку після завантаження UI
        private void AwardsView_Loaded(object sender, RoutedEventArgs e)
        {
            this.LoadLeaderboardAsync();
        }

        private async Task LoadLeaderboardAsync()
        {
            try
            {
                // Отримуємо список лідерів через сервіс
                var leaderboard = await this._userService.GetLeaderboardAsync(TopN, CancellationToken.None);

                // Встановлюємо ItemsSource для ItemsControl
                this.LeaderboardItemsControl.ItemsSource = leaderboard;
            }
            catch (Exception)
            {
                // У реальному додатку: показати повідомлення про помилку завантаження
                // MessageBox.Show("Помилка завантаження таблиці лідерів.");
            }
        }
    }
}