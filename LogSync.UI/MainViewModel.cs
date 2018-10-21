using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LogSync.DataAccess;
using LogSync.Model;
using LogSync.Synchronization;

namespace LogSync.UI
{
    public class MainViewModel : IMainViewModel
    {
        public Dictionary<string, LogViewModel> LogViewModels { get; set; }
        private readonly IFileReader _fileReader;
        private readonly IFileParser _fileParser;
        private readonly ILogSyncer _logSyncer;
        private readonly Dictionary<string, ParsedLog> _unSyncedLogs;
        private Dictionary<string, ParsedLog> _syncedLogs;

        public MainViewModel(IFileReader fileReader, IFileParser fileParser, ILogSyncer logSyncer )
        {
            LogViewModels = new Dictionary<string, LogViewModel>(StringComparer.OrdinalIgnoreCase);
            _unSyncedLogs = new Dictionary<string, ParsedLog>(StringComparer.OrdinalIgnoreCase);
            _fileReader = fileReader;
            _fileParser = fileParser;
            _logSyncer = logSyncer;
        }

        // Endpoint for Main Window "Add File" command
        public void AddFile(string path)
        {
            var log = _fileReader.GetFileRawLines(path);
            _unSyncedLogs[path] = _fileParser.ParseRawLines(log);
        }

        // Endpoint for Main Window "Sync Logs" command
        public void SyncLogs()
        {
            _syncedLogs = _logSyncer.SyncLogs(_unSyncedLogs);
        }

        public void DisplayLogs()
        {
            foreach (var syncedLog in _syncedLogs)
            {
                // Create LogViewModels here
            }
        }
    }
}
