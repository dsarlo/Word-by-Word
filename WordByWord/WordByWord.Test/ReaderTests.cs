using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading.Tasks;
using WordByWord.Models;
using WordByWord.Services;

namespace WordByWord.Test
{
    [TestClass]
    public class ReaderTests
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
        public async Task SplitIntoGroups()
        {
            OcrDocument testDocument = new OcrDocument("test")
            {
                OcrText = "I solemnly swear\r\nI am up to no good."
            };
            _viewModel.SelectedDocument = testDocument;

            // 1 Word at a time
            _viewModel.NumberOfGroups = 1;
            List<string> expected1 = new List<string>() { "I", "solemnly", "swear", "I", "am", "up", "to", "no", "good." };
            List<string> result1 = await _viewModel.SplitIntoGroups();

            CollectionAssert.AreEqual(expected1, result1);

            // 2 Words at a time
            _viewModel.NumberOfGroups = 2;
            List<string> expected2 = new List<string>() { "I solemnly", "swear I", "am up", "to no", "good." };
            List<string> result2 = await _viewModel.SplitIntoGroups();

            CollectionAssert.AreEqual(expected2, result2);

            // 3 Words at a time
            _viewModel.NumberOfGroups = 3;
            List<string> expected3 = new List<string>() { "I solemnly swear", "I am up", "to no good." };
            List<string> result3 = await _viewModel.SplitIntoGroups();

            CollectionAssert.AreEqual(expected3, result3);

            // 4 Words at a time
            _viewModel.NumberOfGroups = 4;
            List<string> expected4 = new List<string>() { "I solemnly swear I", "am up to no", "good." };
            List<string> result4 = await _viewModel.SplitIntoGroups();

            CollectionAssert.AreEqual(expected4, result4);

            // 5 Words at a time
            _viewModel.NumberOfGroups = 5;
            List<string> expected5 = new List<string>() { "I solemnly swear I am", "up to no good." };
            List<string> result5 = await _viewModel.SplitIntoGroups();

            CollectionAssert.AreEqual(expected5, result5);
        }

        [TestMethod]
        public async Task SplitIntoSentences()
        {
            string testingString = "Did you ever hear the tragedy of Darth Plagueis the Wise? " +
                "\r\nI thought not. It's not a story the Jedi would tell you. It's a Sith legend. " +
                "\r\nDarth Plagueis was a Dark Lord of the Sith, so powerful and so wise he could use the Force to influence the midichlorians to create life...";

            OcrDocument testDocument = new OcrDocument("test")
            {
                OcrText = testingString
            };
            _viewModel.SelectedDocument = testDocument;
            
            // 1 Sentence at a time
            _viewModel.NumberOfSentences = 1;
            string[] expected1 = { "Did you ever hear the tragedy of Darth Plagueis the Wise?", "I thought not.",
                "It's not a story the Jedi would tell you.", "It's a Sith legend.", "Darth Plagueis was a Dark " +
                "Lord of the Sith, so powerful and so wise he could use the Force to influence the midichlorians to create life…" };
            List<string> result1 = await _viewModel.SplitIntoSentences();

            CollectionAssert.AreEqual(expected1, result1);

            // 2 Sentences at a time
            _viewModel.NumberOfSentences = 2;
            string[] expected2 = { "Did you ever hear the tragedy of Darth Plagueis the Wise? I thought not.",
                "It's not a story the Jedi would tell you. It's a Sith legend.", "Darth Plagueis was a Dark " +
                "Lord of the Sith, so powerful and so wise he could use the Force to influence the midichlorians to create life…" };
            List<string> result2 = await _viewModel.SplitIntoSentences();

            CollectionAssert.AreEqual(expected2, result2);

            // 3 Sentences at a time
            _viewModel.NumberOfSentences = 3;
            string[] expected3 = { "Did you ever hear the tragedy of Darth Plagueis the Wise? I thought not. " +
                    "It's not a story the Jedi would tell you.", "It's a Sith legend. Darth Plagueis was a Dark " +
                    "Lord of the Sith, so powerful and so wise he could use the Force to influence the midichlorians to create life…" };
            List<string> result3 = await _viewModel.SplitIntoSentences();

            CollectionAssert.AreEqual(expected3, result3);
        }

        [TestMethod]
        public void CalculateRelayDelay()
        {
            int group = 3;
            _viewModel.WordsPerMinute = 200;

            // We expect a group of 3 words @ 200 wpm to be ~900 ms of delay for the reader
            int expected = 900;
            _viewModel.CalculateRelayDelay(group);

            Assert.AreEqual(_viewModel.ReaderDelay, expected); 
        }

        [TestMethod]
        public async Task DefineWord()
        {
            string word = "eggplant";
            string expectedDef = "A widely cultivated perennial Asian herb (Solanum melongena) of the nightshade family yielding edible fruit";
            string result = await Dictionary.DefineAsync(word);

            Assert.AreEqual(expectedDef, result);
        }
    }
}
