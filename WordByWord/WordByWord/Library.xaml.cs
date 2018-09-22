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
        public Library()
        {
            InitializeComponent();

            _viewModel = new ViewModel.ViewModel();
            DataContext = _viewModel;
        }

        private void NotificationMessageReceived(NotificationMessage message)
        {
            switch (message.Notification)
            {
                case "ShowReaderWindow":
                    new Reader(_viewModel).Show();
                    break;
                case "ShowEditorWindow":
                    new Editor(_viewModel, _viewModel.SelectedDocument.OcrText).Show();
                    break;
                case "ShowTextInputWindow":
                    // TODO: Create the window for text input
                    throw new NotImplementedException();
                    break;
            }
        }
    }
}
