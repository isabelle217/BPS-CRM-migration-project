using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheUltimateIktissadMigrationTool
{
    public class Logger
    {
        static List<string> queue = new List<string>();
        public static void Purge()
        {
            if (System.IO.File.Exists(@"c:\log\migrate.txt"))
                System.IO.File.Delete(@"c:\log\migrate.txt");

            System.IO.File.Create(@"c:\log\migrate.txt");

         
        }
        public static void Log(string message)
        {
            try
            {
                queue.Add(message + "\r\n");
                if (queue.Count() > 100)
                {
                    System.IO.File.AppendAllLines(@"c:\log\migrate.txt", queue);
                    queue.Clear();
                }

            }
            catch (Exception)
            {

            }
        }

        public static void End()
        {
            System.IO.File.AppendAllLines(@"c:\log\migrate.txt", queue);
            queue.Clear();
        }
    }
}
