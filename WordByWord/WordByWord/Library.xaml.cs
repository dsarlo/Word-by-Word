using System;
using System.Windows.Controls;
using GalaSoft.MvvmLight.Messaging;
using MahApps.Metro.Controls;

namespace WordByWord
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Library : MetroWindow
    {
        private readonly ViewModel.ViewModel _viewModel;
        Reader _readerTextWindow;
        Editor _editorTextWindow;
        InputText _inputTextWindow;
        
        public Library()
        {
            InitializeComponent();

            _viewModel = new ViewModel.ViewModel();
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
                    if (_readerTextWindow != null)
                    {
                        _readerTextWindow.Close();
                    }
                    break;
                case "CloseEditorWindow":
                    if (_editorTextWindow != null)
                    {
                        _editorTextWindow.Close();
                    }
                    break;
                case "CloseInputTextWindow":
                    if (_inputTextWindow != null)
                    {
                        _inputTextWindow.Close();
                    }
                    break;
            }
        }
    }
}
