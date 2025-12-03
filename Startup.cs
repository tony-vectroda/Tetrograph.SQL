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
                    Console.WriteLine("Usage: Tetrograph.Sql open <filePath>, alter  <filePath>");
                    return 1;
                }
                string cmd = args[0];              
                switch(cmd)
                {
                    case "open":
                        SQLHelper.OpenTestScript(args[1]);
                        break;
                    case "alter":
                        SQLHelper.UpdateProc(args[1]);
                        break;
                    case "th":
                        SQLHelper.TH(args[1]);
                        break;
                    case "gen_open":
                        SQLHelper.gen_open(args[1]);
                        break;
                    case "import_fs":
                        SQLHelper.import_fs(args[1]);
                        break;
                } 
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
