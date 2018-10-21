using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LogSync.Model;

namespace LogSyncer
{
    public class DefaultSyncer : ILogSyncer
    {

        // Take multiple unsynced logs and return multiple synced versions
        public Dictionary<string, ParsedLog> SyncLogs(Dictionary<string, ParsedLog> logs)
        {
            throw new NotImplementedException();
        }
    }
}
