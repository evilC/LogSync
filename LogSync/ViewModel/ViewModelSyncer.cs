using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogSync.ViewModel
{
    /// <summary>
    /// Syncs multiple ViewModels
    /// </summary>
    class ViewModelSyncer
    {
        private Dictionary<string, LogViewModel> viewModels;
        private Dictionary<string, bool> logsParsed = new Dictionary<string, bool>();

        public ViewModelSyncer(Dictionary<string, LogViewModel> vms)
        {
            viewModels = vms;
        }

        public void Sync()
        {
            var logs = new List<string>();
            foreach (var viewModel in viewModels)
            {
                logs.Add(viewModel.Key);
            }
            var chunker = new LogChunkComparer(logs.ToArray());
            foreach (var viewModel in viewModels)
            {
                logsParsed.Add(viewModel.Key, false);
                viewModel.Value.InitParse();
                // Load the first chunk
                chunker.LoadChunk(viewModel.Key, viewModel.Value.GetFirstLogParseData());
            }

            while (AreLogsWaitingToBeParsed())
            {
                // First work out which Timestamp we will be processing
                var thisTime = chunker.GetFirstTimestamp();
                // Find out what the max number of lines any log has with this timestamp
                var max = chunker.GetMaxChunkLengthByTimestamp(thisTime);

                foreach (var viewModel in viewModels)
                {
                    // Get the count of lines for this Timestamp (could be 0)
                    var count = chunker.GetChunkCountByTimestamp(viewModel.Key, thisTime);
                    //var lines = chunker.GetChunkLinesByTimestamp(i, thisTime, max);

                    // Tell the ViewModel to add as many lines for this Timestamp as needed
                    viewModel.Value.AddLinesForTimestamp(thisTime, max);

                    // If we used actual lines from the log (Not all blank ones)...
                    if (count > 0)
                    {
                        // Move on to the next line of this log
                        viewModel.Value.ParseNext();
                        if (viewModel.Value.IsFinished)
                        {
                            logsParsed[viewModel.Key] = true;
                            chunker.LoadChunk(viewModel.Key, new LogParseData() { IsFinished = true, Timestamp = DateTime.MaxValue });
                        }
                        else
                        {
                            // Reload the Chunk for this log
                            chunker.LoadChunk(viewModel.Key, viewModel.Value.GetCurrentLogParseData());
                        }
                    }

                }
            }
        }

        private bool AreLogsWaitingToBeParsed()
        {
            foreach (var viewModel in viewModels)
            {
                if (logsParsed[viewModel.Key] == false)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// A "chunk" is a set of lines from the same long that share the same timestamp
        /// Processes all chunks from all logs to generate synced logs
        /// </summary>
        private class LogChunkComparer
        {
            public Dictionary<string, LogParseData> Chunks { get; set; }

            public LogChunkComparer(string[] ids)
            {
                Chunks = new Dictionary<string, LogParseData>();
                foreach (var chunkName in ids)
                {
                    Chunks.Add(chunkName, new LogParseData());
                }
            }

            public void LoadChunk(string id, LogParseData lpd)
            {
                Chunks[id] = lpd;
            }

            /// <summary>
            /// Finds the timestamp that is next to be processed
            /// </summary>
            /// <returns></returns>
            public DateTime GetFirstTimestamp()
            {
                DateTime ts = DateTime.MaxValue;

                foreach (var chunk in Chunks.Values)
                {
                    if (chunk.Timestamp < ts)
                    {
                        ts = chunk.Timestamp;
                    }
                }

                return ts;
            }

            /// <summary>
            /// For a given chunk index and timestamp, gets the number of lines of the chunk
            /// </summary>
            /// <param name="i">The index of the chunk</param>
            /// <param name="ts">The timestamp of the chunk</param>
            /// <returns></returns>
            public int GetChunkCountByTimestamp(string id, DateTime ts)
            {
                if (Chunks[id].Timestamp == ts)
                {
                    return Chunks[id].LineCount;
                }
                return 0;
            }

            /// <summary>
            /// Gets line count of the longest chunk for a given timestamp
            /// </summary>
            /// <param name="ts">Timestamp of the chunk</param>
            /// <returns></returns>
            public int GetMaxChunkLengthByTimestamp(DateTime ts)
            {
                int max = 0;
                foreach (var chunk in Chunks)
                {
                    var thisLength = GetChunkCountByTimestamp(chunk.Key, ts);

                    if (thisLength > max)
                    {
                        max = thisLength;
                    }
                }
                return max;
            }
        }
    }

}
