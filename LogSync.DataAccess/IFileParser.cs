using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LogSync.Model;

namespace LogSync.DataAccess
{
    public interface IFileParser
    {
        ParsedLog ParseRawLines(List<string> rawLines);
    }
}
