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

            Messenger.Default.Register<NotificationMessage>(this, NotificationMessageReceived);
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Hide();
            new Reader().Show();
        }

        private void NotificationMessageReceived(NotificationMessage message)
        {
            if (message.Notification == "ShowTextInputWindow")
            {
                // TODO: Create the window for text input
                throw new NotImplementedException();
            }
        }
    }
}
