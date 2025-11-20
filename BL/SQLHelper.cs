using System;
using System.Diagnostics;
using System.Security.AccessControl;
using System.Text.Json;
using Cmd = Microsoft.Data.SqlClient.SqlCommand;
using Cn = Microsoft.Data.SqlClient.SqlConnection;

namespace Tetrograph.Sql
{
    internal static class SQLHelper
    {
        //table history
        internal static void TH(string originalFile)
        {
            string obj_name = getObjectName(originalFile);
            if (!(obj_name.Substring(0, 3) == "TD_" || obj_name.Substring(0, 3) == "TS_"))
            {
                Console.WriteLine(obj_name + " has to be a table,starting with TD_ or TS_!");
                return;
            }
               

            string sql = "create_th";
            using (var cn = new Cn(Startup.Settings.ConnectionString[Startup.Settings.CurrentConnection]))
            {
                cn.Open();
                string table_code;
                string read_proc_code;
                using (var cmd = new Cmd(sql, cn))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@oname", obj_name);
                    using (var reader = cmd.ExecuteReader())
                    {
                        reader.Read();
                        table_code = reader.GetString(0);
                        read_proc_code = reader.GetString(1);
                    }
                }
                string table_path_file =  Path.Combine(Startup.Settings.HistoryFolder, historyName(true, obj_name)+".sql");
                string proc_path_file =   Path.Combine(Startup.Settings.HistoryFolder, historyName(false, obj_name)+ ".sql");
                File.WriteAllText(table_path_file, table_code);
                File.WriteAllText(proc_path_file,  read_proc_code);
                Format(table_path_file );
                Format(proc_path_file);

                OpenFile(table_path_file);
                OpenFile(proc_path_file);
            }
        }
        internal static string historyName(bool is_table, string objectName)
        {
            string res;
            if (is_table)
                res = objectName.Replace("TD_", "TD_HISTORY_");
            else
                res = objectName.Replace("TD_", "Read_History_TD_");
            return res;
        }
        internal static string getObjectName(string originalFile)
        {
            string res;
            FileInfo fi = new FileInfo(originalFile);
            string object_name = fi.Name.Remove(fi.Name.Length - fi.Extension.Length);
            res = object_name;
            return res;
        }
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

        OpenFile(filePath);




        }
        internal static void OpenFile(string fullPath)
        {
            var psi = new ProcessStartInfo(fullPath)
            {
                WorkingDirectory = Path.GetDirectoryName(fullPath),
                UseShellExecute = true
            };

            Process.Start(psi);
        }
        internal static void Format(string fullPath)
        {
            if (Startup.Settings.formatter.Length > 1)
            {

                var psif = new ProcessStartInfo
                {
                    FileName = Startup.Settings.formatter,
                    Arguments = "\"" + fullPath + "\"",
                    WorkingDirectory = Path.GetDirectoryName(Startup.Settings.formatter),
                    UseShellExecute = false
                };
                Process.Start(psif);
            }
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
                cn.Close();
            }

            Format(originalFile);
        } }
    }
