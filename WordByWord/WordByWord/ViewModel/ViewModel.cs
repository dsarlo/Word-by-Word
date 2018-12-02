using System;
using System.Collections.Generic;
using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
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
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using MahApps.Metro;
using System.Diagnostics;

namespace WordByWord.ViewModel
{
    public class ViewModel : ObservableObject
    {
        private string _editorText = string.Empty;
        private OcrDocument _selectedDocument;
        private Stopwatch _stopWatch = new Stopwatch();
        private TimeSpan _elapsedTime = new TimeSpan();
        private readonly object _libraryLock = new object();
        private ObservableCollection<OcrDocument> _library = new ObservableCollection<OcrDocument>();// filePaths, ocrtext
        private ContextMenu _addDocumentContext;
        private string _currentWord = string.Empty;
        private string _userInputTitle = string.Empty;
        private string _userInputBody = string.Empty;
        private string _currentDefinition = string.Empty;
        private bool _isBusy;
        private bool _findingDefinition;
        private bool _sentenceReadingEnabled;
        private bool _isDarkMode;
        private int _numberOfGroups = 1;
        private int _wordsPerMinute;
        private int _readerFontSize = 50;
        private int _readerDelay = 500; // two words per second
        private int _numberOfSentences = 1;
        private int _currentWordIndex;
        private int _currentSentenceIndex;
        private bool _resumeReading;
        private bool _displayTime;
        private List<string> _wordsToRead;
        private List<string> _sentencesToRead;

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

            GoBackToLibrary = new RelayCommand(() =>
            {
                _windowService.CloseWindow("Reader");
                _windowService.ShowWindow("Library", this);
            });

            RemoveDocumentCommand = new RelayCommand(RemoveDocument, () => SelectedDocument != null);
            RenameDocumentCommand = new RelayCommand(RenameDocument, () => SelectedDocument != null && !SelectedDocument.IsBusy);
            AddDocumentCommand = new RelayCommand(AddDocumentContext);
            OpenEditorCommand = new RelayCommand(OpenEditorWindow, () => SelectedDocument != null);
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
            PauseReadingCommand = new RelayCommand(() =>
            {
                _cSource.Cancel();
                if (CheckIfAtEnd())
                {
                    ElapsedTime = _stopWatch.Elapsed;
                    DisplayTime = true;
                }
                _stopWatch.Stop();
            });
            
            ResetCommand = new RelayCommand(Reset);
            StepBackwardCommand = new RelayCommand(StepBackward);
            StepForwardCommand = new RelayCommand(StepForward);
            SwapThemeCommand = new RelayCommand(SwapTheme);
            LoadLibrary();
        }

        #region Properties

        public RelayCommand GoBackToLibrary { get; }

        public RelayCommand RenameDocumentCommand { get; }

        public RelayCommand RemoveDocumentCommand { get; }

        public RelayCommand ResetCommand { get; }

        public RelayCommand CreateDocFromUserInputCommand { get; }

        public RelayCommand ReadSelectedDocumentCommand { get; }

        public RelayCommand ConfirmEditCommand { get; }

        public RelayCommand OpenEditorCommand { get; }

        public RelayCommand AddDocumentCommand { get; }

        public RelayCommand PauseReadingCommand { get; }

        public RelayCommand StepBackwardCommand { get; }

        public RelayCommand StepForwardCommand { get; }

        public RelayCommand SwapThemeCommand { get; }

        public TimeSpan ElapsedTime
        {
            get => _elapsedTime;
            set
            {
                Set(() => ElapsedTime, ref _elapsedTime, value);
            }
        }
               
        public bool DisplayTime
        {
            get => _displayTime;
            set
            {
                Set(() => DisplayTime, ref _displayTime, value);
            }
        }

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

        public string CurrentDefinition
        {
            get => _currentDefinition;
            set => Set(() => CurrentDefinition, ref _currentDefinition, value);
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
                RemoveDocumentCommand.RaiseCanExecuteChanged();
                OpenEditorCommand.RaiseCanExecuteChanged();
                RenameDocumentCommand.RaiseCanExecuteChanged();
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

        public bool FindingDefinition
        {
            get => _findingDefinition;
            set => Set(() => FindingDefinition, ref _findingDefinition, value);
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
            }
        }

        public bool IsDarkMode
        {
            get => _isDarkMode;
            set
            {
                Set(() => IsDarkMode, ref _isDarkMode, value);
                Properties.Settings.Default.DarkMode = value;
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
                        System.Drawing.Image originalImage = System.Drawing.Image.FromFile(filePath);
                        Bitmap imageToBitmap = ResizeImage(originalImage, 50, 50);

                        string thumbnailPath = $"{SerializedDataFolderPath}{Path.GetFileName(filePath)}";
                        imageToBitmap.Save(thumbnailPath);

                        OcrDocument ocrDoc = new OcrDocument(filePath) { Thumbnail = new BitmapImage(new Uri(thumbnailPath)), ThumbnailPath = thumbnailPath };
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

        public void StartStopWatch()
        {
            if (CheckIfAtEnd() || CheckIfAtBeginning())
            {
                _stopWatch.Restart();
                DisplayTime = false;
            }
            else
            {
                _stopWatch.Start();
            }
        }

        public void CheckIfDoneReading()
        {
            if (CheckIfAtEnd())
            {
                ElapsedTime = _stopWatch.Elapsed;
                _stopWatch.Stop();
                DisplayTime = true;
            }
        } 

        public bool CheckIfAtEnd()
        {
            return (SentenceReadingEnabled && _currentSentenceIndex == _sentencesToRead?.Count - 1)
              || (!SentenceReadingEnabled && _currentWordIndex == _wordsToRead?.Count - 1);
        }

        public bool CheckIfAtBeginning()
        {
            return (SentenceReadingEnabled && _currentSentenceIndex == 0)
              || (!SentenceReadingEnabled && _currentWordIndex == 0);
        }

        public async Task DefineWordAsync()
        {
            CurrentDefinition = string.Empty;

            if (!IsBusy && NumberOfGroups == 1 && !SentenceReadingEnabled)
            {
                FindingDefinition = true;

                CurrentDefinition = await Dictionary.DefineAsync(CurrentWord);

                if (string.IsNullOrEmpty(CurrentDefinition))
                {
                    CurrentDefinition = "Could not find the definition :(";
                }

                FindingDefinition = false;
            }
        }

        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        public static Bitmap ResizeImage(System.Drawing.Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        public void LoadSettings()
        {
            WordsPerMinute = Properties.Settings.Default.WPM;
            IsDarkMode = Properties.Settings.Default.DarkMode;
            SetTheme();
        }

        private void Reset()
        {
            StopCurrentDocument();
        }

        private void StepBackward()
        {
            if (IsBusy)
            {
                _cSource.Cancel();
            }

            if (!SentenceReadingEnabled)
            {
                if (_currentWordIndex != 0)
                {
                    CurrentWord = _wordsToRead[--_currentWordIndex];
                    _resumeReading = true;
                }
            }
            else
            {
                if (_currentSentenceIndex != 0)
                {
                    CurrentWord = _sentencesToRead[--_currentSentenceIndex];
                    _resumeReading = true;
                }
            }
        }

        private void StepForward()
        {
            if (IsBusy)
            {
                _cSource.Cancel();
            }

            if (!SentenceReadingEnabled)
            {
                if (_currentWordIndex != _wordsToRead.Count - 1)
                {
                    CurrentWord = _wordsToRead[++_currentWordIndex];
                    _resumeReading = true;
                }
            }
            else
            {
                if (_currentSentenceIndex != _sentencesToRead.Count - 1)
                {
                    CurrentWord = _sentencesToRead[++_currentSentenceIndex];
                    _resumeReading = true;
                }
            }
        }

        private void SwapTheme()
        {
            IsDarkMode = !IsDarkMode;
            SetTheme();
        }

        private void SetTheme()
        {
            // Application.Current is null when tests are running.
            if (Application.Current != null) 
                ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent("Blue"),
                    IsDarkMode ? ThemeManager.GetAppTheme("BaseDark") : ThemeManager.GetAppTheme("BaseLight"));
        }

        internal void StopCurrentDocument()
        {
            _cSource.Cancel();
            _cSource.Dispose();
            _cSource = new CancellationTokenSource();
            _resumeReading = false;
            _currentWordIndex = 0;
            _stopWatch.Reset();
            CurrentWord = string.Empty;
            DisplayTime = false;
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
                string serializedLibraryFile = Directory.GetFiles(SerializedDataFolderPath).Single(filePath => filePath.EndsWith("library.json"));
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

                    _windowService.CloseWindow("InputText");

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
                _wordsToRead = await SplitIntoGroups();
                StartStopWatch();
                for (int wordIndex = _resumeReading ? _currentWordIndex : 0; wordIndex < _wordsToRead.Count; wordIndex++)
                {
                    _currentWordIndex = wordIndex;
                    string word = _wordsToRead[wordIndex];
                    if (!string.IsNullOrWhiteSpace(word))
                    {
                        CurrentWord = word;
                        await Task.Delay(ReaderDelay);
                    }

                    if (_resumeReading && wordIndex == _wordsToRead.Count-1)
                    {
                        _resumeReading = false;
                    }

                    if (ctoken.IsCancellationRequested && wordIndex != _wordsToRead.Count - 1)
                    {
                        if (_currentWord != string.Empty)
                        {
                            _resumeReading = true;
                        }

                        break;
                    }
                    CheckIfDoneReading();
                }
                IsBusy = false;
            }
        }

        private async Task ReadSelectedDocumentSentencesAsync(CancellationToken ctoken)
        {
            if (SelectedDocument != null)
            {
                // Split on regex to preserve chars we split on.
                _sentencesToRead = await SplitIntoSentences();
                StartStopWatch();
                for (int sentenceIndex = _resumeReading ? _currentSentenceIndex : 0; sentenceIndex < _sentencesToRead.Count; sentenceIndex++)
                {
                    _currentSentenceIndex = sentenceIndex;
                    string sentence = _sentencesToRead[sentenceIndex];
                    if (!string.IsNullOrWhiteSpace(sentence))
                    {
                        CurrentWord = sentence;
                        string[] words = sentence.Split(' ');
                        CalculateRelayDelay(words.Length);
                        await Task.Delay(ReaderDelay);
                    }

                    if (_resumeReading && sentenceIndex == _sentencesToRead.Count - 1)
                    {
                        _resumeReading = false;
                    }

                    if (ctoken.IsCancellationRequested && sentenceIndex != _sentencesToRead.Count - 1)
                    {
                        if (_currentWord != string.Empty)
                        {
                            _resumeReading = true;
                        }

                        break;
                    }
                    CheckIfDoneReading();
                }
                IsBusy = false;

            }
        }

        public void CalculateRelayDelay(int groups)
        {
            double wps = (double)_wordsPerMinute / 60;
            double ms = 1000 / wps;
            ReaderDelay = (int)ms * groups;
        }

        public async Task<List<string>> SplitIntoSentences()
        {
            List<string> groups = new List<string>();

            string text = SelectedDocument.OcrText;
            int numberOfSentences = NumberOfSentences;

            await Task.Run(() =>
            {
                string[] sentences = Regex.Split(text.Replace("\r\n", " ").Replace("...", "…"), "(?<!(?:Mr|Mr.|Dr|Ms|St|a|p|m|K)\\.)(?<=[\".\u2026!;\\?])\\s+", RegexOptions.IgnoreCase).ToArray();

                for (int i = 0; i < sentences.Length; i += numberOfSentences)
                {
                    string group = string.Join(" ", sentences.Skip(i).Take(numberOfSentences));
                    groups.Add(group);
                }
            });

            return groups;
        }

        public async Task<List<string>> SplitIntoGroups()
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
                    SaveLibrary();
                }
            });
        }

        private void ConfirmEdit()
        {
            Library.Single(doc => doc.FilePath == SelectedDocument.FilePath).OcrText = EditorText;

            _windowService.CloseWindow("Editor");

            SaveLibrary();
        }

        private void RemoveDocument()
        {
            OcrDocument docToRemove = Library.Single(doc => doc.FilePath == SelectedDocument.FilePath);
            Library.Remove(docToRemove);
            if (File.Exists(docToRemove.ThumbnailPath))
            {
                File.SetAttributes(docToRemove.ThumbnailPath, FileAttributes.Normal);
                File.Delete(docToRemove.ThumbnailPath);
            }
            SaveLibrary();
        }

        internal void OpenReaderWindow()
        {
            _windowService.CloseWindow("Library");
            _windowService.ShowWindow("Reader", this);
        }

        internal void OpenLibraryWindow()
        {
            _windowService.ShowWindow("Library", this);
        }

        private void OpenEditorWindow()
        {
            _windowService.ShowWindow("Editor", this);
        }

        private void AddDocumentContext()
        {
            _addDocumentContext.IsOpen = true;
        }

        private void RenameDocument()
        {
            if (!SelectedDocument.IsBusy)
            {
                SelectedDocument.IsEditingFileName = true;
            }
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
