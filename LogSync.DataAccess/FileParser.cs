using System;
using System.Collections.Generic;
using LogSync.Model;

namespace LogSync.DataAccess
{
    public class FileParser : IFileParser
    {
        public ParsedLog ParseRawLines(List<string> rawLines)
        {
            var baseTime = DateTime.MinValue;
            var log = new ParsedLog();
            foreach (var rawLine in rawLines)
            {
                var chunks = rawLine.Split('\t');
                var thisTime = baseTime.AddSeconds(Convert.ToInt32(chunks[0]));
                if (!log.Chunks.ContainsKey(thisTime))
                {
                    log.Chunks.Add(thisTime, new LogChunk(thisTime));
                }
                log.Chunks[thisTime].Lines.Add(chunks[1]);
            }

            return log;
        }
    }
}
