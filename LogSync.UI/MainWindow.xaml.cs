using System.Windows;
using LogSync.DataAccess;
using LogSync.Synchronization;

namespace LogSync.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel _mainViewModel;

        public MainWindow()
        {
            InitializeComponent();
            // Simulte user opening some files for now
            _mainViewModel = new MainViewModel(new FileReader(), new FileParser(), new LogSyncer(new LogChunker()));
            //_mainViewModel.AddFile("..\\..\\..\\Sample Data\\Sample Even.log");
            //_mainViewModel.AddFile("..\\..\\..\\Sample Data\\Sample Odd.log");
            //_mainViewModel.SyncLogs();
            //_mainViewModel.DisplayLogs();
        }
    }
}
