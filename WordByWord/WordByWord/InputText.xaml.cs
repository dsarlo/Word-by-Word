using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WordByWord
{
    /// <summary>
    /// Interaction logic for InputText.xaml
    /// </summary>
    public partial class InputText : MetroWindow
    {
        ViewModel.ViewModel _viewModel;

        public InputText(ViewModel.ViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            this.DataContext = _viewModel;
        }
    }
}
