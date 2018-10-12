
namespace WordByWord.Helpers
{
    public class WindowService : IWindowService
    {
        public void ShowWindow(string window, ViewModel.ViewModel viewModel)
        {
            switch (window)
            {
                case "Editor":
                    Editor editor = new Editor(viewModel);
                    editor.Show();
                    break;
                case "Library":
                    Library library = new Library();
                    library.Show();
                    break;
                case "Reader":
                    Reader reader = new Reader(viewModel);
                    reader.Show();
                    break;
                case "InputText":
                    InputText inputText = new InputText(viewModel);
                    inputText.Show();
                    break;
            }
        }
    }
}
