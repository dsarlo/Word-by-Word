using MahApps.Metro.Controls;

namespace WordByWord
{
    /// <summary>
    /// Interaction logic for Editor.xaml
    /// </summary>
    public partial class Editor : MetroWindow
    {
        public Editor(ViewModel.ViewModel viewModel)
        {
            InitializeComponent();

            DataContext = viewModel;
            viewModel.EditorText = viewModel.SelectedDocument.Text;
        }
    }
}
