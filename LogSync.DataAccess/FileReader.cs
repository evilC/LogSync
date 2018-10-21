using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogSync.DataAccess
{
    public class FileReader : IFileReader
    {
        public List<string> GetFileRawLines(string path)
        {
            var lines = new List<string>();
            using (var lineData = new System.IO.StreamReader(path))
            {
                string line;
                while ((line = lineData.ReadLine()) != null)
                {
                    lines.Add(line);
                }
            }

            return lines;
        }
    }
}
