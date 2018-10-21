using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogSync.Model
{
    public class ParsedLog
    {
        public IDictionary<DateTime, LogChunk> Chunks { get; set; }

        public ParsedLog()
        {
            Chunks = new Dictionary<DateTime, LogChunk>();
        }
    }
}
