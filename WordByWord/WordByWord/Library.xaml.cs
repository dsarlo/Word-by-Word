using System.Windows.Input;
using CommonServiceLocator;
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

            _viewModel = ServiceLocator.Current.GetInstance<ViewModel.ViewModel>();
            DataContext = _viewModel;
        }

        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel.SelectedDocument != null)
            {
                _viewModel.OpenReaderWindow();
            }
        }

        private void Rename_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _viewModel.SelectedDocument.IsEditingFileName = true;
        }

        private void ListViewItem_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && _viewModel.SelectedDocument.IsEditingFileName)
            {
                _viewModel.SelectedDocument.IsEditingFileName = false;
            }
        }
    }
}
