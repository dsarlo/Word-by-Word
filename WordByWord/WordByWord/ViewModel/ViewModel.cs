using System;
using System.Collections.Generic;
using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using GalaSoft.MvvmLight.Command;
using IronOcr;
using Microsoft.Win32;
using WordByWord.Models;
using MahApps.Metro.Controls.Dialogs;
using WordByWord.Helpers;
using System.Text;

namespace WordByWord.ViewModel
{
    public class ViewModel : ObservableObject
    {
        private string _editorText = string.Empty;
        private OcrDocument _selectedDocument;
        private readonly object _libraryLock = new object();
        private ObservableCollection<OcrDocument> _library = new ObservableCollection<OcrDocument>();// filePaths, ocrtext
        private ContextMenu _addDocumentContext;
        private string _currentWord = string.Empty;
        private string _userInputTitle = string.Empty;
        private string _userInputBody = string.Empty;
        private bool _isBusy;
        private int _numberOfGroups = 1;
        private int _readerFontSize = 50;
        private int _readerDelay = 200;

        private readonly IDialogCoordinator _dialogService;
        private readonly IWindowService _windowService;


        public ViewModel(IDialogCoordinator dialogService, IWindowService windowService)
        {
            _dialogService = dialogService;
            _windowService = windowService;

            BindingOperations.EnableCollectionSynchronization(_library, _libraryLock);

            CreateAddDocumentContextMenu();

            AddDocumentCommand = new RelayCommand(AddDocumentContext);
            OpenEditorCommand = new RelayCommand(OpenEditorWindow);
            ConfirmEditCommand = new RelayCommand(ConfirmEdit);
            ReadSelectedDocumentCommand = new RelayCommand(ReadSelectedDocument, () => !IsBusy);
            CreateDocFromUserInputCommand = new RelayCommand(CreateDocFromUserInput);
        }

        #region Properties

        public RelayCommand CreateDocFromUserInputCommand { get; }

        public RelayCommand ReadSelectedDocumentCommand { get; }

        public RelayCommand ConfirmEditCommand { get; }

        public RelayCommand OpenEditorCommand { get; }

        public RelayCommand AddDocumentCommand { get; }

        public int ReaderDelay
        {
            get => _readerDelay;
            set
            {
                Set(() => ReaderDelay, ref _readerDelay, value);
            }
        }

        public int ReaderFontSize
        {
            get => _readerFontSize;
            set
            {
                Set(() => ReaderFontSize, ref _readerFontSize, value);
            }
        }

        public int NumberOfGroups
        {
            get => _numberOfGroups;
            set
            {
                Set(() => NumberOfGroups, ref _numberOfGroups, value);
                switch(value)
                {
                    case 1:
                        ReaderFontSize = 50;
                        ReaderDelay = 200;
                        break;
                    case 2:
                        ReaderFontSize = 45;
                        ReaderDelay = 300;
                        break;
                    case 3:
                        ReaderFontSize = 40;
                        ReaderDelay = 400;
                        break;
                    case 4:
                        ReaderFontSize = 35;
                        ReaderDelay = 500;
                        break;
                    case 5:
                        ReaderFontSize = 30;
                        ReaderDelay = 800;
                        break;
                }
            }
        }

        public string UserInputTitle
        {
            get => _userInputTitle;
            set
            {
                Set(() => UserInputTitle, ref _userInputTitle, value);
            }
        }

        public string UserInputBody
        {
            get => _userInputBody;
            set
            {
                Set(() => UserInputBody, ref _userInputBody, value);
            }
        }

        public string CurrentWord
        {
            get => _currentWord;
            set { Set(() => CurrentWord, ref _currentWord, value); }
        }

        public ObservableCollection<OcrDocument> Library
        {
            get => _library;
            set { Set(() => Library, ref _library, value); }
        }

        public OcrDocument SelectedDocument
        {
            get => _selectedDocument;
            set
            {
                if (!value.IsBusy)
                {
                    if (Set(() => SelectedDocument, ref _selectedDocument, value))
                    {
                        OpenReaderWindow();
                    }
                }
            }
        }

        public string EditorText
        {
            get => _editorText;
            set
            {
                Set(() => EditorText, ref _editorText, value);
            }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                Set(() => IsBusy, ref _isBusy, value);
            }
        }

        #endregion

        #region Events

        private void InputText_Click(object sender, RoutedEventArgs e)
        {
            _windowService.ShowWindow("InputText", this);
        }

        private async void UploadImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if (openFileDialog.ShowDialog() == true)
            {
                List<string> filePaths = new List<string>();
                foreach (string filePath in openFileDialog.FileNames)
                {
                    if (Library.All(doc => doc.FilePath != filePath))
                    {
                        OcrDocument ocrDoc = new OcrDocument(filePath);
                        Library.Add(ocrDoc);
                        filePaths.Add(filePath);
                    }
                }

                if (filePaths.Count > 0)
                {
                    await RunOcrOnFiles(filePaths);
                }
            }
        }

        #endregion

        #region Methods

        private void CreateDocFromUserInput()
        {
            if (!string.IsNullOrEmpty(UserInputTitle))
            {
                if (Library.All(doc => doc.FileName != UserInputTitle))
                {
                    OcrDocument newDoc =
                        new OcrDocument(UserInputTitle) //All OcrDocuments must be created with a filePath!
                        {
                            OcrText = UserInputBody
                        };

                    Library.Add(newDoc);

                    UserInputTitle = string.Empty;
                    UserInputBody = string.Empty;

                    _windowService.CloseWindow("InputText", this);
                }
                else
                {
                    _dialogService.ShowModalMessageExternal(this, "Title taken",
                        "Your library already contains another document with that title, please choose another.");
                }
            }
            else
            {
                _dialogService.ShowModalMessageExternal(this, "Title missing",
                    "Please give your new text document a title.");
            }
        }

        private void ReadSelectedDocument()
        {
            IsBusy = true;
            ReadSelectedDocumentAsync().GetAwaiter();
        }

        private async Task ReadSelectedDocumentAsync()
        {
            if (SelectedDocument != null)
            {
                List<string> words = await SplitIntoGroups(SelectedDocument.OcrText, NumberOfGroups);

                foreach (string word in words)
                {
                    if (!string.IsNullOrWhiteSpace(word))
                    {
                        CurrentWord = word;
                        await Task.Delay(ReaderDelay);
                    }
                }
                IsBusy = false;
            }
        }

        private async Task<List<string>> SplitIntoGroups(string sentence, int factor)
        {
            List<string> groups = new List<string>();

            await Task.Run(() =>
            {
                string[] words = sentence.Replace("\r\n", " ").Split();

                for (int i = 0; i < words.Length; i += factor)
                {
                    string group = string.Join(" ", words.Skip(i).Take(factor));
                    groups.Add(group);
                }
            });
            
            return groups;
        }

        private async Task RunOcrOnFiles(List<string> filePaths)
        {
            await Task.Run(() =>
            {
                foreach (string filePath in filePaths)
                {
                    string ocrResult = GetTextFromImage(filePath);
                    Library.Single(doc => doc.FilePath == filePath).OcrText = ocrResult;
                }
            });
        }

        private void ConfirmEdit()
        {
            Library.Single(doc => doc.FilePath == SelectedDocument.FilePath).OcrText = EditorText;

            _windowService.CloseWindow("Editor", this);
        }

        private void OpenReaderWindow()
        {
            _windowService.ShowWindow("Reader", this);
        }

        private void OpenEditorWindow()
        {
            _windowService.ShowWindow("Editor", this);
        }

        private void AddDocumentContext()
        {
            _addDocumentContext.IsOpen = true;
        }

        private void CreateAddDocumentContextMenu()
        {
            _addDocumentContext = new ContextMenu();

            MenuItem inputText = new MenuItem
            {
                Header = "Input text",
            };
            MenuItem uploadImage = new MenuItem
            {
                Header = "Upload image...",
            };

            inputText.Click += InputText_Click;
            uploadImage.Click += UploadImage_Click;

            _addDocumentContext.Items.Add(inputText);
            _addDocumentContext.Items.Add(uploadImage);
        }

        public string GetTextFromImage(string filePath)
        {
            AdvancedOcr ocr = new AdvancedOcr
            {
                CleanBackgroundNoise = true,
                EnhanceContrast = true,
                EnhanceResolution = true,
                Language = IronOcr.Languages.English.OcrLanguagePack,
                Strategy = AdvancedOcr.OcrStrategy.Advanced,
                ColorSpace = AdvancedOcr.OcrColorSpace.GrayScale,
                DetectWhiteTextOnDarkBackgrounds = true,
                InputImageType = AdvancedOcr.InputTypes.AutoDetect,
                RotateAndStraighten = true,
                ColorDepth = 4
            };

            return ocr.Read(filePath).Text;
        }

        #endregion
    }
}
