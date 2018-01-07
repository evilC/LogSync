using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace LogSync
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        MainWindow mainWindow;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            mainWindow = new MainWindow();
            var args = ProcessCommandLineArguments(e);

            foreach (var arg in args)
            {
                switch (arg.Key.ToLower())
                {
                    case "-loadlogs":
                        mainWindow.LoadLogs(arg.Value.ToArray());
                        break;
                    default:
                        var str = string.Format("Invalid Argument '{0}'", arg);
                        Console.WriteLine(str);
                        throw new Exception(str);
                }
            }

            mainWindow.Show();
        }

        private Dictionary<string, List<string>> ProcessCommandLineArguments(StartupEventArgs e)
        {
            var dict = new Dictionary<string, List<string>>();
            var max = e.Args.Length;

            int currentArg = 0;

            while (currentArg < max)
            {
                if (e.Args[currentArg][0] == '-')
                {
                    var subArgs = GetNonPrefixedArgs(e, currentArg + 1);
                    dict.Add(e.Args[currentArg], subArgs);
                    currentArg += 1 + subArgs.Count;
                }
                else
                {
                    var str = string.Format("Error: Not expecting non-argument parameter '{0}'", e.Args[currentArg]);
                    Console.WriteLine(str);
                    throw new Exception(str);
                }
            }
            return dict;
        }

        private List<string> GetNonPrefixedArgs(StartupEventArgs e, int offset)
        {
            var max = e.Args.Length;
            var chunks = new List<string>();
            for (int i = offset; i < max; i++)
            {
                if (e.Args[i][0] == '-')
                {
                    break;
                }
                chunks.Add(e.Args[i]);
            }
            return chunks;
        }
    }
}
