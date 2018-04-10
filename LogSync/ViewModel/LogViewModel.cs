using LogSync.Model;
using LogSync.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace LogSync.ViewModel
{
    /// <summary>
    /// Holds a single synced log
    /// </summary>
    public class LogViewModel
    {
        public ObservableCollection<LogLine> ViewModelLines { get; set; } = new ObservableCollection<LogLine>();
        public string LogTitle { get; set; }
        public string LogPath { get { return logPath; } }

        private string logPath;

        public bool IsFinished { get { return isFinished; } }

        private DateTime currentParseTime;
        private bool isFinished = false;

        private string lastSearchTerm;
        private int lastSearchResultIndex = -1;

        private LogModel logModel;

        /// <summary>
        /// Loads a log
        /// </summary>
        /// <param name="path">Path to the log file</param>
        public void LoadLog(string path, string title)
        {
            LogTitle = title;
            logPath = path;
            logModel = new LogModel(path);
        }

        // Start the parse process
        public void InitParse()
        {
            currentParseTime = logModel.LogLinesByTimestamp.FirstOrDefault().Key;
            isFinished = false;
            ViewModelLines.Clear();
        }

        // Move on to the next parse line
        public void ParseNext()
        {
            var currentParseIndex = GetCurrentIndex() + 1;
            if (!IndexIsWithinRange(currentParseIndex))
            {
                isFinished = true;
                return;
            }
            currentParseTime = logModel.LogLinesByTimestamp.Keys[currentParseIndex];
            isFinished = false;
        }

        // Adds lines to the ViewModel for a given Timestamp
        public void AddLinesForTimestamp(DateTime ts, int max)
        {
            int count = 0;
            if (logModel.LogLinesByTimestamp.ContainsKey(ts))
            {
                foreach (var line in logModel.LogLinesByTimestamp[ts])
                {
                    ViewModelLines.Add(new LogLine() { Timestamp = ts, Text = line });
                    count++;
                }

            }
            while (count < max)
            {
                ViewModelLines.Add(new LogLine() { Timestamp = null, Text = string.Empty});
                count++;
            }
        }

        public LogParseData GetFirstLogParseData()
        {
            var ts = logModel.LogLinesByTimestamp.FirstOrDefault().Key;
            return GetLogParseData(ts);
        }

        public LogParseData GetCurrentLogParseData()
        {
            return GetLogParseData(currentParseTime);
        }

        public LogParseData GetLogParseData(DateTime ts)
        {
            if (logModel.LogLinesByTimestamp.ContainsKey(ts))
            {
                return new LogParseData() { Timestamp = ts, LineCount = logModel.LogLinesByTimestamp[ts].Count, IsFinished = isFinished };
            }
            else
            {
                return new LogParseData() { Timestamp = ts, LineCount = 0,IsFinished = isFinished };
            }
        }

        private int GetCurrentIndex()
        {
            return logModel.LogLinesByTimestamp.IndexOfKey(currentParseTime);
        }

        private bool IndexIsWithinRange(int i)
        {
            var max = logModel.LogLinesByTimestamp.Count;
            return (i < max);
        }

        public int Search(string text, int startPos = 0)
        {
            //var startPos = text == lastSearchTerm ? lastSearchResultIndex + 1 : 0;
            lastSearchTerm = text;

            for (int i = startPos; i < ViewModelLines.Count; i++)
            {
                var line = ViewModelLines[i].Text;
                if (line.IndexOf(text, StringComparison.OrdinalIgnoreCase) > 0)
                {
                    //return ViewModelLines[i];
                    return i;
                }
            }
            return -1;
        }

        private static bool LineMatches()
        {
            return true;
        }
    }

    public class LogParseData
    {
        public bool IsFinished { get; set; } = false;
        public DateTime Timestamp { get; set; }
        public int LineCount { get; set; } = 0;
        //public List<string> Lines { get; set; }
    }
}
