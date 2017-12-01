using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Diagnostics;

namespace md5Verifier
{
    class Program
    {
        public static DateTime FilterDate = DateTime.MinValue;
        public static bool VerifyChecksums = false, CombineOnly = false, UseDateFilter = false;
        private static string checksumfile = new FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location).Directory.FullName;
        public static string output = "";

        public static string Checksumfile { get => checksumfile; set => checksumfile = value; }

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.Unicode;
            //string reallyLongDirectory = @"C:\Users\Daniel\Source\Repos\FolderRenameAssist\FolderRenameAssist\bin\Debug\New folder\[Ano Hi Mita Hana no Namae o Bokutachi wa Mada Shiranai][あの日見た花の名前を僕達はまだ知らない.][ANK-Raws] 劇場版 あの日見た花の名前を僕達はまだ知らない。 (BDrip x264 FLAC DTS TRUE-HD 5.1ch SUP Hi10P)\New Text Document.txt";
            //string reallyLongFile = @"C:\L\[Fullmetal Alchemist][Hagane no Renkinjutsushi][鋼の錬金術師][Kuro-Raws] Fullmetal Alchemist - The Sacred Star of Milos (BDRip 1080p H.264-Hi10P FLACx2)\[Kuro-Raws] Fullmetal Alchemist - The Sacred Star of Milos (BDRip 1080p H.264-Hi10P FLACx2 DTSx3) [21A184A1].mkv";
            //Console.WriteLine($"Creating a directory that is {reallyLongDirectory.Length} characters long");
            //Directory.CreateDirectory(reallyLongDirectory);

            //Console.WriteLine(reallyLongFile);
            //Console.WriteLine(File.Exists(reallyLongFile));
            int StartingPoint = 0;
            Console.WriteLine("Proccessing Mode");
            Console.WriteLine("A. Verify Checksums");
            Console.WriteLine("\t(You could input integer instead of A, initial index is 0.)");
            Console.WriteLine("\t(or input date format yyyy-MM-dd as Date filter, like " + DateTime.Today.ToString("yyyy-MM-dd") + " .)");
            Console.WriteLine("B. Verify File Existences and Path Length");
            Console.WriteLine("C. Combine All md5s into One");
            Console.WriteLine("D. Create Checksum for every folder.");
            Console.WriteLine("\t(append input date format yyyy-MM-dd as Date filter, like d " + DateTime.Today.ToString("yyyy-MM-dd") + " .)");
            Console.WriteLine("Select Proccessing Mode : ");
            try
            {
                output = Checksumfile + "\\output_" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
                string response = Console.ReadLine().ToLowerInvariant();
                int index = 0;
                if (response.Contains("-") && char.IsDigit(response[0]))
                {
                    if (DateTime.TryParse(response, out FilterDate))
                    {
                        StartingPoint = 0;
                        VerifyChecksums = true;
                        UseDateFilter = true;
                        output = Checksumfile + "\\output_partial_" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
                    }
                    else
                    {
                        Console.WriteLine("Invalid Date, Program Terminated.");
                        Console.ReadKey();
                        return;
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
                        case "d":
                            VerifyChecksums = false;
                            CombineOnly = false;
                            break;
                    }
                }
                else if (response.Length > 1 && response.ToLowerInvariant().StartsWith("d"))
                {
                    if (DateTime.TryParse(response.Split(' ')[1], out FilterDate))
                    {
                        StartingPoint = 0;
                        UseDateFilter = true;
                        output = Checksumfile + "\\output_partial_" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
                    }
                    else
                    {
                        Console.WriteLine("Invalid Date, Program Terminated.");
                        Console.ReadKey();
                        return;
                    }
                }
                switch (response.ToLowerInvariant()[0])
                {
                    case 'd':
                        Exp(0);
                        break;
                    case 'b':
                        VerifyPath();
                        break;
                    //case 'd':
                    //    Create();
                    //    break;
                    default:
                    case 'a':
                    case 'c':
                        Verify(StartingPoint);
                        break;
                }
            }
            catch (Exception ex)
            {
                VerifyChecksums = false;
                UseDateFilter = false;
                Console.WriteLine(ex.ToString());
            }
            Console.ReadKey();
        }
        static void Verify(int StartingPoint)
        {
            DateTime PStart = DateTime.Now;
            int TotalLines = 0, Damaged = 0, OK = 0, Missing = 0;
            List<FileStruct> lists = new List<FileStruct>();
            StringBuilder builder = new StringBuilder();
            List<string> md5List = GetFiles(Checksumfile, "*.md5", UseDateFilter);
            if (!File.Exists(output))
            {
                using (StreamWriter file = File.CreateText(output))
                {
                    StringBuilder md5listbuilder = new StringBuilder();
                    foreach (var md5 in md5List)
                    {
                        md5listbuilder.Append(md5).AppendLine();
                    }
                    file.Write(md5listbuilder.ToString());
                    md5listbuilder.Clear();
                }
            }
            Console.WriteLine("No. of md5 : " + md5List.Count);
            for (int i = StartingPoint; i < md5List.Count; i++)
            {
                string[] lines = File.ReadAllLines(md5List[i]);
                foreach (string line in lines)
                {
                    if (!string.IsNullOrEmpty(line.Trim()))
                    {
                        TotalLines++;
                        FileStruct fs = new FileStruct()
                        {
                            hash = line.Trim().Split('*')[0].Trim()
                            //filepath = new FileInfo(md5List[i]).Directory.FullName + "\\" + line.Trim().Split('*')[1].Trim()
                        };
                        if (line.Contains("*"))
                        {
                            fs.filepath = new FileInfo(md5List[i]).Directory.FullName + "\\" + line.Trim().Split('*')[1].Trim();
                        }
                        else
                        {
                            fs.filepath = new FileInfo(md5List[i]).Directory.FullName + "\\" + line.Trim().Substring(33).Trim();
                        }

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
                                    if (fss.hash.ToLowerInvariant() == ComputeMD5(fss.filepath).ToLowerInvariant())
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
                TimeSpan ts = DateTime.Now.Subtract(PStart);
                using (StreamWriter file = File.AppendText(output))
                {
                    file.WriteLine("Total files checked : " + TotalLines);
                    file.WriteLine("Good : " + OK + ", Damaged : " + Damaged + ", Missing : " + Missing);
                    file.WriteLine("Completed  @ " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    file.WriteLine("Time Elapse : " + ts.ToString());
                }
                Console.WriteLine("Total files checked : " + TotalLines);
                Console.WriteLine("Good : " + OK + ", Damaged : " + Damaged + ", Missing : " + Missing);
                Console.WriteLine("Completed  @ " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                Console.WriteLine("Time Elapse : " + ts.ToString());
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
                Console.WriteLine("Completed!");
            }
        }
        static void Exp(int StartingPoint)
        {
            DateTime PStart = DateTime.Now;
            int TotalLines = 0, Damaged = 0, OK = 0, Missing = 0;
            List<FileCheckStat> lists = new List<FileCheckStat>();
            List<FileCheckStat> DamagedList = new List<FileCheckStat>();
            List<FileCheckStat> MissingList = new List<FileCheckStat>();
            StringBuilder builder = new StringBuilder();
            StringBuilder dmgbuilder = new StringBuilder();
            StringBuilder missingbuilder = new StringBuilder();
            List<string> folderList = Getfolders(Checksumfile).ToList();
            List<string> md5List = GetFiles(Checksumfile, "*.md5", UseDateFilter);
            string log = output;
            if (!File.Exists(log))
            {
                using (StreamWriter file = File.CreateText(output))
                {
                }
            }
            Console.WriteLine("No. of md5 : " + md5List.Count);
            Console.WriteLine("No. of folders : " + folderList.Count);
            for (int i = StartingPoint; i < folderList.Count; i++)
            {
                string md5f = md5List.FirstOrDefault(s => Path.GetFileNameWithoutExtension(s) == Path.GetFileName(folderList[i]));
                if (!string.IsNullOrEmpty(md5f))
                {
                    string[] lines = File.ReadAllLines(md5f);
                    List<FileCheckStat> FileStatslist = new List<FileCheckStat>();
                    foreach (string line in lines)
                    {
                        if (!string.IsNullOrEmpty(line.Trim()))
                        {
                            FileCheckStat fs = new FileCheckStat()
                            {
                                hash = line.Trim().Split('*')[0].Trim()
                            };
                            if (line.Contains("*"))
                            {
                                fs.filepath = new FileInfo(md5f).Directory.FullName + "\\" + line.Trim().Split('*')[1].Trim();
                            }
                            else
                            {
                                fs.filepath = new FileInfo(md5f).Directory.FullName + "\\" + line.Trim().Substring(33).Trim();
                            }
                            fs.IsChecksummed = 9;
                            lists.Add(fs);
                        }
                    }
                }
                foreach (string line in GetFiles(folderList[i], "*.*", false))
                {
                    try
                    {
                        string md5hash = ComputeMD5(line);
                        FileCheckStat fcs = lists.FirstOrDefault(s => s.hash == md5hash && s.filepath == line);
                        if (fcs != null)
                        {//old file checksummed
                            foreach (var file in lists.Where(s => s.hash == md5hash && s.filepath == line))
                            {
                                file.IsChecksummed = 1;
                            }
                        }
                        else
                        {
                            FileCheckStat targetfcs = lists.FirstOrDefault(s => s.hash != md5hash && s.filepath == line);
                            if (targetfcs != null)
                            {//different file with duplicate filename or file corrupted
                                targetfcs.IsChecksummed = 2;
                                DamagedList.Add(targetfcs);
                                lists.Remove(targetfcs);
                            }
                            //new file
                            FileCheckStat fs = new FileCheckStat()
                            {
                                hash = md5hash,
                                filepath = line,
                                IsChecksummed = 0
                            };
                            lists.Add(fs);
                        }
                    }
                    catch (UnauthorizedAccessException uaex)
                    {
                        Console.WriteLine(uaex.Message);
                        using (StreamWriter file = File.AppendText(log))
                        {
                            file.WriteLine(uaex.Message);
                        }
                    }
                }
                if (lists.Any())
                {
                    foreach (FileCheckStat fss in lists) //.Where(s => s.IsChecksummed != 2 && s.IsChecksummed != 9)
                    {
                        fss.filepath = fss.filepath.Replace(folderList[i] + "\\", "");
                    }
                    OK += lists.Count;
                    lists = Sort(lists);
                    foreach (FileCheckStat fss in lists.Where(s => s.IsChecksummed != 2 && s.IsChecksummed != 9))
                    {
                        builder.Append(fss.hash + " *" + fss.filepath).AppendLine();
                    }
                    output = Path.Combine(folderList[i], Path.GetFileName(folderList[i]) + ".md5");
                    using (StreamWriter writer = File.CreateText(output))
                    {
                        writer.Write(builder.ToString());
                    }
                    builder.Clear();
                }

                if (DamagedList.Any())
                {
                    foreach (FileCheckStat fss in DamagedList)
                    {
                        fss.filepath = fss.filepath.Replace(folderList[i] + "\\", "");
                    }
                    Damaged += DamagedList.Count;
                    foreach (FileCheckStat fss in DamagedList)
                    {
                        dmgbuilder.Append(fss.hash + " *" + fss.filepath).AppendLine();
                    }
                    output = Path.Combine(folderList[i], Path.GetFileName(folderList[i]) + "-damaged.md5");
                    using (StreamWriter writer = File.CreateText(output))
                    {
                        writer.Write(dmgbuilder.ToString());
                    }
                    dmgbuilder.Clear();
                    DamagedList.Clear();
                }
                MissingList = lists.Where(s => s.IsChecksummed == 9).ToList();
                if (MissingList.Any())
                {
                    Missing += MissingList.Count;
                    foreach (FileCheckStat fss in MissingList)
                    {
                        missingbuilder.Append(fss.hash + " *" + fss.filepath).AppendLine();
                    }
                    output = Path.Combine(folderList[i], Path.GetFileName(folderList[i]) + "-missing.md5");
                    using (StreamWriter writer = File.CreateText(output))
                    {
                        writer.Write(missingbuilder.ToString());
                    }
                    missingbuilder.Clear();
                    MissingList.Clear();
                }
                lists.Clear();

                TaskbarProgress.SetValue(Process.GetCurrentProcess().MainWindowHandle, ((i + 1) * 200 + folderList.Count) / (folderList.Count * 2), 100);
                TaskbarProgress.SetState(Process.GetCurrentProcess().MainWindowHandle, TaskbarProgress.TaskbarStates.Normal);
                TotalLines = i + 1;
                Console.WriteLine(Convert.ToString((i + 1)) + "/" + folderList.Count + "\t" + Path.GetFileName(folderList[i]) + ".md5 Generated.");
                using (StreamWriter file = File.AppendText(log))
                {
                    file.WriteLine(Convert.ToString((i + 1)) + "/" + folderList.Count + "\t" + Path.GetFileName(folderList[i]) + ".md5 Generated.");
                }
            }
            TimeSpan ts = DateTime.Now.Subtract(PStart);
            Console.WriteLine("Md5 checksume files generated : " + folderList.Count + ", Total files checksumed : " + OK);
            Console.WriteLine("Damaged files : " + Damaged + ", Files missing : " + Missing);
            Console.WriteLine("Completed  @ " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            Console.WriteLine("Time Elapse : " + ts.ToString());
            using (StreamWriter file = File.AppendText(log))
            {
                file.WriteLine("Md5 checksume files generated : " + folderList.Count + ", Total files checksumed : " + OK);
                file.WriteLine("Damaged files : " + Damaged + ", Files missing : " + Missing);
                file.WriteLine("Completed  @ " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                file.WriteLine("Time Elapse : " + ts.ToString());
            }
        }
        static void VerifyPath()
        {
            int Exist = 0, Toolong = 0, Missing = 0;
            List<string> fileList = GetFiles(Checksumfile, "*.*", UseDateFilter);
            if (!File.Exists(output))
            {
                using (StreamWriter file = File.CreateText(output))
                { }
            }
            Console.WriteLine("No. of files : " + fileList.Count);
            for (int i = 0; i < fileList.Count; i++)
            {
                if (File.Exists(fileList[i]))
                {
                    try
                    {
                        Path.GetFullPath(fileList[i]);
                        //using (StreamWriter file = File.AppendText(output))
                        //{
                        //    file.WriteLine("Exist \t" + fileList[i]);
                        //}
                        Exist++;
                    }
                    catch (PathTooLongException ptle)
                    {
                        using (StreamWriter file = File.AppendText(output))
                        {
                            file.WriteLine("Toolong \t" + fileList[i]);
                        }
                        Toolong++;
                    }
                }
                else
                {
                    using (StreamWriter file = File.AppendText(output))
                    {
                        file.WriteLine("Missing \t" + fileList[i]);
                    }
                    Missing++;
                }
                TaskbarProgress.SetValue(Process.GetCurrentProcess().MainWindowHandle, ((i + 1) * 200 + fileList.Count) / (fileList.Count * 2), 100);
                TaskbarProgress.SetState(Process.GetCurrentProcess().MainWindowHandle, TaskbarProgress.TaskbarStates.Normal);
            }
            using (StreamWriter file = File.AppendText(output))
            {
                file.WriteLine("Total files checked : " + fileList.Count);
                file.WriteLine("Exist : " + Exist + ", Toolong : " + Toolong + ", Missing : " + Missing);
                Console.WriteLine("Total files checked : " + fileList.Count);
                Console.WriteLine("Exist : " + Exist + ", Toolong : " + Toolong + ", Missing : " + Missing);
                file.WriteLine("Verification Completed");
                file.WriteLine("----------------------");
            }
            Console.WriteLine("Completed!");
        }
        //static void Create()
        //{
        //    DateTime PStart = DateTime.Now;
        //    int TotalLines = 0, OK = 0;
        //    List<string> folderList = Getfolders(Checksumfile).ToList();
        //    string log = output;
        //    for (int i = 0; i < folderList.Count; i++)
        //    {
        //        List<FileStruct> lists = new List<FileStruct>();
        //        StringBuilder builder = new StringBuilder();

        //        foreach (string line in GetFiles(folderList[i], "*.*", false))
        //        {
        //            try
        //            {
        //                FileStruct fs = new FileStruct()
        //                {
        //                    hash = ComputeMD5(line),
        //                    filepath = line.Replace(folderList[i] + "\\", "")
        //                };
        //                lists.Add(fs);
        //            }
        //            catch (UnauthorizedAccessException uaex)
        //            {
        //                Console.WriteLine(uaex.Message);
        //                using (StreamWriter file = File.AppendText(log))
        //                {
        //                    file.WriteLine(uaex.Message);
        //                }
        //            }
        //        }
        //        if (lists.Any())
        //        {
        //            foreach (FileStruct fss in lists)
        //            {
        //                builder.Append(fss.hash + " *" + fss.filepath).AppendLine();
        //                OK++;
        //            }
        //            lists.Clear();
        //        }

        //        if (builder.Length > 0)
        //        {
        //            bool avail = false;
        //            int retry = 1;
        //            output = Path.Combine(folderList[i], Path.GetFileNameWithoutExtension(folderList[i]) + ".md5");
        //            while (!avail)
        //            {
        //                if (!File.Exists(output))
        //                {
        //                    avail = true;
        //                }
        //                else
        //                {
        //                    output = Path.Combine(folderList[i], Path.GetFileNameWithoutExtension(folderList[i]) + retry.ToString() + ".md5");
        //                    retry++;
        //                }
        //            }
        //            using (TextWriter writer = File.CreateText(output))
        //            {
        //                writer.Write(builder.ToString());
        //            }
        //            builder.Clear();
        //        }
        //        TaskbarProgress.SetValue(Process.GetCurrentProcess().MainWindowHandle, ((i + 1) * 200 + folderList.Count) / (folderList.Count * 2), 100);
        //        TaskbarProgress.SetState(Process.GetCurrentProcess().MainWindowHandle, TaskbarProgress.TaskbarStates.Normal);
        //        TotalLines = i + 1;
        //        Console.WriteLine(Convert.ToString(TotalLines) + "/" + folderList.Count + "\t" + Path.GetFileNameWithoutExtension(folderList[i]) + ".md5 Generated.");
        //        using (StreamWriter file = File.AppendText(log))
        //        {
        //            file.WriteLine(Convert.ToString(TotalLines) + "/" + folderList.Count + "\t" + Path.GetFileNameWithoutExtension(folderList[i]) + ".md5 Generated.");
        //        }
        //    }
        //    TimeSpan ts = DateTime.Now.Subtract(PStart);
        //    Console.WriteLine("Md5 checksume files generated : " + TotalLines + ", Total files checksumed : " + OK);
        //    Console.WriteLine("Completed  @ " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        //    Console.WriteLine("Time Elapse : " + ts.ToString());
        //    using (StreamWriter file = File.AppendText(log))
        //    {
        //        file.WriteLine("Md5 checksume files generated : " + TotalLines + ", Total files checksumed : " + OK);
        //        file.WriteLine("Completed  @ " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        //        file.WriteLine("Time Elapse : " + ts.ToString());
        //    }
        //}
        static private List<string> GetFiles(string path, string pattern, bool UseDateFilter)
        {
            var files = new List<string>();

            try
            {
                if (!path.Contains("$RECYCLE.BIN") && !path.Contains("#recycle") && !path.Contains("@Recycle"))
                {
                    if (UseDateFilter)
                    {
                        string[] candidates = Directory.GetFiles(path, pattern, SearchOption.TopDirectoryOnly);
                        foreach (string c in candidates)
                        {
                            FileInfo fi = new FileInfo(c);
                            DateTime FileDate = DateTime.MinValue;
                            if (fi.LastWriteTime > fi.CreationTime)
                                FileDate = fi.LastWriteTime;
                            else
                                FileDate = fi.CreationTime;
                            if (Convert.ToDateTime(FileDate.ToString("yyyy-MM-dd")) >= FilterDate)
                            //&& (fi.FullName.Substring(fi.FullName.Length-4).ToLowerInvariant().Contains("md5") == false))
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

            switch (pattern)
            {
                case "*.*": //Create
                    return files.Where(x => !x.ToLowerInvariant().Contains("thumbs.db") && !x.ToLowerInvariant().Contains("desktop.ini") && !x.ToLowerInvariant().Contains(".md5")).ToList();

                default: //Verify
                    return files.Where(x => !x.ToLowerInvariant().Contains("thumbs.db") && !x.ToLowerInvariant().Contains("desktop.ini")).ToList();

            }
        }
        static private List<string> Getfolders(string path)
        {
            var folders = new List<string>();

            try
            {
                //var results = Directory.GetDirectories(path).ToList();
                //folders = results.Where(x => !x.Contains("$RECYCLE.BIN") && !x.Contains("#recycle") && !x.Contains("System Volume Information")).ToList();
                if (UseDateFilter)
                {
                    string[] candidates = Directory.GetDirectories(path);
                    foreach (string c in candidates)
                    {
                        DirectoryInfo di = new DirectoryInfo(c);
                        DateTime DirectoryDate = DateTime.MinValue;
                        if (di.LastWriteTime > di.CreationTime)
                            DirectoryDate = di.LastWriteTime;
                        else
                            DirectoryDate = di.CreationTime;
                        if (Convert.ToDateTime(DirectoryDate.ToString("yyyy-MM-dd")) >= FilterDate)
                        {
                            folders.Add(c);
                        }
                    }
                    folders = folders.Where(x => !x.Contains("$RECYCLE.BIN") && !x.Contains("#recycle") && !x.Contains("System Volume Information") && !x.Contains("@Recycle")).ToList();
                }
                else
                {
                    folders.AddRange(Directory.GetDirectories(path).ToList().Where(x => !x.Contains("$RECYCLE.BIN") && !x.Contains("#recycle") && !x.Contains("System Volume Information") && !x.Contains("@Recycle")).ToList());
                }
            }
            catch (UnauthorizedAccessException) { }

            return folders;
        }
        static string ComputeMD5(string path)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(path))
                {
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();
                }
            }
        }
        static private List<FileCheckStat> Sort(List<FileCheckStat> t)
        {
            int subfolderindex = t.FindIndex(a => a.filepath.Contains("\\"));
            List<FileCheckStat> FirstHalf = t.Where(a => a.filepath.Contains("\\")).ToList();
            FirstHalf.Sort((a, b) => a.filepath.CompareTo(b.filepath));
            //List<FileCheckStat> firsthalf = t.Take(subfolderindex).OrderBy(x => x.filepath).ToList();
            //List<FileCheckStat> Secondhalf = t.Skip(subfolderindex).OrderBy(x => x.filepath).ToList();
            List<FileCheckStat> SecondHalf = t.Where(a => !a.filepath.Contains("\\")).ToList();
            SecondHalf.Sort((a, b) => a.filepath.CompareTo(b.filepath));
            return new List<FileCheckStat>(FirstHalf.Concat(SecondHalf).ToList());
        }
    }
}
