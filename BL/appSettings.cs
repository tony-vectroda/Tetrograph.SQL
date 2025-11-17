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

        public  string Author { get; set; } =  "..put your name here..";
        public int  CurrentConnection  { get; set; } =0;
        public string[] ConnectionString { get; set; } = [];
        public string formatter { get; set; } = "D:\\install\\CONSOLE_TOOLS\\SqlFormatter.1.6.10\\SqlFormatter.exe";

        public string ParametersSQL { get; set; } //  = "DECLARE @res VARCHAR(max);\r\n\r\nSELECT @res = 'Declare ' + CHAR(13) + STRING_AGG(pr.name + ' ' + CASE \r\n\t\t\tWHEN t.name IN (\r\n\t\t\t\t\t'varchar'\r\n\t\t\t\t\t,'nvarchar'\r\n\t\t\t\t\t,'char'\r\n\t\t\t\t\t,'nchar'\r\n\t\t\t\t\t,'binary'\r\n\t\t\t\t\t,'varbinary'\r\n\t\t\t\t\t)\r\n\t\t\t\tTHEN t.name + '(' + CASE \r\n\t\t\t\t\t\tWHEN pr.max_length = - 1\r\n\t\t\t\t\t\t\tTHEN 'MAX'\r\n\t\t\t\t\t\tWHEN t.name LIKE 'n%'\r\n\t\t\t\t\t\t\tTHEN CAST(pr.max_length / 2 AS VARCHAR(10))\r\n\t\t\t\t\t\tELSE CAST(pr.max_length AS VARCHAR(10))\r\n\t\t\t\t\t\tEND + ')'\r\n\t\t\tWHEN t.name IN (\r\n\t\t\t\t\t'decimal'\r\n\t\t\t\t\t,'numeric'\r\n\t\t\t\t\t)\r\n\t\t\t\tTHEN t.name + '(' + CAST(pr.precision AS VARCHAR(10)) + ',' + CAST(pr.scale AS VARCHAR(10)) + ')'\r\n\t\t\tELSE t.name\r\n\t\t\tEND + '=' + CASE \r\n\t\t\tWHEN t.name IN (\r\n\t\t\t\t\t'varchar'\r\n\t\t\t\t\t,'nvarchar'\r\n\t\t\t\t\t,'char'\r\n\t\t\t\t\t,'nchar'\r\n\t\t\t\t\t)\r\n\t\t\t\tTHEN '''' + '2' + ''''\r\n\t\t\tELSE '2'\r\n\t\t\tEND, ',' + CHAR(13)) + CHAR(13) + CHAR(13) + 'exec dbo.' + @object_name + ' ' + CHAR(13) + STRING_AGG(pr.name, ',' + CHAR(13))\r\nFROM sys.objects o\r\nINNER JOIN sys.parameters pr ON o.object_id = pr.object_id\r\nINNER JOIN sys.types t ON pr.user_type_id = t.user_type_id\r\nWHERE o.type IN (\r\n\t\t'P'\r\n\t\t,'FN'\r\n\t\t,'IF'\r\n\t\t,'TF'\r\n\t\t)\r\n\tAND o.name = @object_name;\r\n\r\nSELECT @res;";

       // public string DropSQL { get; set; } //= "DECLARE @res VARCHAR(max);\r\n\r\nSELECT @res = 'Declare ' + CHAR(13) + STRING_AGG(pr.name + ' ' + CASE \r\n\t\t\tWHEN t.name IN (\r\n\t\t\t\t\t'varchar'\r\n\t\t\t\t\t,'nvarchar'\r\n\t\t\t\t\t,'char'\r\n\t\t\t\t\t,'nchar'\r\n\t\t\t\t\t,'binary'\r\n\t\t\t\t\t,'varbinary'\r\n\t\t\t\t\t)\r\n\t\t\t\tTHEN t.name + '(' + CASE \r\n\t\t\t\t\t\tWHEN pr.max_length = - 1\r\n\t\t\t\t\t\t\tTHEN 'MAX'\r\n\t\t\t\t\t\tWHEN t.name LIKE 'n%'\r\n\t\t\t\t\t\t\tTHEN CAST(pr.max_length / 2 AS VARCHAR(10))\r\n\t\t\t\t\t\tELSE CAST(pr.max_length AS VARCHAR(10))\r\n\t\t\t\t\t\tEND + ')'\r\n\t\t\tWHEN t.name IN (\r\n\t\t\t\t\t'decimal'\r\n\t\t\t\t\t,'numeric'\r\n\t\t\t\t\t)\r\n\t\t\t\tTHEN t.name + '(' + CAST(pr.precision AS VARCHAR(10)) + ',' + CAST(pr.scale AS VARCHAR(10)) + ')'\r\n\t\t\tELSE t.name\r\n\t\t\tEND + '=' + CASE \r\n\t\t\tWHEN t.name IN (\r\n\t\t\t\t\t'varchar'\r\n\t\t\t\t\t,'nvarchar'\r\n\t\t\t\t\t,'char'\r\n\t\t\t\t\t,'nchar'\r\n\t\t\t\t\t)\r\n\t\t\t\tTHEN '''' + '2' + ''''\r\n\t\t\tELSE '2'\r\n\t\t\tEND, ',' + CHAR(13)) + CHAR(13) + CHAR(13) + 'exec dbo.' + @object_name + ' ' + CHAR(13) + STRING_AGG(pr.name, ',' + CHAR(13))\r\nFROM sys.objects o\r\nINNER JOIN sys.parameters pr ON o.object_id = pr.object_id\r\nINNER JOIN sys.types t ON pr.user_type_id = t.user_type_id\r\nWHERE o.type IN (\r\n\t\t'P'\r\n\t\t,'FN'\r\n\t\t,'IF'\r\n\t\t,'TF'\r\n\t\t)\r\n\tAND o.name = @object_name;\r\n\r\nSELECT @res;";




        public ExtensionsModel Extensions { get; set; } = new ExtensionsModel();
    }

    internal class ExtensionsModel
    {
        public string Input { get; set; } = ".sql";
        public string Output { get; set; } = ".tg.sql";
    }
}
