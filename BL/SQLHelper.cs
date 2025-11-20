using System;
using System.Diagnostics;
using System.Security.AccessControl;
using Cmd = Microsoft.Data.SqlClient.SqlCommand;
using Cn = Microsoft.Data.SqlClient.SqlConnection;

namespace Tetrograph.Sql
{
    internal static class SQLHelper
    {

        internal static void OpenTestScript(string originalFile)
        {
            FileInfo fi = new FileInfo(originalFile);
            string object_name = fi.Name.Remove(fi.Name.Length - fi.Extension.Length);
            const string sql = @"select type from sys.objects where name = @object_name";
            string drop_code = null;
            string object_type;
            string sql_exec = null;
            Console.WriteLine(object_name);
            string filePath = fi.DirectoryName + @"\_" + object_name + ".sql";//.Trim('"').Replace(".sql", ".ttg");
            bool save_file = false;
            using (var cn = new Cn(Startup.Settings.ConnectionString[Startup.Settings.CurrentConnection]))
            {
                cn.Open();
                using (var cmd = new Cmd(sql, cn))
                {
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.Parameters.AddWithValue("@object_name", object_name);
                    object_type = ((string)cmd.ExecuteScalar()).TrimEnd();
                }
                string keyword = null;
                switch (object_type)
                {
                    case "P":
                        keyword = "PROCEDURE";
                        break;
                    case "FN":
                        keyword = "FUNCTION";
                        break;
                    case "V":
                        keyword = "VIEW";
                        break;
                }
                save_file = !File.Exists(filePath);
                if (!save_file)
                    save_file = File.ReadAllText(filePath).Length == 0;

                Console.WriteLine(save_file.ToString() + "-" + filePath);

                if (save_file)
                {
                    using (var cmd = new Cmd(Startup.Settings.ParametersSQL, cn))
                    {
                        cmd.CommandType = System.Data.CommandType.Text;
                        cmd.Parameters.AddWithValue("@object_name", object_name);
                        object obj = cmd.ExecuteScalar();
                        if (obj is DBNull)
                            switch (object_type)
                            {
                                case "V":
                                    sql_exec = $"Select top 10 * from {object_name};";
                                    break;

                                case "P":
                                    keyword = $"Exec dbo.{object_name};";
                                    break;
                                case "FN":
                                    keyword = $"Select dbo.{object_name}();";
                                    break;
                            }
                        else
                            sql_exec = (string)obj + ";";//
                    }
                    File.WriteAllText(filePath, sql_exec);

                }
                cn.Close();
            }
            ;

            var psi = new ProcessStartInfo(filePath)
            {
                WorkingDirectory = Path.GetDirectoryName(originalFile),
                UseShellExecute = true
            };

            Process.Start(psi);




        }
        internal static void UpdateProc(string originalFile)
        {
            FileInfo fi = new FileInfo(originalFile);
            string object_name = fi.Name.Remove(fi.Name.Length - fi.Extension.Length);
            const string sql = @"select type from sys.objects where name = @object_name";
            string drop_code = null;
            string object_type;
            string sql_exec = null;
            Console.WriteLine(object_name);
            string filePath = fi.DirectoryName + @"\_" + object_name + ".sql";//.Trim('"').Replace(".sql", ".ttg");
            bool save_file = false;
            using (var cn = new Cn(Startup.Settings.ConnectionString[Startup.Settings.CurrentConnection]))
            {
                cn.Open();
                using (var cmd = new Cmd(sql, cn))
                {
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.Parameters.AddWithValue("@object_name", object_name);
                    object_type = ((string)cmd.ExecuteScalar()).TrimEnd();
                }
                string keyword = null;
                switch (object_type)
                {
                    case "P":
                        keyword = "PROCEDURE";
                        break;
                    case "FN":
                        keyword = "FUNCTION";
                        break;
                    case "V":
                        keyword = "VIEW";
                        break;
                }
                drop_code = $"if  exists(select * from  sys.objects where name ='{object_name}' and type='{object_type}') DROP {keyword} {object_name};";
                using (var cmd = new Cmd(drop_code, cn))
                {
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.ExecuteNonQuery();
                }

                string create_object = File.ReadAllText(originalFile);

                using (var cmd = new Cmd(create_object, cn))
                {
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.ExecuteNonQuery();
                }

                save_file = !File.Exists(filePath);
                if (!save_file)
                    save_file = File.ReadAllText(filePath).Length == 0;



                if (save_file)
                {
                    //string par_names = "DECLARE @res VARCHAR(max);\r\n\r\nSELECT @res = 'Declare ' + CHAR(13) + STRING_AGG(pr.name + ' ' + CASE \r\n\t\t\tWHEN t.name IN (\r\n\t\t\t\t\t'varchar'\r\n\t\t\t\t\t,'nvarchar'\r\n\t\t\t\t\t,'char'\r\n\t\t\t\t\t,'nchar'\r\n\t\t\t\t\t,'binary'\r\n\t\t\t\t\t,'varbinary'\r\n\t\t\t\t\t)\r\n\t\t\t\tTHEN t.name + '(' + CASE \r\n\t\t\t\t\t\tWHEN pr.max_length = - 1\r\n\t\t\t\t\t\t\tTHEN 'MAX'\r\n\t\t\t\t\t\tWHEN t.name LIKE 'n%'\r\n\t\t\t\t\t\t\tTHEN CAST(pr.max_length / 2 AS VARCHAR(10))\r\n\t\t\t\t\t\tELSE CAST(pr.max_length AS VARCHAR(10))\r\n\t\t\t\t\t\tEND + ')'\r\n\t\t\tWHEN t.name IN (\r\n\t\t\t\t\t'decimal'\r\n\t\t\t\t\t,'numeric'\r\n\t\t\t\t\t)\r\n\t\t\t\tTHEN t.name + '(' + CAST(pr.precision AS VARCHAR(10)) + ',' + CAST(pr.scale AS VARCHAR(10)) + ')'\r\n\t\t\tELSE t.name\r\n\t\t\tEND + '=' + CASE \r\n\t\t\tWHEN t.name IN (\r\n\t\t\t\t\t'varchar'\r\n\t\t\t\t\t,'nvarchar'\r\n\t\t\t\t\t,'char'\r\n\t\t\t\t\t,'nchar'\r\n\t\t\t\t\t)\r\n\t\t\t\tTHEN '''' + '2' + ''''\r\n\t\t\tELSE '2'\r\n\t\t\tEND, ',' + CHAR(13)) + CHAR(13) + CHAR(13) + 'exec dbo.' + @object_name + ' ' + CHAR(13) + STRING_AGG(pr.name, ',' + CHAR(13))\r\nFROM sys.objects o\r\nINNER JOIN sys.parameters pr ON o.object_id = pr.object_id\r\nINNER JOIN sys.types t ON pr.user_type_id = t.user_type_id\r\nWHERE o.type IN (\r\n\t\t'P'\r\n\t\t,'FN'\r\n\t\t,'IF'\r\n\t\t,'TF'\r\n\t\t)\r\n\tAND o.name = @object_name;\r\n\r\nSELECT @res;";
                    //Console.WriteLine(par_names);
                    using (var cmd = new Cmd(Startup.Settings.ParametersSQL, cn))
                    {
                        cmd.CommandType = System.Data.CommandType.Text;
                        cmd.Parameters.AddWithValue("@object_name", object_name);
                        object obj = cmd.ExecuteScalar();
                        if (obj is DBNull)
                            switch (object_type)
                            {
                                case "V":
                                    sql_exec = $"Select top 10 * from {object_name};";
                                    break;

                                case "P":
                                    keyword = $"Exec dbo.{object_name};";
                                    break;
                                case "FN":
                                    keyword = $"Select dbo.{object_name}();";
                                    break;
                            }
                        else
                            sql_exec = (string)obj + ";";//
                    }

                }
                cn.Close();
            }





         ;



            if (Startup.Settings.formatter.Length > 1)
            {

                var psif = new ProcessStartInfo
                {
                    FileName = Startup.Settings.formatter,
                    Arguments = originalFile,
                    WorkingDirectory = Path.GetDirectoryName(Startup.Settings.formatter),
                    UseShellExecute = false
                };
                Process.Start(psif);

            }

            if (Startup.Settings.formatter.Length > 1)
            {

                var psif = new ProcessStartInfo
                {
                    FileName = Startup.Settings.formatter,
                    Arguments = originalFile,
                    WorkingDirectory = Path.GetDirectoryName(Startup.Settings.formatter),
                    UseShellExecute = false
                };
                Process.Start(psif);



                if (save_file)
                {
                    string content = $"/*{Environment.NewLine}--------------------------------------------------------------------- {Environment.NewLine}Testing script of {object_name} created{Environment.NewLine}at:{DateTime.Now.ToString()}{Environment.NewLine}" +
                        $"Author:{Startup.Settings.Author}{Environment.NewLine}" +
                        $"---------------------------------------------------------------------{Environment.NewLine}*/" + Environment.NewLine +
                       sql_exec;
                    File.WriteAllText(filePath, content);
                }
                if (false)
                {
                    var psi = new ProcessStartInfo();
                    psi.FileName = @"C:\Program Files (x86)\Microsoft SQL Server Management Studio 19\Common7\IDE\Ssms.exe";
                    psi.WorkingDirectory = "C:\\Program Files (x86)\\Microsoft SQL Server Management Studio 19\\Common7\\IDE\\";
                    psi.Arguments = $"-nosplash \"{filePath}\"";
                    psi.UseShellExecute = true;
                    Process.Start(psi);
                }

                //   else
                //   Clipboard.SetText(filePath);
                /*
                var psi = new ProcessStartInfo(filePath)
                {
                    UseShellExecute = true
                };

                Process.Start(psi);
                */
            }
        }
    }
}//fixed 20/11