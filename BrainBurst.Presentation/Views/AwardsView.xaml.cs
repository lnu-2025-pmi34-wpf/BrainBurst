using System.Windows.Controls;
using BrainBurst.BLL.Interfaces; 
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System;

namespace BrainBurst.Presentation.Views
{
    public partial class AwardsView : UserControl
    {
        private const int TopN = 10;
        private readonly IUserService _userService;

        // Оновлений конструктор для DI
        public AwardsView(IUserService userService) 
        {
            InitializeComponent();
            _userService = userService;
            
            // Завантажуємо дані після того, як елемент завантажиться у вікно
            this.Loaded += AwardsView_Loaded;
        }
        
        // Викликаємо асинхронну логіку після завантаження UI
        private void AwardsView_Loaded(object sender, RoutedEventArgs e)
        {
            LoadLeaderboardAsync();
        }

        private async Task LoadLeaderboardAsync()
        {
            try
            {
                // Отримуємо список лідерів через сервіс
                var leaderboard = await _userService.GetLeaderboardAsync(TopN, CancellationToken.None);
                
                // Встановлюємо ItemsSource для ItemsControl
                LeaderboardItemsControl.ItemsSource = leaderboard;
            }
            catch (Exception)
            {
                // У реальному додатку: показати повідомлення про помилку завантаження
                // MessageBox.Show("Помилка завантаження таблиці лідерів.");
            }
        }
    }
}