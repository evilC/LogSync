using System;
using System.Collections.Generic;
using System.Linq;
using LogSync.Model;

namespace LogSync.Synchronization
{
    public class LogChunker : ILogChunker
    {
        private Dictionary<string, ParsedLog> _logs;
        private Dictionary<string, DateTime?> _nextDates;
        private Dictionary<string, int> _currentIndexes;

        public void Load(Dictionary<string, ParsedLog> logs)
        {
            _logs = logs;
            _nextDates = new Dictionary<string, DateTime?>(StringComparer.OrdinalIgnoreCase);
            _currentIndexes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var currentDate = DateTime.MaxValue; ;

            foreach (var log in logs)
            {
                var thisDate = log.Value.Chunks.FirstOrDefault().Key;
                _currentIndexes[log.Key] = 0;
                _nextDates[log.Key] = GetNextDate(log.Key);
                if (thisDate < currentDate)
                {
                    currentDate = thisDate;
                }
            }
        }

        public DateTime? GetNextTimestamp(DateTime startDate)
        {
            DateTime? t = null;
            foreach (var log in _nextDates)
            {
                if (log.Value != null && log.Value > startDate && (t == null || log.Value < t))
                {
                    t = log.Value;
                }
            }
            return t;
        }

        public Dictionary<string, LogChunk> GetChunks(DateTime timeStamp)
        {
            var outChunks = new Dictionary<string, LogChunk>();
            var maxLines = GetMaxLineCount(timeStamp);

            foreach (var log in _logs)
            {
                outChunks.Add(log.Key, new LogChunk(timeStamp));
                var outChunk = outChunks[log.Key];
                var addedLines = 0;
                if (log.Value.Chunks.ContainsKey(timeStamp))
                {
                    var thisChunk = log.Value.Chunks[timeStamp];
                    outChunk.Lines = thisChunk.Lines;
                    addedLines += thisChunk.Lines.Count;
                    _currentIndexes[log.Key]++;
                    _nextDates[log.Key] = GetNextDate(log.Key);
                }

                for (var i = 0; i < maxLines - addedLines; i++)
                {
                    outChunk.Lines.Add(string.Empty);
                }
            }

            return outChunks;
        }

        private DateTime? GetNextDate(string name)
        {
            var i = _currentIndexes[name];
            var log = _logs[name];
            if (i < log.Chunks.Count)
            {
                return log.Chunks.Keys[i];
            }

            return null;
        }

        private int GetMaxLineCount(DateTime timestamp)
        {
            var max = 0;
            foreach (var log in _logs)
            {
                if (!log.Value.Chunks.ContainsKey(timestamp)) continue;

                var lineCount = log.Value.Chunks[timestamp].Lines.Count;
                if (lineCount > max)
                {
                    max = lineCount;
                }
            }

            return max;
        }
    }
}
