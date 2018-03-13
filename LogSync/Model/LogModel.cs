using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LogSync.Model
{
    class LogModel
    {
        public SortedList<DateTime, List<string>> LogLinesByTimestamp { get { return logLinesByTimestamp; } }
        private SortedList<DateTime, List<string>> logLinesByTimestamp;
        static Regex regex = new Regex(@"(.*?)?(?<datetime>(?<date>(?<year>\d+)-(?<month>\d+)-(?<day>\d+)) (?<time>(?<hours>\d+):(?<minutes>\d+):(?<seconds>\d+)((?<mssep>[,\.])(?<milliseconds>\d+))?))(?<timetextsep>\s+?)?(?<text>.*)");
        private string logPath;

        public LogModel(string path)
        {
            logPath = path;
            var text = File.ReadAllText(path);
            var lines = text.Split(
                new[] { "\r\n", "\r", "\n" },
                StringSplitOptions.None
            );
            DateTime lastSeenDate = DateTime.MinValue;
            DateTime ts;

            logLinesByTimestamp = new SortedList<DateTime, List<string>>();

            // Find first timestamp
            for (int i = 0; i < lines.Length; i++)
            {
                var logLine = ParseLine(lines[i]);
                if (logLine.Timestamp != null)
                {
                    lastSeenDate = (DateTime)logLine.Timestamp;
                    break;
                }
            }
            if (lastSeenDate == DateTime.MinValue)
            {
                throw new Exception(string.Format("Could not find any valid timestamps in file {0}", logPath));
            }

            // Parse the log
            for (int i = 0; i < lines.Length; i++)
            {
                var logLine = ParseLine(lines[i]);

                if (logLine.Timestamp == null)
                {
                    if (logLine.Text == string.Empty)
                    {
                        // Completely blank line - ignore
                        continue;
                    }
                    ts = lastSeenDate;
                }
                else
                {
                    ts = (DateTime)logLine.Timestamp;
                }
                //lastSeenDate = ts;

                if (!logLinesByTimestamp.ContainsKey(ts))
                {
                    logLinesByTimestamp.Add(ts, new List<string>());
                }
                logLinesByTimestamp[ts].Add(logLine.Text);
            }
        }

        private LogLine ParseLine(string line)
        {
            DateTime? ts;
            var formatStr = "yyyy-MM-dd HH:mm:ss";
            var match = regex.Match(line);
            string lineDateStr = match.Groups["datetime"].Value;
            string lineTextStr = match.Groups["text"].Value;
            string msSeparator = match.Groups["mssep"].Value;
            string msValue = match.Groups["milliseconds"].Value;
            if (!string.IsNullOrEmpty(msValue) && !string.IsNullOrEmpty(msSeparator))
            {
                formatStr += $"{msSeparator}fff";
            }
            if (lineDateStr == string.Empty)
            {
                ts = null;
            }
            else
            {
                try
                {
                    ts = DateTime.ParseExact(
                        lineDateStr,
                        //string.Format("yyyy-MM-dd HH:mm:ss{0}fff", msSeparator),
                        formatStr,
                        CultureInfo.InvariantCulture);
                }
                catch
                {
                    ts = null;
                }
            }
            return new LogLine() { Timestamp = ts, Text = lineTextStr };
        }
    }

    public class LogLine
    {
        public DateTime? Timestamp { get; set; }
        public string Text { get; set; }
    }
}
