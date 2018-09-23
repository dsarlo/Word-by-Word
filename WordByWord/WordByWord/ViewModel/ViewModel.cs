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
using GalaSoft.MvvmLight.Messaging;
using WordByWord.Models;

namespace WordByWord.ViewModel
{
    public class ViewModel : ObservableObject
    {
        private string _editorText = string.Empty;
        private OcrDocument _selectedDocument;
        private readonly object _lock = new object();
        private ObservableCollection<OcrDocument> _library = new ObservableCollection<OcrDocument>();// filePaths, ocrtext
        private ContextMenu _addDocumentContext;
        private string _currentWord = string.Empty;
        private string _userInputTitle = string.Empty;
        private string _userInputBody = string.Empty;
        private bool _isBusy;

        public ViewModel()
        {
            BindingOperations.EnableCollectionSynchronization(_library, _lock);

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
            Messenger.Default.Send(new NotificationMessage("ShowTextInputWindow"));
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

        public void CreateDocFromUserInput()
        {
            OcrDocument newDoc = new OcrDocument()
            {
                FilePath = "None",
                FileName = UserInputTitle,
                OcrText = UserInputBody
            };

            Library.Add(newDoc);

            UserInputTitle = string.Empty;
            UserInputBody = string.Empty;

            Messenger.Default.Send(new NotificationMessage("CloseInputTextWindow"));
        }

        private void ReadSelectedDocument()
        {
            IsBusy = true;
            ReadSelectedDocumentAsync().GetAwaiter();
        }

        public async Task ReadSelectedDocumentAsync()
        {
            if (SelectedDocument != null)
            {
                string[] words = SelectedDocument.OcrText.Replace("\r\n", " ").Split(' ');

                foreach (string word in words)
                {
                    if (!string.IsNullOrWhiteSpace(word))
                    {
                        CurrentWord = word;
                        await Task.Delay(200);
                    }
                }
                IsBusy = false;
            }
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
            Messenger.Default.Send(new NotificationMessage("CloseEditorWindow"));
        }

        private void OpenReaderWindow()
        {
            Messenger.Default.Send(new NotificationMessage("ShowReaderWindow"));
        }

        private void OpenEditorWindow()
        {
            Messenger.Default.Send(new NotificationMessage("ShowEditorWindow"));
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
