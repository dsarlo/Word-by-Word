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
            if (_viewModel.SelectedDocument != null)
            {
                _viewModel.OpenReaderWindow();
            }
        }

        private void Rename_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_viewModel.SelectedDocument != null)
            {
                OcrDocument listViewDoc = (OcrDocument) LibraryListView.Items[LibraryListView.SelectedIndex];
                if (!_viewModel.SelectedDocument.IsBusy && listViewDoc.FilePath == _viewModel.SelectedDocument.FilePath)
                {
                    _viewModel.SelectedDocument.IsEditingFileName = true;
                }
            }
        }

        private void ListViewItem_KeyDown(object sender, KeyEventArgs e)
        {
            if (_viewModel.SelectedDocument != null)
            {
                if (e.Key == Key.Enter && _viewModel.SelectedDocument.IsEditingFileName)
                {
                    _viewModel.SelectedDocument.IsEditingFileName = false;
                    _viewModel.SaveLibrary();
                }
            }
        }
    }
}
