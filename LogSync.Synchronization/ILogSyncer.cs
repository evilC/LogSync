using System.Collections.Generic;
using LogSync.Model;

namespace LogSync.Synchronization
{
    public interface ILogSyncer
    {
        Dictionary<string, ParsedLog> SyncLogs(Dictionary<string, ParsedLog> logs);
    }
}
