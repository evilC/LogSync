using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LogSync.Model;

namespace LogSyncer
{
    public interface ILogSyncer
    {
        Dictionary<string, ParsedLog> SyncLogs(Dictionary<string, ParsedLog> logs);
    }
}
