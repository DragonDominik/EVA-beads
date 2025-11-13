using System.Windows;
using System.Windows.Input;
using EscaperWPF.ViewModel;

namespace EscaperWPF
{
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = DataContext as MainViewModel;
        }
    }
}
