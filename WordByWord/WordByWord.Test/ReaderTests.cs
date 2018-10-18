using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using WordByWord.Helpers;

namespace WordByWord.Test
{
    [TestClass]
    public class ReaderTests
    {
        private static ViewModel.ViewModel _viewModel;
        private static PrivateObject _viewModelPrivate;

        [ClassInitialize]
        public static void TestSetup(TestContext testContext)
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<IWindowService, WindowService>();
            SimpleIoc.Default.Register<IDialogCoordinator, DialogCoordinator>();
            SimpleIoc.Default.Register<ViewModel.ViewModel>();

            _viewModel = ServiceLocator.Current.GetInstance<ViewModel.ViewModel>();
            _viewModelPrivate = new PrivateObject(_viewModel);
        }

        [TestMethod]
        public async Task GroupByFactor()
        {
            string testingString = "I solemnly swear\r\nI am up to no good.";
            
            // 1 Word at a time
            int grouping1 = 1;
            object[] parms1 = { testingString, grouping1 };
            List<string> expected1 = new List<string>() { "I", "solemnly", "swear", "I", "am", "up", "to", "no", "good." };
            List<string> result1 = await (Task<List<string>>)_viewModelPrivate.Invoke("SplitIntoGroups", parms1);

            CollectionAssert.AreEqual(expected1, result1);

            // 2 Words at a time
            int grouping2 = 2;
            object[] parms2 = { testingString, grouping2 };
            List<string> expected2 = new List<string>() { "I solemnly", "swear I", "am up", "to no", "good." };
            List<string> result2 = await (Task<List<string>>) _viewModelPrivate.Invoke("SplitIntoGroups", parms2);

            CollectionAssert.AreEqual(expected2, result2);

            // 3 Words at a time
            int grouping3 = 3;
            object[] parms3 = { testingString, grouping3 };
            List<string> expected3 = new List<string>() { "I solemnly swear", "I am up", "to no good." };
            List<string> result3 = await (Task<List<string>>)_viewModelPrivate.Invoke("SplitIntoGroups", parms3);

            CollectionAssert.AreEqual(expected3, result3);

            // 4 Words at a time
            int grouping4 = 4;
            object[] parms4 = { testingString, grouping4 };
            List<string> expected4 = new List<string>() { "I solemnly swear I", "am up to no", "good." };
            List<string> result4 = await (Task<List<string>>)_viewModelPrivate.Invoke("SplitIntoGroups", parms4);

            CollectionAssert.AreEqual(expected4, result4);

            // 5 Words at a time
            int grouping5 = 5;
            object[] parms5 = { testingString, grouping5 };
            List<string> expected5 = new List<string>() { "I solemnly swear I am", "up to no good." };
            List<string> result5 = await (Task<List<string>>)_viewModelPrivate.Invoke("SplitIntoGroups", parms5);

            CollectionAssert.AreEqual(expected5, result5);
        }

        [TestMethod]
        public async Task SplitIntoSentences()
        {
            string testingString = "Did you ever hear the tragedy of Darth Plagueis the Wise? " +
                "\r\nI thought not. It's not a story the Jedi would tell you. It's a Sith legend. " +
                "\r\nDarth Plagueis was a Dark Lord of the Sith, so powerful and so wise he could use the Force to influence the midichlorians to create life...";

            string testingString = "I mean, nobody wishes more than I do that it had all been quick and clean, and my head had come off properly, I mean, it would have saved me a great deal of pain and ridicule. However -\" Nearly Headless Nick shook his letter open and read furiously: \"'We can only accept huntsmen whose heads have parted company with their bodies. You will appreciate that it would be impossible otherwise for members to participate in hunt activities such as Horseback Head-Juggling and Head Polo. It is with the greatest regret, therefore, that I must inform you that you do not fulfill our requirements. With very best wishes, Sir Patrick Delaney-Podmore.'";

            // 1 Sentence at a time
            int numSentences1 = 1;
            object[] args1 = { testingString, numSentences1 };
            string[] expected1 = { "Did you ever hear the tragedy of Darth Plagueis the Wise?", "I thought not.",
                "It's not a story the Jedi would tell you.", "It's a Sith legend.", "Darth Plagueis was a Dark " +
                "Lord of the Sith, so powerful and so wise he could use the Force to influence the midichlorians to create life…" };
            string[] result1 = await (Task<string[]>) _viewModelPrivate.Invoke("SplitIntoSentences", args1);

            CollectionAssert.AreEqual(expected1, result1);

            // 2 Sentences at a time
            int numSentences2 = 2;
            object[] args2 = { testingString, numSentences2 };
            string[] expected2 = { "Did you ever hear the tragedy of Darth Plagueis the Wise? I thought not.",
                "It's not a story the Jedi would tell you. It's a Sith legend.", "Darth Plagueis was a Dark " +
                "Lord of the Sith, so powerful and so wise he could use the Force to influence the midichlorians to create life…" };
            string[] result2 = await (Task<string[]>) _viewModelPrivate.Invoke("SplitIntoSentences", args2);

            CollectionAssert.AreEqual(expected2, result2);

            // 3 Sentences at a time
            int numSentences3 = 3;
            object[] args3 = { testingString, numSentences3 };
            string[] expected3 = { "Did you ever hear the tragedy of Darth Plagueis the Wise? I thought not. " +
                    "It's not a story the Jedi would tell you.", "It's a Sith legend. Darth Plagueis was a Dark " +
                    "Lord of the Sith, so powerful and so wise he could use the Force to influence the midichlorians to create life…" };
            string[] result3 = await (Task<string[]>) _viewModelPrivate.Invoke("SplitIntoSentences", args3);

            CollectionAssert.AreEqual(expected3, result3);
        }
    }
}
