using EscaperWPF.ViewModel;
using System;
using System.Windows;

namespace EscaperWPF
{
    public partial class App : Application
    {
        private MainWindow _window;
        private MainViewModel _viewModel;

        public App()
        {
            Startup += App_Startup;
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            _viewModel = new MainViewModel();

            _window = new MainWindow
            {
                DataContext = _viewModel
            };

            _window.Show();
        }
    }
}
