using MahApps.Metro.Controls;

namespace WordByWord
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Library : MetroWindow
    {
        public Library()
        {
            InitializeComponent();

            ViewModel.ViewModel viewModel = new ViewModel.ViewModel();
            DataContext = viewModel;
        }
    }
}
