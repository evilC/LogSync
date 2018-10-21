using System;
using System.Collections.Generic;
using LogSync.Model;

namespace LogSync.Synchronization
{
    public interface ILogChunker
    {
        Dictionary<string, LogChunk> GetChunks(DateTime timeStamp);
        DateTime? GetNextTimestamp(DateTime startDate);
        void Load(Dictionary<string, ParsedLog> logs);
    }
}