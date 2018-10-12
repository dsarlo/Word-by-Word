
namespace WordByWord.Helpers
{
    public interface IWindowService
    {
        void ShowWindow(string window, ViewModel.ViewModel viewModel);
        void CloseWindow(string window, ViewModel.ViewModel viewModel);
    }

    public class WindowService : IWindowService
    {
        private Editor _editor;
        private Library _library;
        private Reader _reader;
        private InputText _inputText;

        public void ShowWindow(string window, ViewModel.ViewModel viewModel)
        {
            switch (window)
            {
                case "Editor":
                    _editor = new Editor(viewModel);
                    _editor.ShowDialog();
                    break;
                case "Library":
                    _library = new Library();
                    _library.ShowDialog();
                    break;
                case "Reader":
                    _reader = new Reader(viewModel);
                    _reader.ShowDialog();
                    break;
                case "InputText":
                    _inputText = new InputText(viewModel);
                    _inputText.ShowDialog();
                    break;
            }
        }

        public void CloseWindow(string window, ViewModel.ViewModel viewModel)
        {
            switch (window)
            {
                case "Editor":
                    _editor?.Close();
                    _editor = null;
                    break;
                case "Library":
                    _library?.Close();
                    _library = null;
                    break;
                case "Reader":
                    _reader?.Close();
                    _reader = null;
                    break;
                case "InputText":
                    _inputText?.Close();
                    _inputText = null;
                    break;
            }
        }
    }
}
