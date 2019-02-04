using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using WordByWord.Models;
using WordByWord.Services;

namespace WordByWord.Test
{
    [TestClass]
    public class LibraryTests
    {
        private static ViewModel.ViewModel _viewModel;

        [ClassInitialize]
        public static void TestSetup(TestContext testContext)
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<IWindowService, WindowService>();
            SimpleIoc.Default.Register<IDialogCoordinator, DialogCoordinator>();
            SimpleIoc.Default.Register<ViewModel.ViewModel>();

            _viewModel = ServiceLocator.Current.GetInstance<ViewModel.ViewModel>();
        }

        [TestMethod]
        public void ImageCharacterRecognitionTest()//Must be run in debug!
        {
            string ocrResult = _viewModel.GetTextFromImage(@"TestFiles\LockeEssay.PNG");
            Assert.IsTrue(ocrResult.Length > 0);
        }

        [TestMethod]
        public void FileImportTest()
        {
            //Reset for next test
            _viewModel.Library = new ObservableCollection<OcrDocument>();

            string[] tooManyTestFiles = new string[25];
            _viewModel.ImportFilesToLibrary(tooManyTestFiles);
            Assert.IsTrue(_viewModel.Library.Count == 0);

            //Reset for next test
            _viewModel.Library = new ObservableCollection<OcrDocument>();

            string[] goodTestFile = { @"TestFiles\LockeEssay.PNG" };
            _viewModel.ImportFilesToLibrary(goodTestFile);
            Assert.IsTrue(_viewModel.Library.Count == 1);

            //Reset for next test
            _viewModel.Library = new ObservableCollection<OcrDocument>();

            string[] badTestFile = { @"TestFiles\library.json" };
            _viewModel.ImportFilesToLibrary(badTestFile);
            Assert.IsTrue(_viewModel.Library.Count == 0);
        }

        [TestMethod]
        public void LibrarySaveAndLoadTest()
        {
            OcrDocument doc1 = new OcrDocument("g")
            {
                FileName = "Hello!",
                IsBusy = false,
                IsEditingFileName = false,
                OcrText = "g"
            };

            OcrDocument doc2 = new OcrDocument("C:\\Users\\dan\\Pictures\\GDB.PNG")
            {
                FileName = "GDB.PNG",
                OcrText = "00081 Oxbfffea68 --> 0x342\r\n\r\n00121 0xbfffea6c --> 0xbfffed24 --> 0xbfffef2b (\"/h0me/seed/Desktop/exploit\")\r\n00161 0xbfffea70 --> Oxb7fe3d39 (<check_match+9>: add ebx,0xlb2c7)\r\n00201 Oxbfffea74 --> Oxb7bf73d0 --> 0X94b90ca0\r\n\r\n00241 0xbfffea78 --> 0x53d\r\n\r\n00281 0xbfffea7c --> 0xb7ffd5b0 --> Oxb7bf3000 --> 0x464c457f\r\n\r\n[ ------------------------------------------------------------------------------ ]\r\n\r\nLegend: code, data, rodata, value\r\n\r\nBreakpoint 1, main (argc=0x1, argv=0xbfffed24) at exploit.c:25\r\n25 memset(&buffer, 0x90, 500);\r\n\r\ngdb-peda$ p &buffer\r\n\r\n$1 = (char (*)[500]) Oxbfffea78\r\n\r\ngdb-peda$ p $ebp\r\n\r\n$2 = (void *) 0xbfffec78\r\n\r\ngdb-peda$ p 0xbfffec78 .. 0xbfffea78\r\n\r\n$3 = OXZOO",
                IsBusy = false,
                IsEditingFileName = false
            };

            OcrDocument doc3 = new OcrDocument("C:\\Users\\dan\\Pictures\\GDB2.PNG")
            {
                FileName = "GDB2.PNG",
                OcrText = "(gdb) info frame\r\nStack level. 0, frame at 0xbfffeae0:\r\n\r\neip = 0x80484c1 in bof (stack.c:11); saved eip = 0x804852e\r\ncalled by frame at OxbfffedIO\r\n\r\nsource language c.\r\nArglist at Oxbfffead8, args:\r\n\r\nstr=0xbfffeaf8 \"1N300Ph//shh/bin\\211N343PS\\211.'Nj\"\r\n\r\nLocals at Oxbfffead8, Previous frame's sp is Oxbfffeae0\r\nSaved registers:\r\n\r\nebp at Oxbfffead8, eip at Oxbfffeadc",
                IsBusy = false,
                IsEditingFileName = false
            };

            List<OcrDocument> testLibrary = new List<OcrDocument> { doc1, doc2, doc3 };

            string serializedTestLibrary = JsonConvert.SerializeObject(testLibrary, Formatting.Indented);

            Assert.AreEqual(File.ReadAllText(@"TestFiles\library.json"), serializedTestLibrary);

            List<OcrDocument> deserializedLibrary =
                JsonConvert.DeserializeObject<List<OcrDocument>>(File.ReadAllText(@"TestFiles\library.json"));

            Assert.IsNotNull(deserializedLibrary);
            Assert.AreEqual(testLibrary.Count, deserializedLibrary.Count);
        }
    }
}
