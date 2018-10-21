using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TFSChangeHistory
{
    static class Program
    {
        [DllImport("kernel32.dll")]
        static extern bool AttachConsole(int dwProcessId);
        private const int ATTACH_PARENT_PROCESS = -1;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args == null || args.Contains("-c") == false)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
            }
            else
            {
                AttachConsole(ATTACH_PARENT_PROCESS);
                HandleCLIMode(args);
            }

        }

        private static void HandleCLIMode(string[] args)
        {
            try
            {
                var request = ParseArgs(args);
                var response = ChangesetManager.GetChangesetHistory(request);
                Console.WriteLine("Comments | Author | Check-In Date | Changeset ID");
                foreach (var changeset in response)
                {
                    Console.WriteLine("{0} | {1} | {2} | {3}", changeset.Comment, changeset.Owner, changeset.CheckInDateTime, changeset.ChangesetId);
                }
            }
            catch (ArgumentException aex)
            {
                PrintUsage();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unknown error");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        private static ChangesetHistoryRequest ParseArgs(string[] args)
        {
            ChangesetHistoryRequest request = new ChangesetHistoryRequest();

            for (int index = 0; index < args.Length; index++)
            {
                switch (args[index])
                {
                    case "-t":
                        index++;
                        if (args[index].Trim().StartsWith("-"))
                        {
                            throw new ArgumentException("Value for argument -t is missing");
                        }
                        request.TFSUrl = args[index];
                        break;
                    case "-r":
                        index++;
                        if (args[index].Trim().StartsWith("-"))
                        {
                            throw new ArgumentException("Value for argument -r is missing");
                        }
                        request.ReleaseBranchUrl = args[index];
                        break;
                    case "-from":
                        index++;
                        if (args[index].Trim().StartsWith("-"))
                        {
                            throw new ArgumentException("Value for argument -from is missing");
                        }
                        if (DateTime.TryParse(args[index], out DateTime fromDate))
                        {
                            request.FromDate = fromDate;
                        }
                        else
                        {
                            throw new ArgumentException("Value for argument -from is invalid");
                        }
                        break;
                    case "-to":
                        index++;
                        if (args[index].Trim().StartsWith("-"))
                        {
                            throw new ArgumentException("Value for argument -to is missing");
                        }
                        if (DateTime.TryParse(args[index], out DateTime toDate))
                        {
                            request.ToDate = toDate;
                        }
                        else
                        {
                            throw new ArgumentException("Value for argument -from is invalid");
                        }
                        break;
                    case "-i":
                        index++;
                        if (args[index].Trim().StartsWith("-"))
                        {
                            throw new ArgumentException("Value for argument -i is missing");
                        }
                        request.IgnoreFromUsersString = args[index];
                        break;
                }
            }

            return request;

        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage");
            Console.WriteLine("UI: TFSChangeHistory.exe");
            Console.WriteLine("CLI: TFSChangeHistory.exe -c -t <TFS URL> -r <Release branch URL> -from <Check-in from date> -to <Check-in to date> -i <usernames to ignore delimited by comma> ");

        }

    }
}
