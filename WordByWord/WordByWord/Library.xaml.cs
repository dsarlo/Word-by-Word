using GalaSoft.MvvmLight.Messaging;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace WordByWord
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Library : MetroWindow
    {
        private readonly ViewModel.ViewModel _viewModel;
        private Reader _readerTextWindow;
        private Editor _editorTextWindow;
        private InputText _inputTextWindow;
        
        public Library()
        {
            InitializeComponent();

            _viewModel = new ViewModel.ViewModel(DialogCoordinator.Instance);
            DataContext = _viewModel;

            Messenger.Default.Register<NotificationMessage>(this, NotificationMessageReceived);
        }

        private void NotificationMessageReceived(NotificationMessage message)
        {
            switch (message.Notification)
            {
                case "ShowReaderWindow":
                    _readerTextWindow = new Reader(_viewModel);
                    _readerTextWindow.Show();
                    break;
                case "ShowEditorWindow":
                    _editorTextWindow = new Editor(_viewModel, _viewModel.SelectedDocument.OcrText);
                    _editorTextWindow.Show();
                    break;
                case "ShowTextInputWindow":
                    _inputTextWindow = new InputText(_viewModel);
                    _inputTextWindow.Show();
                    break;
                case "CloseReaderWindow":
                    _readerTextWindow?.Close();
                    break;
                case "CloseEditorWindow":
                    _editorTextWindow?.Close();
                    break;
                case "CloseInputTextWindow":
                    _inputTextWindow?.Close();
                    break;
            }
        }
    }
}
