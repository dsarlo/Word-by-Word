using MahApps.Metro.Controls;

namespace WordByWord
{
    /// <summary>
    /// Interaction logic for InputText.xaml
    /// </summary>
    public partial class InputText : MetroWindow
    {
        public InputText(ViewModel.ViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }
    }
}
