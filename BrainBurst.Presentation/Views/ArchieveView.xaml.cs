using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace BrainBurst.Presentation.Views
{
    public partial class ArchiveView : UserControl
    {
        public ArchiveView()
        {
            InitializeComponent();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            GoBack();
        }

        private void ResultItem_Click(object sender, RoutedEventArgs e)
        {
        
            var mistakes = new List<TestMistake>
            {
                new TestMistake
                {
                    QuestionText = "Що таке граф?",
                    UserAnswer = "Не знаю",
                    CorrectAnswer = "Множина вершин і ребер"
                },
                new TestMistake
                {
                    QuestionText = "Що таке дерево?",
                    UserAnswer = "Рослина",
                    CorrectAnswer = "Ациклічний граф"
                }
            };
            int totalQuestions = 10;

            if (NavigationService.GetNavigationService(this) != null)
            {
                NavigationService.GetNavigationService(this).Navigate(new TestResultsView(mistakes, totalQuestions));
            }
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