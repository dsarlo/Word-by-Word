using System;
using System.Collections.Generic;
using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using System.IO;
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
using WordByWord.Services;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Text;
using System.Threading;

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
        private bool _sentenceReadingEnabled;
        private int _numberOfGroups;
        private int _wordsPerMinute;
        private int _readerFontSize;
        private int _readerDelay = 500; // two words per second
        private int _numberOfSentences;
        private int _pausedWordIndex;
        private bool _resumeReading;

        private CancellationTokenSource _cSource = new CancellationTokenSource();

        private static readonly string SerializedDataFolderPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\word-by-word\";
        private readonly string _serializedLibraryPath = $"{SerializedDataFolderPath}library.json";

        private readonly IDialogCoordinator _dialogService;
        private readonly IWindowService _windowService;

        public ViewModel(IDialogCoordinator dialogService, IWindowService windowService)
        {
            LoadSettings();

            _dialogService = dialogService;
            _windowService = windowService;

            BindingOperations.EnableCollectionSynchronization(_library, _libraryLock);

            CreateAddDocumentContextMenu();

            AddDocumentCommand = new RelayCommand(AddDocumentContext);
            OpenEditorCommand = new RelayCommand(OpenEditorWindow);
            ConfirmEditCommand = new RelayCommand(ConfirmEdit);
            ReadSelectedDocumentCommand = new RelayCommand(async () =>
            {
                if (!IsBusy)
                {
                    CancellationToken ctoken = _cSource.Token;

                    await ReadSelectedDocument(ctoken);
                    _cSource.Dispose();
                    _cSource = new CancellationTokenSource();
                }
            }, true);
            
            CreateDocFromUserInputCommand = new RelayCommand(CreateDocFromUserInput);

            PauseReadingCommand = new RelayCommand(() => _cSource.Cancel());

            ResetCommand = new RelayCommand(Reset);

            LoadLibrary();
        }

        #region Properties
        public RelayCommand ResetCommand { get; }

        public RelayCommand CreateDocFromUserInputCommand { get; }

        public RelayCommand ReadSelectedDocumentCommand { get; }

        public RelayCommand ConfirmEditCommand { get; }

        public RelayCommand OpenEditorCommand { get; }

        public RelayCommand AddDocumentCommand { get; }

        public RelayCommand PauseReadingCommand { get; }

        public int ReaderDelay
        {
            get => _readerDelay;
            set
            {
                Set(() => ReaderDelay, ref _readerDelay, value);
            }
        }

        public int NumberOfSentences
        {
            get => _numberOfSentences;
            set
            {
                Set(() => NumberOfSentences, ref _numberOfSentences, value);
                switch (value)
                {
                    case 1:
                        ReaderFontSize = 30;
                        break;
                    case 2:
                        ReaderFontSize = 20;
                        break;
                    case 3:
                        ReaderFontSize = 20;
                        break;
                }
                Properties.Settings.Default.NumberOfSentences = value;
                Properties.Settings.Default.Save();
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

                StopCurrentDocument();

                CalculateRelayDelay(_numberOfGroups);

                switch (value)
                {
                    case 1:
                        ReaderFontSize = 50;
                        break;
                    case 2:
                        ReaderFontSize = 45;
                        break;
                    case 3:
                        ReaderFontSize = 40;
                        break;
                    case 4:
                        ReaderFontSize = 35;
                        break;
                    case 5:
                        ReaderFontSize = 30;
                        break;
                }
                Properties.Settings.Default.WordsGrouping = value;
                Properties.Settings.Default.Save();
            }
        }


        public int WordsPerMinute
        {
            get => _wordsPerMinute;
            set
            {
                Set(() => WordsPerMinute, ref _wordsPerMinute, value);
                CalculateRelayDelay(_numberOfGroups);
                Properties.Settings.Default.WPM = value;
                Properties.Settings.Default.Save();
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
                if (value == null)
                {
                    Set(() => SelectedDocument, ref _selectedDocument, null);
                }
                else if (!value.IsBusy)
                {
                    Set(() => SelectedDocument, ref _selectedDocument, value);
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

        public bool SentenceReadingEnabled
        {
            get => _sentenceReadingEnabled;
            set
            {
                Set(() => SentenceReadingEnabled, ref _sentenceReadingEnabled, value);
                CurrentWord = string.Empty;
                if (value)
                {
                    ReaderFontSize = 30;
                }
                Properties.Settings.Default.SentencesEnabled = value;
                Properties.Settings.Default.Save();
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
                    SaveLibrary();
                }
            }
        }

        #endregion

        #region Methods
        public void LoadSettings()
        {
            NumberOfSentences = Properties.Settings.Default.NumberOfSentences;
            SentenceReadingEnabled = Properties.Settings.Default.SentencesEnabled;
            NumberOfGroups = Properties.Settings.Default.WordsGrouping;
            WordsPerMinute = Properties.Settings.Default.WPM;
        }

        public void Reset()
        {
            MetroDialogSettings settings = new MetroDialogSettings()
            {
                AffirmativeButtonText = "Reset",
                NegativeButtonText = "Cancel"
            };
            MessageDialogResult result = _dialogService.ShowModalMessageExternal(this, "Are you sure?", "This will reset your settings to defaults and return the reader to the beginning of the document. Are you sure you want to proceed?", MessageDialogStyle.AffirmativeAndNegative, settings);
            if (result == MessageDialogResult.Affirmative)
            {
                NumberOfSentences = 1;
                SentenceReadingEnabled = false;
                WordsPerMinute = 120;
                NumberOfGroups = 1;
                CurrentWord = string.Empty;

                StopCurrentDocument();
            }
        }

        internal void StopCurrentDocument()
        {
            _cSource.Cancel();
            _cSource.Dispose();
            _cSource = new CancellationTokenSource();
            _resumeReading = false;
            _pausedWordIndex = 0;
            CurrentWord = string.Empty;
        }

        public void SaveLibrary()
        {
            if (!Directory.Exists(SerializedDataFolderPath))
            {
                Directory.CreateDirectory(SerializedDataFolderPath);
            }
            string libraryAsJson = JsonConvert.SerializeObject(_library.Where(doc => !doc.IsBusy), Formatting.Indented);
            File.WriteAllText(_serializedLibraryPath, libraryAsJson, Encoding.UTF8);
        }

        public void LoadLibrary()
        {
            if (Directory.Exists(SerializedDataFolderPath))
            {
                string serializedLibraryFile = Directory.GetFiles(SerializedDataFolderPath).FirstOrDefault();
                if (!string.IsNullOrEmpty(serializedLibraryFile))
                {
                    Library = JsonConvert.DeserializeObject<ObservableCollection<OcrDocument>>(
                        File.ReadAllText(serializedLibraryFile));
                }
            }
        }

        private void CreateDocFromUserInput()
        {
            if (!string.IsNullOrEmpty(UserInputTitle))
            {
                if (Library.All(doc => doc.FileName != UserInputTitle))
                {
                    OcrDocument newDoc = new OcrDocument(UserInputTitle) //All OcrDocuments must be created with a filePath!
                    {
                        OcrText = UserInputBody
                    };

                    Library.Add(newDoc);

                    UserInputTitle = string.Empty;
                    UserInputBody = string.Empty;

                    _windowService.CloseWindow("InputText", this);

                    SaveLibrary();
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

        private async Task ReadSelectedDocument(CancellationToken ctoken)
        {
            IsBusy = true;
            if (!SentenceReadingEnabled)
            {
                await ReadSelectedDocumentWordsAsync(ctoken);
            }
            else
            {
                await ReadSelectedDocumentSentencesAsync(ctoken);
            }
        }

        private async Task ReadSelectedDocumentWordsAsync(CancellationToken ctoken)
        {
            if (SelectedDocument != null)
            {
                List<string> words = await SplitIntoGroups();

                for (int wordIndex = _resumeReading ? _pausedWordIndex : 0; wordIndex < words.Count; wordIndex++)
                {
                    string word = words[wordIndex];
                    if (!string.IsNullOrWhiteSpace(word))
                    {
                        CurrentWord = word;
                        await Task.Delay(ReaderDelay);
                    }

                    if (_resumeReading && wordIndex == words.Count-1)
                    {
                        _resumeReading = false;
                        _pausedWordIndex = 0;
                    }

                    if (ctoken.IsCancellationRequested && wordIndex != words.Count - 1)
                    {
                        if (_currentWord != string.Empty)
                        {
                            _pausedWordIndex = wordIndex;
                            _resumeReading = true;
                        }

                        break;
                    }
                }
                IsBusy = false;
            }
        }

        private async Task ReadSelectedDocumentSentencesAsync(CancellationToken ctoken)
        {
            if (SelectedDocument != null)
            {
                // Split on regex to preserve chars we split on.
                List<string> sentences = await SplitIntoSentences();

                for (int sentenceIndex = _resumeReading ? _pausedWordIndex : 0; sentenceIndex < sentences.Count; sentenceIndex++)
                {
                    string sentence = sentences[sentenceIndex];
                    if (!string.IsNullOrWhiteSpace(sentence))
                    {
                        CurrentWord = sentence;
                        string[] words = sentence.Split(' ');
                        CalculateRelayDelay(words.Length);
                        await Task.Delay(ReaderDelay);
                    }

                    if (_resumeReading && sentenceIndex == sentences.Count - 1)
                    {
                        _resumeReading = false;
                        _pausedWordIndex = 0;
                    }

                    if (ctoken.IsCancellationRequested && sentenceIndex != sentences.Count - 1)
                    {
                        if (_currentWord != string.Empty)
                        {
                            _pausedWordIndex = sentenceIndex;
                            _resumeReading = true;
                        }

                        break;
                    }
                }
                IsBusy = false;

            }
        }

        private void CalculateRelayDelay(int groups)
        {
            double wps = (double)_wordsPerMinute / 60;
            double ms = 1000 / wps;
            ReaderDelay = (int)ms * groups;
        }

        private async Task<List<string>> SplitIntoSentences()
        {
            List<string> groups = new List<string>();

            string text = SelectedDocument.OcrText;
            int numberOfSentences = NumberOfSentences;

            await Task.Run(() =>
            {
                string[] sentences = Regex.Split(text.Replace("\r\n", " ").Replace("...", "…"), "(?<!(?:Mr|Mr.|Dr|Ms|St|a|p|m|K)\\.)(?<=[\".!;\\?])\\s+", RegexOptions.IgnoreCase).ToArray();

                for (int i = 0; i < sentences.Length; i += numberOfSentences)
                {
                    string group = string.Join(" ", sentences.Skip(i).Take(numberOfSentences));
                    groups.Add(group);
                }
            });

            return groups;
        }

        private async Task<List<string>> SplitIntoGroups()
        {
            List<string> groups = new List<string>();

            string sentence = SelectedDocument.OcrText;
            int numberOfWords = NumberOfGroups;

            await Task.Run(() =>
            {
                string[] words = sentence.Replace("\r\n", " ").Split();

                for (int i = 0; i < words.Length; i += numberOfWords)
                {
                    string group = string.Join(" ", words.Skip(i).Take(numberOfWords));
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

            SaveLibrary();
        }

        internal void OpenReaderWindow()
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
