
using System;

namespace WordByWord.Services
{
    public enum Windows
    {
        Library = 0,
        Reader = 1,
        Editor = 2,
        InputText = 3,
        Info = 4
    }

    public interface IWindowService
    {
        void ShowWindow(Windows window, ViewModel.ViewModel viewModel);
        void CloseWindow(Windows window);

        bool IsWindowOpen(Windows window);

        event EventHandler<Windows> OnWindowOpened;
    }

    public class WindowService : IWindowService
    {
        private Editor _editor;
        private Library _library;
        private Reader _reader;
        private InputText _inputText;
        private Info _info;

        public event EventHandler<Windows> OnWindowOpened;

        public void ShowWindow(Windows window, ViewModel.ViewModel viewModel)
        {
            switch (window)
            {
                case Windows.Editor:
                    _editor = new Editor(viewModel);
                    OnWindowOpened(_editor, Windows.Editor);
                    _editor.ShowDialog();
                    break;
                case Windows.Library:
                    if (_library == null)
                    {
                        _library = new Library();
                    }
                    OnWindowOpened(_library, Windows.Library);
                    _library.Show();
                    break;
                case Windows.Reader:
                    _reader = new Reader(viewModel);
                    OnWindowOpened(_reader, Windows.Reader);
                    _reader.ShowDialog();
                    break;
                case Windows.InputText:
                    _inputText = new InputText(viewModel);
                    OnWindowOpened(_inputText, Windows.InputText);
                    _inputText.ShowDialog();
                    break;
                case Windows.Info:
                    _info = new Info();
                    OnWindowOpened(_info, Windows.Info);
                    _info.ShowDialog();
                    break;
            }
        }

        public void CloseWindow(Windows window)
        {
            switch (window)
            {
                case Windows.Editor:
                    _editor?.Close();
                    _editor = null;
                    break;
                case Windows.Library:
                    _library?.Hide();
                    break;
                case Windows.Reader:
                    _reader?.Close();
                    _reader = null;
                    break;
                case Windows.InputText:
                    _inputText?.Close();
                    _inputText = null;
                    break;
                case Windows.Info:
                    _info?.Close();
                    _info = null;
                    break;
            }
        }

        public bool IsWindowOpen(Windows window)
        {
            bool windowIsOpen = false;

            switch(window)
            {
                case Windows.Reader:
                    windowIsOpen = _reader != null;
                    break;
                case Windows.Editor:
                    windowIsOpen = _editor != null;
                    break;
                case Windows.Library:
                    windowIsOpen = _library != null;
                    break;
                case Windows.Info:
                    windowIsOpen = _info != null;
                    break;
                case Windows.InputText:
                    windowIsOpen = _inputText != null;
                    break;
            }
            return windowIsOpen;
        }
    }
}
