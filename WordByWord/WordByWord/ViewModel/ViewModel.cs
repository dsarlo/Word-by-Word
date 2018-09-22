using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight.Command;

namespace WordByWord.ViewModel
{
    public class ViewModel : ObservableObject
    {
        private ObservableCollection<string> _library = new ObservableCollection<string>();

        public ViewModel()
        {
            AddDocumentCommand = new RelayCommand(AddDocument);
        }

        #region Properties

        public RelayCommand AddDocumentCommand { get; }

        public ObservableCollection<string> Library
        {
            get => _library;
            set { Set(() => Library, ref _library, value); }
        }

        #endregion

        #region Methods

        private void AddDocument()
        {
            _library.Add("HELLO");
        }

        #endregion
    }
}
