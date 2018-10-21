using System;
using System.Collections.Generic;
using LogSync.Model;

namespace LogSync.Synchronization
{
    public class LogSyncer : ILogSyncer
    {
        private readonly ILogChunker _logChunker;

        public LogSyncer(ILogChunker logChunker)
        {
            _logChunker = logChunker;
        }

        // Take multiple unsynced logs and return multiple synced versions
        public Dictionary<string, ParsedLog> SyncLogs(Dictionary<string, ParsedLog> logs)
        {
            
            _logChunker.Load(logs);
            var outputLogs = new Dictionary<string, ParsedLog>();
            //var unprocessedLines = new Dictionary<string, List<string>>();
            foreach (var parsedLog in logs)
            {
                outputLogs.Add(parsedLog.Key, new ParsedLog());
            }

            var currentDate = DateTime.MinValue;
            DateTime? nextDate;
            while ((nextDate = _logChunker.GetNextTimestamp(currentDate)) != null)
            {
                currentDate = (DateTime)nextDate;
                var chunks = _logChunker.GetChunks(currentDate);
                foreach (var log in outputLogs)
                {
                    log.Value.Chunks.Add(currentDate, chunks[log.Key]);
                }
            }

            return outputLogs;
        }
    }
}
