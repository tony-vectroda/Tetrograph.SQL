using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading;

namespace Tetrograph.Sql
{
    internal static class AppSettings
    {
        private static readonly string SettingsPath =
            Path.Combine(AppContext.BaseDirectory, "Tetrograph.Sql.settings.json");

        internal static SettingsModel Load()
        {
            if (!File.Exists(SettingsPath))
            {
           
                var defaultJson = JsonSerializer.Serialize(new SettingsModel(), new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsPath, defaultJson);
            }

            string json = File.ReadAllText(SettingsPath);
            return JsonSerializer.Deserialize<SettingsModel>(json);
        }
    }

    internal class SettingsModel
    {   
        public int  CurrentConnection  { get; set; } =0;
        public string Author { get; set; } = "..put your name here..";
        public string[] ConnectionString { get; set; } = [];
        public string formatter { get; set; } = "D:\\install\\CONSOLE_TOOLS\\SqlFormatter.1.6.10\\SqlFormatter.exe";
        public string ParametersSQL { get; set; }
        public string  HistoryFolder { get; set; }

        public string[] Ext { get; set; } = [];

    }

    internal class ExtensionsModel
    {
        public string Input { get; set; } = ".sql";
        public string Output { get; set; } = ".tg.sql";
    }
}
