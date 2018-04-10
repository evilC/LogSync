using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LogSync.ViewModel;
using LogSync.View;
using SWF = System.Windows.Forms;

namespace LogSync
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public SyncedViewModel logSync;
        SearchView searchView;

        public MainWindow(StartupEventArgs e)
        {
            InitializeComponent();

            logSync = new SyncedViewModel(this, e);
            searchView = new SearchView(logSync);
        }

        /// <summary>
        /// Called when the user uses the UI to open logs
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Open_Click(object sender, RoutedEventArgs e)
        {
            // Open file picker Dialog
            SWF.OpenFileDialog o = new SWF.OpenFileDialog();
            o.Multiselect = true;
            o.ShowDialog();

            // Exit if user did not select any files
            if (o.FileNames.Length == 0)
            {
                return;
            }
            logSync.LoadLogs(o.FileNames);
        }

        private void Sync_Click(object sender, RoutedEventArgs e)
        {
            logSync.SyncLogs();
        }

        private void SearchCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void SearchForwardsCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            searchView.SetLog();
            searchView.ShowDialog();
            //logSync.Search("Smoke - Verify Agent installation");
        }

        private void SearchBackwardsCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //logSync.Search("New Feat");
        }

    }

    public static class CustomCommands
    {
        public static readonly RoutedUICommand SearchForwards = new RoutedUICommand
                (
                        "SearchForwards",
                        "SearchForwards",
                        typeof(CustomCommands),
                        new InputGestureCollection()
                        {
                                        new KeyGesture(Key.F3, ModifierKeys.None)
                        }
                );

        public static readonly RoutedUICommand SearchBackwards = new RoutedUICommand
                (
                        "SearchBackwards",
                        "SearchBackwards",
                        typeof(CustomCommands),
                        new InputGestureCollection()
                        {
                                        new KeyGesture(Key.F3, ModifierKeys.Shift)
                        }
                );

    }
}
