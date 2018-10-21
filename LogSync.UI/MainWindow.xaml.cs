using System.Windows;
using LogSync.DataAccess;
using LogSyncer;

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
            _mainViewModel = new MainViewModel(new FileReader(), new MockFileParser(), new DefaultSyncer());
            _mainViewModel.AddFile("..\\..\\..\\Sample Data\\Sample Even.log");
            //_mainViewModel.SyncLogs();
            //_mainViewModel.DisplayLogs();
        }
    }
}
