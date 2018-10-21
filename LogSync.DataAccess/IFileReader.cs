using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogSync.DataAccess
{
    interface IFileReader
    {
        List<string> GetFileRawLines(string path);
    }
}
