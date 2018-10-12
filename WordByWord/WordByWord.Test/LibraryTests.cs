﻿using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WordByWord.Helpers;

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
        public void ImageCharacterRecognitionTest()
        {
            string ocrResult = _viewModel.GetTextFromImage(@"TestFiles\LockeEssay.PNG");
            Assert.IsTrue(ocrResult.Length > 0);
        }
    }
}
