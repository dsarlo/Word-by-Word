using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WordByWord.Test
{
    [TestClass]
    public class ReaderTests
    {
        private static ViewModel.ViewModel _viewModel;

        [ClassInitialize]
        public static void TestSetup(TestContext testContext)
        {
            _viewModel = new ViewModel.ViewModel();
        }

        [TestMethod]
        public void TestMethod1()
        {
        }
    }
}
