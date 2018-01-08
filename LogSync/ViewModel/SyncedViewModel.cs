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

            var args = ProcessCommandLineArguments(e);

            foreach (var arg in args)
            {
                switch (arg.Key.ToLower())
                {
                    case "-loadlogs":
                        LoadLogs(arg.Value.ToArray());
                        break;
                    default:
                        var str = string.Format("Invalid Argument '{0}'", arg);
                        Console.WriteLine(str);
                        throw new Exception(str);
                }
            }

            mainWindow.Show();
        }

        private Dictionary<string, List<string>> ProcessCommandLineArguments(StartupEventArgs e)
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

        public void LoadLogs(string[] logs)
        {
            // Remove any old views
            mainWindow.logGrid.Children.Clear();
            mainWindow.logGrid.ColumnDefinitions.Clear();

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
                mainWindow.logGrid.ColumnDefinitions.Add(col);
                // Add the View to the Grid
                //Grid.SetRow(logViewObject, 0);
                Grid.SetColumn(logViewObject, i);
                mainWindow.logGrid.Children.Add(logViewObject);

                // Create the ViewModel
                var logViewModel = AddLog(logs[i], titles[i]);

                // Bind data to ViewModel
                logViewObject.DataContext = logViewModel;
            }

            SyncLogs();
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
            logViewModel.InitParse();
            logViewModels.Add(logPath, logViewModel);
            return logViewModel;
        }

        public void SyncLogs()
        {
            var syncer = new ViewModelSyncer(logViewModels);
            syncer.Sync();
        }

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
        /// Syncs multiple ViewModels
        /// </summary>
        private class ViewModelSyncer
        {
            private Dictionary<string, LogViewModel> viewModels;
            private Dictionary<string, bool> logsParsed = new Dictionary<string, bool>();

            public ViewModelSyncer(Dictionary<string, LogViewModel> vms)
            {
                viewModels = vms;
            }

            public void Sync()
            {
                var logs = new List<string>();
                foreach (var viewModel in viewModels)
                {
                    logs.Add(viewModel.Key);
                }
                var chunker = new LogChunkComparer(logs.ToArray());
                foreach (var viewModel in viewModels)
                {
                    logsParsed.Add(viewModel.Key, false);
                    viewModel.Value.InitParse();
                    // Load the first chunk
                    chunker.LoadChunk(viewModel.Key, viewModel.Value.GetFirstLogParseData());
                }

                while (AreLogsWaitingToBeParsed())
                {
                    // First work out which Timestamp we will be processing
                    var thisTime = chunker.GetFirstTimestamp();
                    // Find out what the max number of lines any log has with this timestamp
                    var max = chunker.GetMaxChunkLengthByTimestamp(thisTime);

                    foreach (var viewModel in viewModels)
                    {
                        // Get the count of lines for this Timestamp (could be 0)
                        var count = chunker.GetChunkCountByTimestamp(viewModel.Key, thisTime);
                        //var lines = chunker.GetChunkLinesByTimestamp(i, thisTime, max);

                        // Tell the ViewModel to add as many lines for this Timestamp as needed
                        viewModel.Value.AddLinesForTimestamp(thisTime, max);

                        // If we used actual lines from the log (Not all blank ones)...
                        if (count > 0)
                        {
                            // Move on to the next line of this log
                            viewModel.Value.ParseNext();
                            if (viewModel.Value.IsFinished)
                            {
                                logsParsed[viewModel.Key] = true;
                                chunker.LoadChunk(viewModel.Key, new LogParseData() { IsFinished = true, Timestamp = DateTime.MaxValue });
                            }
                            else
                            {
                                // Reload the Chunk for this log
                                chunker.LoadChunk(viewModel.Key, viewModel.Value.GetCurrentLogParseData());
                            }
                        }

                    }
                }
            }

            private bool AreLogsWaitingToBeParsed()
            {
                foreach (var viewModel in viewModels)
                {
                    if (logsParsed[viewModel.Key] == false)
                    {
                        return true;
                    }
                }
                return false;
            }

            /// <summary>
            /// A "chunk" is a set of lines from the same long that share the same timestamp
            /// Processes all chunks from all logs to generate synced logs
            /// </summary>
            private class LogChunkComparer
            {
                public Dictionary<string, LogParseData> Chunks { get; set; }

                public LogChunkComparer(string[] ids)
                {
                    Chunks = new Dictionary<string, LogParseData>();
                    foreach (var chunkName in ids)
                    {
                        Chunks.Add(chunkName, new LogParseData());
                    }
                }

                public void LoadChunk(string id, LogParseData lpd)
                {
                    Chunks[id] = lpd;
                }

                /// <summary>
                /// Finds the timestamp that is next to be processed
                /// </summary>
                /// <returns></returns>
                public DateTime GetFirstTimestamp()
                {
                    DateTime ts = DateTime.MaxValue;

                    foreach (var chunk in Chunks.Values)
                    {
                        if (chunk.Timestamp < ts)
                        {
                            ts = chunk.Timestamp;
                        }
                    }

                    return ts;
                }

                /// <summary>
                /// For a given chunk index and timestamp, gets the number of lines of the chunk
                /// </summary>
                /// <param name="i">The index of the chunk</param>
                /// <param name="ts">The timestamp of the chunk</param>
                /// <returns></returns>
                public int GetChunkCountByTimestamp(string id, DateTime ts)
                {
                    if (Chunks[id].Timestamp == ts)
                    {
                        return Chunks[id].LineCount;
                    }
                    return 0;
                }

                /// <summary>
                /// Gets line count of the longest chunk for a given timestamp
                /// </summary>
                /// <param name="ts">Timestamp of the chunk</param>
                /// <returns></returns>
                public int GetMaxChunkLengthByTimestamp(DateTime ts)
                {
                    int max = 0;
                    foreach (var chunk in Chunks)
                    {
                        var thisLength = GetChunkCountByTimestamp(chunk.Key, ts);

                        if (thisLength > max)
                        {
                            max = thisLength;
                        }
                    }
                    return max;
                }
            }
        }
    }
}
