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

        public MainWindow(StartupEventArgs e)
        {
            InitializeComponent();

            logSync = new SyncedViewModel(this, e);
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
    }
}
