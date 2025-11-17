using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Tetrograph.Sql
{
    internal static class Startup
    {
        // Entry point for console app
      public  static SettingsModel Settings;
        public static int Main(string[] args)
        { 
            Settings = AppSettings.Load(); 
            try
            {
                if (args.Length < 2)
                {
                    Console.WriteLine("Usage: Tetrograph.Sql open <filePath>");
                    return 1;
                }

                string cmd = args[0];

                if (!string.Equals(cmd, "open", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"Unknown command: {cmd}");
                    return 2;
                }

              SQLHelper.OpenTestScript(args[1]);
                Console.WriteLine("OK");    
                return 0;
            }
            catch (Exception ex)
            { 
                Console.WriteLine("ERROR: " + ex.Message);
                return 99;
            }
        }
    }

}
