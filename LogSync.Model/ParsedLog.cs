using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogSync.Model
{
    public class ParsedLog
    {
        public SortedList<DateTime, LogChunk> Chunks { get; set; }

        public ParsedLog()
        {
            Chunks = new SortedList<DateTime, LogChunk>();
        }
    }
}
