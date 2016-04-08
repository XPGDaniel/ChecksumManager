using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace md5Verifier
{
    class Program
    {
        static void Main(string[] args)
        {
            int StartingPoint = 0, TotalLines = 0, Damaged = 0, OK = 0;
            string checksumfile = new FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location).Directory.FullName;
            string output = checksumfile + "\\output_" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
            List<FileStruct> lists = new List<FileStruct>();
            List<string> md5List = GetFiles(checksumfile, "*.md5");
            if (!File.Exists(output))
            {
                using (StreamWriter file = File.CreateText(output))
                {}
            }
            Console.WriteLine("No. of md5 : " + md5List.Count);
            if (args.Length == 1)
            {
                if (args[0].ToString().ToLowerInvariant().Contains(".md5"))
                {
                    StartingPoint = md5List.FindIndex(a => a.ToLowerInvariant() == args[0].ToString().ToLowerInvariant());
                }
                else
                {
                    StartingPoint = md5List.FindIndex(a => a.ToLowerInvariant() == args[0].ToString().ToLowerInvariant() + ".md5");
                }
            }
            for (int i = StartingPoint; i < md5List.Count; i++)
            {
                Console.WriteLine((md5List[i]));
                string[] lines = File.ReadAllLines(md5List[i]);
                foreach (string line in lines)
                {
                    if (!string.IsNullOrEmpty(line.Trim()))
                    {
                        TotalLines++;
                        FileStruct fs = new FileStruct();
                        fs.hash = line.Trim().Split('*')[0].Trim();
                        fs.filepath = new FileInfo(md5List[i]).Directory.FullName + "\\" + line.Trim().Split('*')[1].Trim();
                        lists.Add(fs);
                        //string pattern = @"\.\d{4}", replaced = "";
                        //if (Regex.Match(Path.GetFileNameWithoutExtension(fs.filepath), pattern).Captures.Count > 0)
                        //    replaced = Regex.Match(Path.GetFileNameWithoutExtension(fs.filepath), pattern).Captures[0].ToString();
                        //string result = !Path.GetExtension(fs.filepath).ToLowerInvariant().Contains("bak") && !Path.GetExtension(fs.filepath).ToLowerInvariant().Contains("dts") && !Path.GetExtension(fs.filepath).ToLowerInvariant().Contains("ac3") ? Regex.Split(Path.GetFileNameWithoutExtension(fs.filepath), pattern)[0] : Path.GetFileName(fs.filepath);
                        //string renewline = line.Trim().Split('*')[0] + "*" + line.Trim().Split('*')[1].Trim().Split('\\')[1];
                        //File.WriteAllText(result + replaced + ".md5", renewline);
                    }
                }
                if (lists.Any())
                {
                    using (System.IO.StreamWriter file = File.AppendText(output))
                    {
                        file.WriteLine(Convert.ToString(i + 1) + "/" + md5List.Count + "\t" + md5List[i].Split('\'')[md5List[i].Split('\'').Length -1] + " Verifying...");
                    }
                    foreach (FileStruct fss in lists)
                    {
                        if (fss.hash.ToLowerInvariant() == computeMD5(fss.filepath).ToLowerInvariant())
                        {
                            using (StreamWriter file = File.AppendText(output))
                            {
                                file.WriteLine("OK \t" + Path.GetFileName(fss.filepath));
                            }
                            OK++;
                        }
                        else
                        {
                            using (StreamWriter file = File.AppendText(output))
                            {
                                file.WriteLine("Damaged \t" + Path.GetFileName(fss.filepath));
                            }
                            Damaged++;
                        }
                    }
                    using (StreamWriter file = File.AppendText(output))
                    {
                        file.WriteLine("Verification Completed");
                        file.WriteLine("----------------------");
                    }
                    lists.Clear();
                }
                Console.WriteLine(Convert.ToString(i + 1) + "/" + md5List.Count + "\t" +  md5List[i].Split('\'')[md5List[i].Split('\'').Length -1] + " Checked.");
                //windows7ProgressBar.Value = ((i + 1) * 200 + md5List.Count) / (md5List.Count * 2);
                TaskbarProgress.SetValue(Process.GetCurrentProcess().MainWindowHandle, ((i + 1) * 200 + md5List.Count) / (md5List.Count * 2), 100);
                TaskbarProgress.SetState(Process.GetCurrentProcess().MainWindowHandle, TaskbarProgress.TaskbarStates.Normal);
            }
            using (StreamWriter file = File.AppendText(output))
            {
                file.WriteLine("Total files checked : " + TotalLines);
                file.WriteLine("Good : " + OK + ", Damaged : " + Damaged);
                file.WriteLine("Completed  @ " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            Console.WriteLine("Total files checked : " + TotalLines);
            Console.WriteLine("Good : " + OK + ", Damaged : " + Damaged);
            Console.WriteLine("Completed  @ " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            Console.ReadKey();
        }
        static private List<string> GetFiles(string path, string pattern)
        {
            var files = new List<string>();

            try
            {
                if (!path.Contains("$RECYCLE.BIN") && !path.Contains("#recycle"))
                {
                    files.AddRange(Directory.GetFiles(path, pattern, SearchOption.TopDirectoryOnly));
                    foreach (var directory in Directory.GetDirectories(path))
                        files.AddRange(GetFiles(directory, pattern));
                }
            }
            catch (UnauthorizedAccessException) { }

            return files;
        }
        static string computeMD5(string path)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(path))
                {
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();
                }
            }
        }
    }
}
