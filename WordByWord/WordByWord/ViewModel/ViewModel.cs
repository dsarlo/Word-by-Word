using System;
using System.Collections.Generic;
using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using GalaSoft.MvvmLight.Command;
using IronOcr;
using Microsoft.Win32;

namespace WordByWord.ViewModel
{
    public class ViewModel : ObservableObject
    {
        private ObservableCollection<string> _library = new ObservableCollection<string>();
        private readonly Dictionary<string, string> _recognizedText = new Dictionary<string, string>();
        private ContextMenu _addDocumentContext;

        public ViewModel()
        {
            CreateAddDocumentContextMenu();

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

        #region Events

        private void InputText_Click(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private async void UploadImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Image files (*.png;*.jpeg)|*.png;*.jpeg",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if (openFileDialog.ShowDialog() == true)
            {
                foreach (string filePath in openFileDialog.FileNames)
                {
                    Library.Add(Path.GetFileName(filePath));
                }

                await RunOcrOnFiles(openFileDialog.FileNames);
            }
        }

        #endregion 

        #region Methods

        private void AddDocument()
        {
            _addDocumentContext.IsOpen = true;
        }

        private void CreateAddDocumentContextMenu()
        {
            _addDocumentContext = new ContextMenu();
            
            MenuItem inputText = new MenuItem
            {
                Header = "Input text"
            };
            MenuItem uploadImage = new MenuItem
            {
                Header = "Upload image..."
            };

            inputText.Click += InputText_Click;
            uploadImage.Click += UploadImage_Click;

            _addDocumentContext.Items.Add(inputText);
            _addDocumentContext.Items.Add(uploadImage);
        }

        private async Task RunOcrOnFiles(string[] filePaths)
        {
            await Task.Run(() =>
            {
                foreach (string filePath in filePaths)
                {
                    if (!_recognizedText.ContainsKey(filePath))//TODO Notify of duplicate?
                    {
                        _recognizedText.Add(filePath, GetTextFromImage(filePath));
                    }
                }
            });
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

            OcrResult results = ocr.Read(filePath);
            return results.Text;
        }

        #endregion
    }
}
