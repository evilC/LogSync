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

        public LogModel(string path)
        {
            var text = File.ReadAllText(path);
            var lines = text.Split(
                new[] { "\r", "\n" },
                StringSplitOptions.None
            );

            var regex = new Regex(@"(?<datetime>(?<date>(?<year>\d+)-(?<month>\d+)-(?<day>\d+)) (?<time>(?<hours>\d+):(?<minutes>\d+):(?<seconds>\d+)(?<mssep>[,\.])(?<milliseconds>\d+))) (?<text>.*)");

            logLinesByTimestamp = new SortedList<DateTime, List<string>>();

            for (int i = 0; i < lines.Length; i++)
            {
                var match = regex.Match(lines[i]);
                string lineDateStr = match.Groups["datetime"].Value;
                string lineTextStr = match.Groups["text"].Value;
                string msSeparator = match.Groups["mssep"].Value;
                if (lineDateStr == string.Empty)
                {
                    if (lineTextStr == string.Empty)
                    {
                        continue;
                    }
                    throw new Exception(string.Format("Line {0} of file '{1}' did not match Regex.\n{2}", i, path , lines[i]));
                }
                DateTime ts = DateTime.ParseExact(
                    lineDateStr, 
                    string.Format("yyyy-MM-dd HH:mm:ss{0}fff", msSeparator), 
                    CultureInfo.InvariantCulture);

                if (!logLinesByTimestamp.ContainsKey(ts))
                {
                    logLinesByTimestamp.Add(ts, new List<string>());
                }
                logLinesByTimestamp[ts].Add(lineTextStr);
            }
        }
    }

    public class LogLine
    {
        public DateTime? Timestamp { get; set; }
        public string Text { get; set; }
    }
}
