using System.Collections.Generic;

namespace LogSync.UI
{
    public interface IMainViewModel
    {
        Dictionary<string, LogViewModel> LogViewModels { get; set; }

        void AddFile(string path);
        void DisplayLogs();
        void SyncLogs();
    }
}