using System;
using System.Collections.Generic;
using System.IO;
using LogSync.DataAccess;
using LogSync.DataAccess.Tests;
using LogSync.Model;
using LogSync.Synchronization;
using NUnit.Framework;

namespace LogSync.Tests
{
    [TestFixture]
    public class LogSyncTests
    {
        private static readonly List<string> Numbers = new List<string>() {"Zero", "One", "Two", "Three", "Four", "Five", "Six"};
        private static readonly List<string> LogNames = new List<string>() {"Even", "Odd"};
        private static List<List<string>> _logs;

        private readonly Dictionary<string, ParsedLog> _parsedLogs;
        private readonly MockFileParser _parser;

        public LogSyncTests()
        {
            _parser = new MockFileParser();
            _logs = new List<List<string>>() {new List<string>(), new List<string>()};
            for (var i = 1; i < 7; i++)
            {
                var dict = i % 2;
                _logs[dict].Add(BuildRawLine(i));
                if (!IsDoubleEntry(i)) continue;
                _logs[dict].Add(BuildRawLine(i, true));
            }

            _parsedLogs = new Dictionary<string, ParsedLog>();
            for (var i = 0; i < 2; i++)
            {
                _parsedLogs.Add($"{LogNames[i]}", _parser.ParseRawLines(_logs[i]));
            }
        }

        [Test]
        public void MockFileParserTest()
        {
            var logs = new List<ParsedLog>() { _parser.ParseRawLines(_logs[0]), _parser.ParseRawLines(_logs[1]) };

            for (var i = 1; i < 7; i++)
            {
                // Odd log = 0, Even log = 1
                var dict = i % 2;
                var lines = logs[dict];

                // Calculate expected values
                var t = DateTime.MinValue.AddSeconds(i);
                var isDoubleEntry = IsDoubleEntry(i);
                var expectedCount = isDoubleEntry ? 2 : 1;

                // Each key is present in each log
                Assert.That(lines.Chunks, Contains.Key(t));
                // Each chunk has the expected number of items
                Assert.That(lines.Chunks[t].Lines.Count, Is.EqualTo(expectedCount));
                // Each chunk has the correct contents
                Assert.That(lines.Chunks[t].Lines[0], Is.EqualTo(BuildLineText(i)));
                if (!isDoubleEntry) continue;
                Assert.That(lines.Chunks[t].Lines[1], Is.EqualTo($"{BuildLineText(i, true)}"));
            }
        }

        [Test]
        public void FileReaderLoadsFiles()
        {
            var reader = new FileReader();
            var logs = new List<List<string>>
            {
                reader.GetFileRawLines(Path.Combine(TestContext.CurrentContext.TestDirectory,
                    "..\\..\\..\\Sample Data\\Sample Even.log")),
                reader.GetFileRawLines(Path.Combine(TestContext.CurrentContext.TestDirectory,
                    "..\\..\\..\\Sample Data\\Sample Odd.log")),
            };

            Assert.That(logs[0].Count, Is.EqualTo(4));
            Assert.That(logs[1].Count, Is.EqualTo(4));

            for (var i = 1; i < 7; i++)
            {
                var dict = i % 2;
                var lines = logs[dict][0];
                logs[dict].RemoveAt(0);

                // Calculate expected values
                var isDoubleEntry = IsDoubleEntry(i);
                //var expectedCount = isDoubleEntry ? 2 : 1;

                // Each key is present in each log
                Assert.That(lines, Is.EqualTo(BuildRawLine(i)));

                if (!isDoubleEntry) continue;
                lines = logs[dict][0];
                logs[dict].RemoveAt(0);
                Assert.That(lines, Is.EqualTo(BuildRawLine(i, true)));
            }
        }

        [Test]
        public void LogSyncerTest()
        {
            var syncer = new LogSyncer(new LogChunker());
            var logs = syncer.SyncLogs(_parsedLogs);

            for (var logNum = 0; logNum < 2; logNum++)
            {
                var log = logs[LogNames[logNum]];
                Assert.That(log.Chunks.Count, Is.EqualTo(6));
                for (var chunkNum = 1; chunkNum < 7; chunkNum++)
                {
                    var evenOdd = chunkNum % 2;
                    var expectedDate = DateTime.MinValue.AddSeconds(chunkNum);
                    var isDoubleEntry = IsDoubleEntry(chunkNum);
                    var expectedLines = isDoubleEntry ? 2 : 1;
                    Assert.That(log.Chunks.ContainsKey(expectedDate));
                    var chunk = log.Chunks[expectedDate];
                    Assert.That(chunk.Lines.Count, Is.EqualTo(expectedLines));

                    var expectedString = evenOdd == logNum ? BuildLineText(chunkNum) : string.Empty;
                    Assert.That(chunk.Lines[0], Is.EqualTo(expectedString));
                    if (!isDoubleEntry) continue;

                    expectedString = evenOdd == logNum ? BuildLineText(chunkNum, true) : string.Empty;
                    Assert.That(chunk.Lines[1], Is.EqualTo(expectedString));
                }
            }

            
        }

        private static string BuildRawLine(int index, bool again = false)
        {
            return $"{index}\t{BuildLineText(index, again)}";
        }

        private static string BuildLineText(int i, bool again = false)
        {
            var text = Numbers[i];
            if (again) text += " Again";
            return text;
        }

        private static bool IsDoubleEntry(int i)
        {
            return (i == 3 || i == 4);
        }
    }
}
