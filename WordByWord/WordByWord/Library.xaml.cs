using System.Windows.Controls;
using MahApps.Metro.Controls;

namespace WordByWord
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Library : MetroWindow
    {
        private readonly ViewModel.ViewModel _viewModel;

        public Library()
        {
            InitializeComponent();

            _viewModel = new ViewModel.ViewModel();
            DataContext = _viewModel;
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Hide();
            new Reader().Show();
        }
    }
}
