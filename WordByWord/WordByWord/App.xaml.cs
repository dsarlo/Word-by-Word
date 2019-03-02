using System;
using System.Windows;
using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using WordByWord.Services;

namespace WordByWord
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<ILoggerFactory>(() => new LoggerFactory());

            ILogger logger = SimpleIoc.Default.GetInstance<ILoggerFactory>().CreateLogger<Application>();

            logger.LogDebug("Initializing services");

            SimpleIoc.Default.Register<IWindowService, WindowService>();
            SimpleIoc.Default.Register<IDialogCoordinator, DialogCoordinator>();
            SimpleIoc.Default.Register<ViewModel.ViewModel>();

            try
            {
                ServiceLocator.Current.GetInstance<IWindowService>()
                .ShowWindow(Services.Windows.Library, null);
            }
            catch (Exception ex)
            {
                logger.LogCritical("Critical Error: " + ex.Message);
            }
        }
    }
}
