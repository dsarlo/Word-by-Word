using System;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;

namespace WordByWord.Models
{
    public class Document : ObservableObject
    {
        private string _ocrText = string.Empty;
        private bool _isBusy = true;
        private string _fileName;
        private bool _isEditingFileName;
        private string _thumbnailPath;
        private BitmapSource _thumbnail;

        public Document(string filePath)
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

        public string Text
        {
            get => _ocrText;
            set
            {
                Set(() => Text, ref _ocrText, value);
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
