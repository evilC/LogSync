using System;
using System.Collections.Generic;

namespace LogSync.Model
{
    public class LogChunk
    {
        public DateTime TimeStamp { get; }
        public List<string> Lines { get; set; }

        public LogChunk(DateTime timeStamp)
        {
            TimeStamp = timeStamp;
            Lines = new List<string>();
        }
    }
}
