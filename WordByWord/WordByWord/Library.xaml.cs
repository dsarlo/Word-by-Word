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
        public Library()
        {
            InitializeComponent();

            ViewModel.ViewModel viewModel = new ViewModel.ViewModel();
            DataContext = viewModel;
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
