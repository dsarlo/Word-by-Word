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
            List<string> expected1= new List<string>() { "I", "solemnly", "swear", "I", "am", "up", "to", "no", "good." };
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
        public void Test()
        {

        }
    }
}
