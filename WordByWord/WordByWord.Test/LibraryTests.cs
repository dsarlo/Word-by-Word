using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WordByWord.Test
{
    [TestClass]
    public class LibraryTests
    {
        private static ViewModel.ViewModel _viewModel;

        [ClassInitialize]
        public static void TestSetup(TestContext testContext)
        {
            _viewModel = new ViewModel.ViewModel();
        }

        [TestMethod]
        public void ImageCharacterRecognitionTest()
        {
            string ocrResult = _viewModel.GetTextFromImage(@"TestFiles\LockeEssay.PNG");
            Assert.IsTrue(ocrResult.Length > 0);
        }
    }
}
