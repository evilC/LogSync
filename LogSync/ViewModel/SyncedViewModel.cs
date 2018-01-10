using LogSync.View;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

/*
Project -> LogSync Properties... -> Command-line args
-loadlogs "..\..\..\Sample Data\Tachyon.ConsumerAPI.log" "..\..\..\Sample Data\Tachyon.CoreAPI.log"
*/

namespace LogSync.ViewModel
{
    /// <summary>
    /// Creates and coordinates multiple ViewModels
    /// </summary>
    public class SyncedViewModel
    {
        private MainWindow mainWindow;
        private Dictionary<string, LogView> logViews = new Dictionary<string, LogView>();
        private Dictionary<string, LogViewModel> logViewModels = new Dictionary<string, LogViewModel>();

        public SyncedViewModel(MainWindow mw, StartupEventArgs e)
        {
            mainWindow = mw;

            ProcessCommandLineArguments(e);

            mainWindow.Show();
        }

        public void LoadLogs(string[] logs)
        {
            for (int i = 0; i < logs.Length; i++)
            {
                var logViewObject = new LogView(this, logs[i]);
                logViews.Add(logs[i], logViewObject);
            }
        }

        /// <summary>
        /// Adds a log, and returns the viewmodel, so it can be databound to.
        /// </summary>
        /// <param name="logPath">The path to the log file</param>
        /// <returns>The ViewModel to DataBind to</returns>
        public LogViewModel AddLog(string logPath, string logTitle)
        {
            var logViewModel = new LogViewModel();
            logViewModel.LoadLog(logPath, logTitle);
            logViewModels.Add(logPath, logViewModel);
            return logViewModel;
        }

        public void SyncLogs()
        {
            var i = 0;
            var logs = new List<string>();

            foreach (var logView in logViews)
            {
                logs.Add(logView.Key);
            }
            var titles = GetLogTitles(logs);

            // Remove any old views
            mainWindow.logGrid.Children.Clear();
            mainWindow.logGrid.ColumnDefinitions.Clear();

            foreach (var logView in logViews)
            {
                // Add a new Column to the Grid
                var col = new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) };
                mainWindow.logGrid.ColumnDefinitions.Add(col);
                // Add the View to the Grid
                //Grid.SetRow(logViewObject, 0);
                Grid.SetColumn(logView.Value, i);
                mainWindow.logGrid.Children.Add(logView.Value);

                if (!logViewModels.ContainsKey(logView.Key))
                {
                    // Create the ViewModel
                    AddLog(logView.Key, titles[logView.Key]);
                }

                logView.Value.DataContext = logViewModels[logView.Key];

                i++;
            }

            var syncer = new ViewModelSyncer(logViewModels);
            syncer.Sync();
        }

        private Dictionary<string, string> GetLogTitles(List<string> logs)
        {
            var dict = new Dictionary<string, string>();
            // If only one log, return the full title
            if (logs.Count < 2)
            {
                foreach (var log in logs)
                {
                    dict.Add(log, log);
                }
            }
            else
            {
                var pathChunks = new List<string[]>();
                for (int i = 0; i < logs.Count; i++)
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
                for (int i = 0; i < pathChunks.Count; i++)
                {
                    dict.Add(logs[i], Path.Combine(pathChunks[i]));
                }

            }

            return dict;
        }

        #region View Synchronization
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
        #endregion

        public void CloseView(string id)
        {
            if (logViews.ContainsKey(id))
            {
                logViews.Remove(id);
                logViewModels.Remove(id);
                SyncLogs();
            }
        }

        #region Command-Line Argument processing
        private void ProcessCommandLineArguments(StartupEventArgs e)
        {
            var args = ChunkCommandLineArguments(e);

            foreach (var arg in args)
            {
                switch (arg.Key.ToLower())
                {
                    case "-loadlogs":
                        LoadLogs(arg.Value.ToArray());
                        SyncLogs();
                        break;
                    default:
                        var str = string.Format("Invalid Argument '{0}'", arg);
                        Console.WriteLine(str);
                        throw new Exception(str);
                }
            }
        }

        private Dictionary<string, List<string>> ChunkCommandLineArguments(StartupEventArgs e)
        {
            var dict = new Dictionary<string, List<string>>();
            var max = e.Args.Length;

            int currentArg = 0;

            while (currentArg < max)
            {
                if (e.Args[currentArg][0] == '-')
                {
                    var subArgs = GetNonPrefixedArgs(e, currentArg + 1);
                    dict.Add(e.Args[currentArg], subArgs);
                    currentArg += 1 + subArgs.Count;
                }
                else
                {
                    var str = string.Format("Error: Not expecting non-argument parameter '{0}'", e.Args[currentArg]);
                    Console.WriteLine(str);
                    throw new Exception(str);
                }
            }
            return dict;
        }

        private List<string> GetNonPrefixedArgs(StartupEventArgs e, int offset)
        {
            var max = e.Args.Length;
            var chunks = new List<string>();
            for (int i = offset; i < max; i++)
            {
                if (e.Args[i][0] == '-')
                {
                    break;
                }
                chunks.Add(e.Args[i]);
            }
            return chunks;
        }

        #endregion
    }
}
