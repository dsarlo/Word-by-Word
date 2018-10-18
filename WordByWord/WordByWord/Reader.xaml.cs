using System.ComponentModel;
using MahApps.Metro.Controls;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace WordByWord
{
    /// <summary>
    /// Interaction logic for Reader.xaml
    /// </summary>
    public partial class Reader : MetroWindow
    {
        private readonly ViewModel.ViewModel _viewModel;

        public Reader(ViewModel.ViewModel viewModel)
        {
            InitializeComponent();

            _viewModel = viewModel;
            DataContext = _viewModel;
        }

        private void ShowHideMenu(string storyboard, Button btnHide, Button btnShow, StackPanel pnl)
        {
            Storyboard sb = Resources[storyboard] as Storyboard;
            sb.Begin(pnl);

            if (storyboard.Contains("Show"))
            {
                btnHide.Visibility = System.Windows.Visibility.Visible;
                btnShow.Visibility = System.Windows.Visibility.Hidden;
            }
            else if (storyboard.Contains("Hide"))
            {
                btnHide.Visibility = System.Windows.Visibility.Hidden;
                btnShow.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void BtnBottomMenuHide_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ShowHideMenu("SbHideBottomMenu", BtnBottomMenuHide, BtnBottomMenuShow, BottomMenu);
        }

        private void BtnBottomMenuShow_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ShowHideMenu("SbShowBottomMenu", BtnBottomMenuHide, BtnBottomMenuShow, BottomMenu);
        }
    }
}
