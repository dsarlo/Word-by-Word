using System;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;

namespace WordByWord.Models
{
    public class OcrDocument : ObservableObject
    {
        private string _ocrText = string.Empty;
        private bool _isBusy = true;
        private string _fileName;
        private bool _isEditingFileName;
        private string _thumbnailPath;
        private BitmapSource _thumbnail;
        private int _currentWordIndex = 0;
        private int _currentSentenceIndex = 0;

        public OcrDocument(string filePath)
        {
            FilePath = filePath;
            _fileName = Path.GetFileName(filePath);
        }

        public string FilePath { get; }

        public string FileName
        {
            get => _fileName;
            set { Set(() => FileName, ref _fileName, value); }
        }

        public int CurrentWordIndex
        {
            get => _currentWordIndex;
            set { Set(() => CurrentWordIndex, ref _currentWordIndex, value); }
        }

        public int CurrentSentenceIndex
            get => _currentSentenceIndex;
            set { Set(() => CurrentSentenceIndex, ref _currentSentenceIndex, value); }
        } 

        public string OcrText
        {
            get => _ocrText;
            set
            {
                Set(() => OcrText, ref _ocrText, value);
                IsBusy = false;
            }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set { Set(() => IsBusy, ref _isBusy, value); }
        }

        public bool IsEditingFileName
        {
            get => _isEditingFileName;
            set
            {
                Set(() => IsEditingFileName, ref _isEditingFileName, value);
            }
        }

        public string ThumbnailPath
        {
            get => _thumbnailPath;
            set { Set(() => ThumbnailPath, ref _thumbnailPath, value); }
        }

        [JsonIgnore]
        public BitmapSource Thumbnail
        {
            get
            {
                if (_thumbnail == null)
                {
                    if (ThumbnailPath == null)
                    {
                        BitmapSource thumbnail = Imaging.CreateBitmapSourceFromHBitmap(
                            Properties.Resources.check.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(50, 50));

                        _thumbnail = thumbnail;
                    }
                    else
                    {
                        BitmapImage thumbnail = new BitmapImage();
                        thumbnail.BeginInit();
                        thumbnail.CacheOption = BitmapCacheOption.OnLoad;
                        thumbnail.UriSource = new Uri(ThumbnailPath);
                        thumbnail.EndInit();

                        _thumbnail = thumbnail;
                    }
                }
                return _thumbnail;
            }
            set { Set(() => Thumbnail, ref _thumbnail, value); }
        }
    }
}
