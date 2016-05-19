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
        public static DateTime FilterDate = DateTime.MinValue;
        static void Main(string[] args)
        {
            int StartingPoint = 0, TotalLines = 0, Damaged = 0, OK = 0, Missing = 0;
            string checksumfile = new FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location).Directory.FullName;
            string output = checksumfile + "\\output_" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
            bool VerifyChecksums = false, CombineOnly = false, UseDateFilter = false; ;
            List<FileStruct> lists = new List<FileStruct>();
            StringBuilder builder = new StringBuilder();
            Console.WriteLine("Proccessing Mode");
            Console.WriteLine("A. Verify Checksums");
            Console.WriteLine("\t(You could input integer instead of A, initial index is 0.)");
            Console.WriteLine("\t(or input date format yyyy-MM-dd as Date filter, like " + DateTime.Today.ToString("yyyy-MM-dd") + " .)");
            Console.WriteLine("B. Verify File Existences Only");
            Console.WriteLine("C. Combine All md5s into One");
            Console.WriteLine("Select Proccessing Mode : ");
            try
            {
                string response = Console.ReadLine().ToLowerInvariant();
                int index = 0;
                if (response.Contains("-"))
                {
                    if (DateTime.TryParse(response, out FilterDate))
                    {
                        StartingPoint = 0;
                        VerifyChecksums = true;
                        UseDateFilter = true;
                    }
                }
                else if (int.TryParse(response, out index))
                {
                    StartingPoint = index;
                    VerifyChecksums = true;
                }
                else if (response.Length == 1)
                {
                    switch (response)
                    {
                        case "a":
                            StartingPoint = 0;
                            VerifyChecksums = true;
                            CombineOnly = false;
                            break;
                        case "b":
                            VerifyChecksums = false;
                            CombineOnly = false;
                            break;
                        case "c":
                            VerifyChecksums = false;
                            CombineOnly = true;
                            break;
                    }
                }
            }
            catch (Exception)
            {
                VerifyChecksums = false;
                UseDateFilter = false;
            }
            List<string> md5List = GetFiles(checksumfile, "*.md5", UseDateFilter);
            if (!File.Exists(output))
            {
                using (StreamWriter file = File.CreateText(output))
                { }
            }
            Console.WriteLine("No. of md5 : " + md5List.Count);
            //if (args.Length == 1)
            //{
            //    StartingPoint = Convert.ToInt32(args[0]);
            //if (args[0].ToString().ToLowerInvariant().Contains(".md5"))
            //{
            //    StartingPoint = md5List.FindIndex(a => a.ToLowerInvariant() == args[0].ToString().ToLowerInvariant());
            //}
            //else
            //{
            //    StartingPoint = md5List.FindIndex(a => a.ToLowerInvariant() == args[0].ToString().ToLowerInvariant() + ".md5");
            //}
            //}
            for (int i = StartingPoint; i < md5List.Count; i++)
            {
                Console.WriteLine((md5List[i]));
                string[] lines = File.ReadAllLines(md5List[i]);
                foreach (string line in lines)
                {
                    if (!string.IsNullOrEmpty(line.Trim()))
                    {
                        //if (!CombineOnly)
                        //{
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
                        //}
                        //else
                        //{
                        //    builder.Append(line.Trim()).AppendLine();
                        //}
                    }
                }
                if (lists.Any())
                {
                    if (!CombineOnly)
                    {
                        using (System.IO.StreamWriter file = File.AppendText(output))
                        {
                            file.WriteLine(Convert.ToString(i + 1) + "/" + md5List.Count + "\t" + md5List[i].Split('\'')[md5List[i].Split('\'').Length - 1] + " Proccessing...");
                        }
                        foreach (FileStruct fss in lists)
                        {
                            if (File.Exists(fss.filepath))
                            {
                                if (VerifyChecksums)
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
                                else
                                {
                                    using (StreamWriter file = File.AppendText(output))
                                    {
                                        file.WriteLine("Exist \t" + Path.GetFileName(fss.filepath));
                                    }
                                    OK++;
                                }
                            }
                            else
                            {
                                using (StreamWriter file = File.AppendText(output))
                                {
                                    file.WriteLine("Missing \t" + Path.GetFileName(fss.filepath));
                                }
                                Missing++;
                            }
                        }
                        using (StreamWriter file = File.AppendText(output))
                        {
                            file.WriteLine("Verification Completed");
                            file.WriteLine("----------------------");
                        }
                        Console.WriteLine(Convert.ToString(i + 1) + "/" + md5List.Count + "\t" + md5List[i].Split('\'')[md5List[i].Split('\'').Length - 1] + " Checked.");
                        TaskbarProgress.SetValue(Process.GetCurrentProcess().MainWindowHandle, ((i + 1) * 200 + md5List.Count) / (md5List.Count * 2), 100);
                        TaskbarProgress.SetState(Process.GetCurrentProcess().MainWindowHandle, TaskbarProgress.TaskbarStates.Normal);
                    }
                    else
                    {
                        //using (FileStream file = File.Create(output))
                        //{ }
                        foreach (FileStruct fss in lists)
                        {
                            builder.Append(fss.hash + " *" + fss.filepath.Substring(fss.filepath.IndexOf("\\") + 1)).AppendLine();
                        }
                    }
                    lists.Clear();
                }
            }
            if (!CombineOnly)
            {
                using (StreamWriter file = File.AppendText(output))
                {
                    file.WriteLine("Total files checked : " + TotalLines);
                    file.WriteLine("Good : " + OK + ", Damaged : " + Damaged + ", Missing : " + Missing);
                    file.WriteLine("Completed  @ " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                }
                Console.WriteLine("Total files checked : " + TotalLines);
                Console.WriteLine("Good : " + OK + ", Damaged : " + Damaged + ", Missing : " + Missing);
                Console.WriteLine("Completed  @ " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            else
            {
                if (builder.Length > 0)
                {
                    using (TextWriter writer = File.CreateText(output))
                    {
                        writer.Write(builder.ToString());
                    }
                    builder.Clear();
                }
            }
            Console.ReadKey();
        }
        static private List<string> GetFiles(string path, string pattern, bool UseDateFilter)
        {
            var files = new List<string>();

            try
            {
                if (!path.Contains("$RECYCLE.BIN") && !path.Contains("#recycle"))
                {
                    if (UseDateFilter)
                    {
                        string[] candidates = Directory.GetFiles(path, pattern, SearchOption.TopDirectoryOnly);
                        foreach (string c in candidates)
                        {
                            FileInfo fi = new FileInfo(c);
                            if (Convert.ToDateTime(fi.LastWriteTime.ToString("yyyy-MM-dd")) >= FilterDate)
                            {
                                files.Add(c);
                            }
                        }
                    }
                    else
                    {
                        files.AddRange(Directory.GetFiles(path, pattern, SearchOption.TopDirectoryOnly));
                    }
                    foreach (var directory in Directory.GetDirectories(path))
                        files.AddRange(GetFiles(directory, pattern, UseDateFilter));
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
