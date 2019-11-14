using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Wox.Plugin;

namespace WoxPluginKillPort
{
    public class Main : IPlugin
    {
        public void Init(PluginInitContext context) { }
        public static bool IsInt(string value)
        {
            return Regex.IsMatch(value, @"^[+-]?\d*$");
        }
        public static string getPidToName(int pid) 
        {
            Process[] ps = Process.GetProcesses();
            foreach (Process p in ps)
            {
                if (p.Id == pid) 
                {
                    return p.ProcessName;
                }
            }
            return "";
        }
        public static void killPid(int pid) 
        {
            Process[] pro = Process.GetProcesses();
            foreach (Process p in pro)
            {
                if (p.Id == pid)
                {
                    p.Kill();
                }
            }
        }
        public List<Result> Query(Query query)
        {
            List<Result> results = new List<Result>();
            string port = query.Search;
            if (!IsInt(port))
            {
                return results;
            }
            Process pro = new Process();
            // 设置命令行、参数
            pro.StartInfo.FileName = "cmd.exe";
            pro.StartInfo.UseShellExecute = false;
            pro.StartInfo.RedirectStandardInput = true;
            pro.StartInfo.RedirectStandardOutput = true;
            pro.StartInfo.RedirectStandardError = true;
            pro.StartInfo.CreateNoWindow = true;
            // 启动CMD
            pro.Start();
            // 运行端口检查命令
            pro.StandardInput.WriteLine("netstat -ano");
            pro.StandardInput.WriteLine("exit");
            // 获取结果
            Regex reg = new Regex("\\s+", RegexOptions.Compiled);
            string line = null;
            while ((line = pro.StandardOutput.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.StartsWith("TCP", StringComparison.OrdinalIgnoreCase))
                {
                    line = reg.Replace(line, ",");
                    string[] arr = line.Split(',');
                    if (arr[1].EndsWith(":"+port))
                    {
                        int pid = Int32.Parse(arr[4]);
                        results.Add(new Result()
                        {
                            Title = "占用进程["+ getPidToName(pid)+ "]",
                            SubTitle = "进程ID["+pid+"]",
                            Action = e =>
                            {
                                killPid(pid);
                                return true;
                            }
                        });
                    }
                }
            }

            
            return results;
        }
    }
}
