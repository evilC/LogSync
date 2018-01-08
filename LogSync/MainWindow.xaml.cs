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
using Path = System.IO.Path;

/*
Project -> LogSync Properties... -> Command-line args
-loadlogs "..\..\..\Sample Data\Tachyon.ConsumerAPI.log" "..\..\..\Sample Data\Tachyon.CoreAPI.log"
*/

namespace LogSync
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public SyncedViewModel logSync;

        //private List<LogView> logViews = new List<LogView>();
        private Dictionary<string, LogView> logViews = new Dictionary<string, LogView>();

        public MainWindow()
        {
            InitializeComponent();

            logSync = new SyncedViewModel();
        }

        /// <summary>
        /// Creates LogViews from a list of logs
        /// </summary>
        /// <param name="logs"></param>
        public void LoadLogs(string[] logs)
        {
            // Remove any old views
            logGrid.Children.Clear();
            logGrid.ColumnDefinitions.Clear();

            // Create new views
            logViews = new Dictionary<string, LogView>();

            // Build Title names for logs
            var titles = GetLogTitles(logs);

            for (int i = 0; i < logs.Length; i++)
            {
                var logViewObject = new LogView(this, logs[i]);
                logViews.Add(logs[i], logViewObject);

                // Add a new Column to the Grid
                var col = new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) };
                logGrid.ColumnDefinitions.Add(col);
                // Add the View to the Grid
                //Grid.SetRow(logViewObject, 0);
                Grid.SetColumn(logViewObject, i);
                logGrid.Children.Add(logViewObject);

                // Create the ViewModel
                var logViewModel = logSync.AddLog(logs[i], titles[i]);

                // Bind data to ViewModel
                logViewObject.DataContext = logViewModel;
            }

            // Tell ViewModel to sync the logs
            logSync.SyncLogs();
        }

        /// <summary>
        /// Removes common parts of the log paths, to shorten the titles
        /// </summary>
        /// <param name="logs"></param>
        /// <returns></returns>
        private string[] GetLogTitles(string[] logs)
        {
            // If only one log, return the full title
            if (logs.Length < 2)
            {
                return logs;
            }
            var pathChunks = new List<string[]>();
            for (int i = 0; i < logs.Length; i++)
            {
                string path = Path.GetFullPath(logs[i]);
                pathChunks.Add(path.Split(Path.DirectorySeparatorChar));
            }

            var max = pathChunks.Count;

            // While the first element of each of the arrays in the list are equal, remove them
            while (pathChunks.Where(path => path.Length == 0)               // Is there only the filename left?
                .ToList().Count != max                                      // Abort if all paths only have filename left
                &&
                pathChunks.Where(path => path.FirstOrDefault() != null)     // Get the path from the paths...
                    .Select(chunk => chunk.FirstOrDefault())                // Select the first element of each path array...
                    .Distinct().Count() == 1)                               // If they are all identical...
            {
                // Remove the first element of each path array, as it is common to all logs
                pathChunks.ForEach(path => path.ToList().RemoveAt(0));

                for (int i = 0; i < pathChunks.Count; i++)
                {
                    pathChunks[i] = pathChunks[i].Skip(1).ToArray();
                }
            }

            // Build output
            var paths = new List<string>();
            for (int i = 0; i < pathChunks.Count; i++)
            {
                paths.Add(Path.Combine(pathChunks[i]));
            }

            return paths.ToArray();
        }

        /// <summary>
        /// Called when one of the child windows scrolls
        /// Tells all the other windows to scroll to match
        /// </summary>
        /// <param name="id">The id of the child window. A lookup to the logViews array</param>
        /// <param name="e">The ScrollChangedEventArgs of the original scroll</param>
        public void OnScrollChanged(string id, ScrollChangedEventArgs e)
        {
            // Iterate through all views
            foreach (var logView in logViews)
            {
                if (logView.Key == id)
                {
                    continue;
                }
                logViews[logView.Key].DoScroll(e);
            }
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
            LoadLogs(o.FileNames);
        }

    }
}
