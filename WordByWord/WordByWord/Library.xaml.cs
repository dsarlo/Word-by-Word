using System.Windows;
using System.Windows.Input;
using CommonServiceLocator;
using MahApps.Metro.Controls;
using WordByWord.Models;

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
            if (_viewModel.SelectedDocument != null && !_viewModel.SelectedDocument.IsBusy)
            {
                _viewModel.OpenReaderWindow();
            }
        }

        private void ListViewItem_KeyDown(object sender, KeyEventArgs e)
        {
            if (_viewModel.SelectedDocument != null && e.Key == Key.Enter && _viewModel.SelectedDocument.IsEditingFileName)
            {
                _viewModel.SelectedDocument.IsEditingFileName = false;
                _viewModel.SaveLibrary();
            }
        }

        private void RenameTextBox_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_viewModel.SelectedDocument != null && _viewModel.SelectedDocument.IsEditingFileName)
            {
                _viewModel.SelectedDocument.IsEditingFileName = false;
                _viewModel.SaveLibrary();
            }
        }
    }
}
