using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using LogSync.Model;

namespace LogSync.DataAccess.Tests
{
    [TestFixture]
    public class TestClass1
    {
        private static readonly List<string> Numbers = new List<string>() {"Zero", "One", "Two", "Three", "Four", "Five", "Six"};
        private static List<List<string>> _logs;

        public TestClass1()
        {
            _logs = new List<List<string>>() {new List<string>(), new List<string>()};
            for (var i = 1; i < 7; i++)
            {
                var dict = i % 2;
                _logs[dict].Add(BuildRawLine(i));
                if (!IsDoubleEntry(i)) continue;
                _logs[dict].Add(BuildRawLine(i, true));
            }
        }

        [Test]
        public void MockFileParserTest()
        {
            var parser = new MockFileParser();
            var logs = new List<ParsedLog>() {parser.ParseRawLines(_logs[0]), parser.ParseRawLines(_logs[1]) };

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
