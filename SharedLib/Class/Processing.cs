using SharedLib.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SharedLib.Class
{
    public class Processing
    {
        private static object lockMe = new object();
        public static void Verify(string SearchPath, string ReportFile, Mode Mode, int StartingPoint = 0, DateTime? FilterDate = null, bool UseDateFilter = false)
        {
            DateTime PStart = DateTime.Now;
            List<FileItem> lists = new List<FileItem>();
            List<string> md5List = Retriever.GetFiles(SearchPath, SearchExtension.MD5, FilterDate, UseDateFilter);
            Output_ProcessReport Result = new Output_ProcessReport();
            Console.WriteLine("No. of md5 : " + md5List.Count);
            if (!File.Exists(ReportFile))
            {
                if (!CreateReportFile(ReportFile, Mode, md5List))
                    throw new Exception("Unable to create report file.");
            }
            for (int i = StartingPoint; i < md5List.Count; i++)
            {
                lists = BuildFileItemList(md5List[i]);
                if (lists.Any())
                {
                    Result.Total += lists.Count;
                    if (Mode != Mode.Combine_All_MD5)
                        using (StreamWriter file = File.AppendText(ReportFile))
                        {
                            file.WriteLine(Convert.ToString(i + 1) + "/" + md5List.Count + "\t" + md5List[i].Replace(SearchPath, "") + " Proccessing..."); //md5List[i].Split('\'')[md5List[i].Split('\'').Length - 1]
                        }
                    Output_ProcessReport ItemResult = ProcessMD5File(lists, Mode, ReportFile);
                    Result.Healthy += ItemResult.Healthy;
                    Result.Damaged += ItemResult.Damaged;
                    Result.Missing += ItemResult.Missing;

                    if (Mode != Mode.Combine_All_MD5)
                        using (StreamWriter file = File.AppendText(ReportFile))
                        {
                            file.WriteLine("Verification Completed");
                            file.WriteLine("----------------------");
                        }
                    Console.WriteLine(Convert.ToString(i + 1) + "/" + md5List.Count + "\t" + md5List[i].Replace(SearchPath, "") + " Checked.");
                    SetProgressBar(i, md5List.Count);
                    lists.Clear();
                }
            }
            TimeSpan Elapsed = DateTime.Now.Subtract(PStart);
            SummariseReport(ReportFile, Mode, Elapsed, Result);
        }
        public static void ParalleRefresh(string SearchPath, string ReportFile, Mode Mode, ProcessingPower ProcessingPower, int StartingPoint = 0, DateTime? FilterDate = null, bool UseDateFilter = false)
        {
            DateTime PStart = DateTime.Now;
            Output_ProcessReport Report = new Output_ProcessReport();
            List<FileItem> FileList = new List<FileItem>();
            List<FileItem> HealthyList = new List<FileItem>();
            List<FileItem> DamagedList = new List<FileItem>();
            List<FileItem> MissingList = new List<FileItem>();
            List<string> folderList = Retriever.Getfolders(SearchPath, FilterDate, UseDateFilter);
            List<string> md5List = Retriever.GetFiles(SearchPath, SearchExtension.MD5, FilterDate, UseDateFilter);
            if (!File.Exists(ReportFile))
            {
                CreateReportFile(ReportFile, Mode, null);
            }
            Console.WriteLine("No. of md5 : " + md5List.Count);
            Console.WriteLine("No. of folders : " + folderList.Count);
            for (int i = StartingPoint; i < folderList.Count; i++)
            {
                string md5File = md5List.FirstOrDefault(s => Path.GetFileNameWithoutExtension(s) == Path.GetFileName(folderList[i]));
                if (!string.IsNullOrEmpty(md5File))
                {
                    FileList = BuildFileItemList(md5File);
                }
                FileList = ProcessFilesInFolder(folderList[i], ReportFile, FileList, DamagedList, ProcessingPower);
                if (FileList.Any())
                {
                    FileList = Sort(FileList, folderList[i]);
                    HealthyList = FileList.Where(s => s.IsChecksummed != ChecksumStat.DuplicateNameOrCorrupted && s.IsChecksummed != ChecksumStat.Init).ToList();
                    if (HealthyList.Any())
                    {
                        if (ProcessGeneratedList(HealthyList, ListType.Healthy, folderList[i]))
                        {
                            Report.Healthy += HealthyList.Count;
                        }
                        HealthyList.Clear();
                    }
                    DamagedList = FileList.Where(s => s.IsChecksummed == ChecksumStat.DuplicateNameOrCorrupted).ToList();
                    if (DamagedList.Any())
                    {
                        DamagedList = Sort(DamagedList, folderList[i]);
                        if (ProcessGeneratedList(DamagedList, ListType.Damaged, folderList[i]))
                        {
                            Report.Damaged += DamagedList.Count;
                        }
                        DamagedList.Clear();
                    }
                    MissingList = FileList.Where(s => s.IsChecksummed == ChecksumStat.Init).ToList();
                    if (MissingList.Any())
                    {
                        MissingList = Sort(MissingList, folderList[i]);
                        if (ProcessGeneratedList(MissingList, ListType.Missing, folderList[i]))
                        {
                            Report.Missing += MissingList.Count;
                        }
                        MissingList.Clear();
                    }
                    FileList.Clear();
                }
                SetProgressBar(i, folderList.Count);
                Console.WriteLine(Convert.ToString((i + 1)) + "/" + folderList.Count + "\t" + Path.GetFileName(folderList[i]) + OutputExtension.MD5 + " Generated.");
                using (StreamWriter file = File.AppendText(ReportFile))
                {
                    file.WriteLine(Convert.ToString((i + 1)) + "/" + folderList.Count + "\t" + Path.GetFileName(folderList[i]) + OutputExtension.MD5 + " Generated.");
                }
                Report.Total += 1;
            }
            TimeSpan Elapsed = DateTime.Now.Subtract(PStart);
            SummariseReport(ReportFile, Mode, Elapsed, Report);
            //DateTime PStart = DateTime.Now;
            //Output_ProcessReport Report = new Output_ProcessReport();
            //List<FileItem> FileList = new List<FileItem>();
            //List<FileItem> HealthyList = new List<FileItem>();
            //List<FileItem> DamagedList = new List<FileItem>();
            //List<FileItem> MissingList = new List<FileItem>();
            //List<string> folderList = Retriever.Getfolders(SearchPath, FilterDate, UseDateFilter);
            //List<string> md5List = Retriever.GetFiles(SearchPath, SearchExtension.MD5, FilterDate, UseDateFilter);
            //if (!File.Exists(ReportFile))
            //{
            //    CreateReportFile(ReportFile, Mode, null);
            //}
            //Console.WriteLine("No. of md5 : " + md5List.Count);
            //Console.WriteLine("No. of folders : " + folderList.Count);
            //int index = 0;
            //StreamWriter file = File.AppendText(ReportFile);
            ////using (StreamWriter file = File.AppendText(ReportFile))
            ////{
            //try
            //{
            //    var result = Parallel.ForEach(folderList, new ParallelOptions { MaxDegreeOfParallelism = 2 },
            //    folder =>
            //    {
            //        string FolderPath = folder;
            //        index += 1;
            //        Task task = Task.Factory.StartNew(() =>    // Begin task
            //        {
            //            return TaskTest(md5List, FileList, HealthyList, DamagedList, MissingList, FolderPath, ReportFile);
            //        })
            //            .ContinueWith(ant =>
            //            {
            //                Report.Healthy += ant.Result.Healthy;
            //                Report.Damaged += ant.Result.Damaged;
            //                Report.Missing += ant.Result.Missing;
            //                Report.Total += ant.Result.Total;
            //                SetProgressBar(index, folderList.Count);
            //                Console.WriteLine(Convert.ToString(index + 1) + "/" + folderList.Count + "\t" + Path.GetFileName(FolderPath) + OutputExtension.MD5 + " Generated.");
            //            //using (StreamWriter file = File.AppendText(ReportFile))
            //            //{
            //            file.WriteLine(Convert.ToString(index + 1) + "/" + folderList.Count + "\t" + Path.GetFileName(FolderPath) + OutputExtension.MD5 + " Generated.");
            //            //}
            //        });
            //    }).IsCompleted;
            //    //if (result)
            //    //    file.Dispose();
            //}
            //finally
            //{
            //    file.Dispose();
            //}
            ////}
            //TimeSpan Elapsed = DateTime.Now.Subtract(PStart);
            //SummariseReport(ReportFile, Mode, Elapsed, Report);
        }
        public static void VideoCheck(string SearchPath, string ReportFile, Mode Mode)
        {
            DateTime PStart = DateTime.Now;
            Output_ProcessReport Report = new Output_ProcessReport();
            List<string> videoList = Retriever.GetVideoFiles(SearchPath);
            if (!File.Exists(ReportFile))
            {
                CreateReportFile(ReportFile, Mode, null);
            }
            if (videoList != null)
                if (videoList.Any())
                    for (var i = 0; i < videoList.Count; i++)
                    {
                        if (ExecuteFFMPEG(ReportFile, SearchPath, videoList[i], ProcessType.VideoCheck))
                        {
                            Report.Damaged += 1;
                            string frameTraceReport = Path.Combine(SearchPath, Path.GetFileName(videoList[i]) + OutputExtension.TXT);
                            if (!File.Exists(frameTraceReport))
                            {
                                CreateReportFile(frameTraceReport, Mode, null);
                            }
                            ExecuteFFMPEG(ReportFile, SearchPath, videoList[i], ProcessType.FrameTrace);
                            if (File.Exists(frameTraceReport))
                            {
                                if (new FileInfo(frameTraceReport).Length == 0)
                                {
                                    try
                                    {
                                        File.Delete(frameTraceReport);
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex.ToString());
                                    }
                                }
                            }
                        }
                        else
                            Report.Healthy += 1;
                        SetProgressBar(i, videoList.Count);
                        Report.Total += 1;
                    }
            TimeSpan Elapsed = DateTime.Now.Subtract(PStart);
            SummariseReport(ReportFile, Mode, Elapsed, Report);
        }
        #region private function
        public static string ComputeMD5(string path)
        {
            using (MD5 md5 = MD5.Create())
            {
                using (FileStream stream = File.OpenRead(path))
                {
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
                }
            }
        }
        private static bool CreateReportFile(string ReportFile, Mode Mode, List<string> list)
        {
            try
            {
                using (StreamWriter file = File.CreateText(ReportFile))
                {
                    if (Mode != Mode.Combine_All_MD5)
                        file.WriteLine($@"Mode.{ Enum.GetName(typeof(Mode), Mode) }");
                    if (Mode != Mode.Combine_All_MD5)
                        if (list != null)
                            if (list.Any())
                            {
                                StringBuilder md5listbuilder = new StringBuilder();
                                foreach (string md5File in list)
                                {
                                    md5listbuilder.Append(md5File).AppendLine();
                                }
                                md5listbuilder.Append("----------------------").AppendLine();
                                file.Write(md5listbuilder.ToString());
                                md5listbuilder.Clear();
                            }
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private static List<FileItem> BuildFileItemList(string md5File)
        {
            try
            {
                List<FileItem> ItemList = new List<FileItem>();
                string[] lines = File.ReadAllLines(md5File);
                foreach (string line in lines)
                {
                    string TrimmedLine = line.Trim();
                    if (!string.IsNullOrEmpty(TrimmedLine))
                    {
                        FileItem fItem = new FileItem()
                        {
                            Hash = TrimmedLine.Split('*')[0].Trim()
                        };
                        if (line.Contains("*"))
                        {
                            fItem.Filepath = Path.Combine(new FileInfo(md5File).Directory.FullName, TrimmedLine.Split('*')[1].Trim());
                        }
                        else
                        {
                            fItem.Filepath = Path.Combine(new FileInfo(md5File).Directory.FullName, TrimmedLine.Substring(33).Trim());
                        }
                        fItem.IsChecksummed = ChecksumStat.Init;
                        ItemList.Add(fItem);
                    }
                }
                return ItemList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private static List<FileItem> ProcessFilesInFolder(string SearchPath, string ReportFile, List<FileItem> fileList, List<FileItem> DamagedList, ProcessingPower ProcessingPower)
        {
            List<FileItem> Calculated = Retriever.GetFiles(SearchPath, SearchExtension.Everything, null, false).Select(x => new FileItem() { Filepath = x }).ToList();
            ParallelOptions pOptions = new ParallelOptions();
            switch (ProcessingPower)
            {
                case ProcessingPower.Half:
                    pOptions.MaxDegreeOfParallelism = (Environment.ProcessorCount / 2);
                    break;
                case ProcessingPower.Full:
                    pOptions.MaxDegreeOfParallelism = Environment.ProcessorCount;
                    break;
                case ProcessingPower.Single:
                default:
                    pOptions.MaxDegreeOfParallelism = 1;
                    break;
            }
            var result = Parallel.ForEach(Calculated, pOptions,
                file =>
                {
                    file.Hash = ComputeMD5(file.Filepath);
                });
            if (Calculated.Any())
            {
                foreach (FileItem file in Calculated)
                {
                    try
                    {
                        FileItem fcs = fileList.FirstOrDefault(s => s.Hash == file.Hash && s.Filepath == file.Filepath);
                        if (fcs != null)
                        {//old file checksummed
                            foreach (var fileItem in fileList.Where(s => s.Hash == file.Hash && s.Filepath == file.Filepath))
                            {
                                fileItem.IsChecksummed = ChecksumStat.ExistingFileChecked;
                            }
                        }
                        else
                        {
                            FileItem targetfcs = fileList.FirstOrDefault(s => s.Hash != file.Hash && s.Filepath == file.Filepath);
                            if (targetfcs != null)
                            {//different file with duplicate filename or file corrupted
                                targetfcs.IsChecksummed = ChecksumStat.DuplicateNameOrCorrupted;
                                DamagedList.Add(targetfcs);
                                fileList.Remove(targetfcs);
                            }
                            //new file
                            FileItem fs = new FileItem()
                            {
                                Hash = file.Hash,
                                Filepath = file.Filepath,
                                IsChecksummed = ChecksumStat.NewFileChecked
                            };
                            fileList.Add(fs);
                        }
                    }
                    catch (UnauthorizedAccessException uaex)
                    {
                        Console.WriteLine(uaex.Message);
                        using (StreamWriter report = File.AppendText(ReportFile))
                        {
                            report.WriteLine(uaex.Message);
                        }
                    }
                }
            }
            return fileList;
        }
        private static Output_ProcessReport ProcessMD5File(List<FileItem> list, Mode Mode, string ReportFile)
        {
            try
            {
                Output_ProcessReport Report = new Output_ProcessReport();
                using (StreamWriter file = File.AppendText(ReportFile))
                {
                    foreach (FileItem fItem in list)
                    {
                        if (Mode == Mode.Combine_All_MD5)
                        {
                            file.WriteLine(fItem.Hash + " *" + fItem.Filepath.Substring(fItem.Filepath.IndexOf("\\") + 1));
                            continue;
                        }

                        if (!File.Exists(fItem.Filepath))
                        {
                            file.WriteLine("Missing \t" + Path.GetFileName(fItem.Filepath));
                            Report.Missing++;
                            continue;
                        }

                        if (Mode == Mode.Verify_Checksums)
                        {
                            if (fItem.Hash.ToLowerInvariant() == ComputeMD5(fItem.Filepath).ToLowerInvariant())
                            {
                                file.WriteLine("Healthy \t" + Path.GetFileName(fItem.Filepath));
                                Report.Healthy++;
                            }
                            else
                            {
                                file.WriteLine("Damaged \t" + Path.GetFileName(fItem.Filepath));
                                Report.Damaged++;
                            }
                        }
                        else
                        {
                            file.WriteLine("Exist \t" + Path.GetFileName(fItem.Filepath));
                            Report.Healthy++;
                        }
                    }
                }
                return Report;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private static bool ProcessGeneratedList(List<FileItem> list, ListType lType, string FolderPath)
        {
            try
            {
                StringBuilder builder = new StringBuilder();
                foreach (FileItem fItem in list)
                {
                    fItem.Filepath = fItem.Filepath.Replace(FolderPath + "\\", "");
                    switch (lType)
                    {
                        case ListType.Healthy:
                            if (fItem.IsChecksummed != ChecksumStat.DuplicateNameOrCorrupted && fItem.IsChecksummed != ChecksumStat.Init)
                            {
                                builder.Append(fItem.Hash + " *" + fItem.Filepath).AppendLine();
                            }
                            break;
                        case ListType.Damaged:
                        case ListType.Missing:
                            builder.Append(fItem.Hash + " *" + fItem.Filepath).AppendLine();
                            break;
                    }
                }

                switch (lType)
                {
                    case ListType.Healthy:
                        using (StreamWriter writer = File.CreateText(Path.Combine(FolderPath, Path.GetFileName(FolderPath) + OutputExtension.MD5)))
                        {
                            writer.Write(builder.ToString());
                        }
                        break;
                    case ListType.Damaged:
                        using (StreamWriter writer = File.CreateText(Path.Combine(FolderPath, Path.GetFileName(FolderPath) + "-damaged" + OutputExtension.MD5)))
                        {
                            writer.Write(builder.ToString());
                        }
                        break;
                    case ListType.Missing:
                        using (StreamWriter writer = File.CreateText(Path.Combine(FolderPath, Path.GetFileName(FolderPath) + "-missing" + OutputExtension.MD5)))
                        {
                            writer.Write(builder.ToString());
                        }
                        break;
                }
                builder.Clear();
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private static List<FileItem> Sort(List<FileItem> t, string SearchPath)
        {
            List<FileItem> FirstHalf = t.Where(a => a.Filepath.Replace(SearchPath + "\\", "").Contains("\\")).ToList();
            FirstHalf.Sort((a, b) => a.Filepath.CompareTo(b.Filepath));
            List<FileItem> SecondHalf = t.Where(a => !a.Filepath.Replace(SearchPath + "\\", "").Contains("\\")).ToList();
            SecondHalf.Sort((a, b) => a.Filepath.CompareTo(b.Filepath));
            return new List<FileItem>(FirstHalf.Concat(SecondHalf).ToList());
        }
        private static void SummariseReport(string ReportFile, Mode Mode, TimeSpan Elapsed, Output_ProcessReport Result)
        {
            using (StreamWriter file = File.AppendText(ReportFile))
            {
                string SummaryTotal = null;
                switch (Mode)
                {
                    case Mode.Create_Checksum_for_Every_Folder:
                        SummaryTotal = "Total md5s generated : " + Result.Total;
                        break;
                    case Mode.Check_Video_Corruption:
                        SummaryTotal = "Total video checked : " + Result.Total;
                        break;
                    default:
                        SummaryTotal = "Total files checked : " + Result.Total;
                        break;
                }
                string SummarySubCat = "Healthy : " + Result.Healthy + ", Damaged : " + Result.Damaged + ", Missing : " + Result.Missing;
                string SummaryTimeStamp = "Completed  @ " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string SummaryElasp = "Time Elapse : " + Elapsed.ToString();
                Console.WriteLine(SummaryTotal);
                Console.WriteLine(SummarySubCat);
                Console.WriteLine(SummaryTimeStamp);
                Console.WriteLine(SummaryElasp);

                if (Mode != Mode.Combine_All_MD5)
                {
                    file.WriteLine("Version:" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
                    file.WriteLine(SummaryTotal);
                    file.WriteLine(SummarySubCat);
                    file.WriteLine(SummaryTimeStamp);
                    file.WriteLine(SummaryElasp);
                }
            }
        }
        private static bool ExecuteFFMPEG(string ReportFile, string WorkingPath, string VideoPath, ProcessType pType)
        {
            using (StreamWriter Report = File.AppendText(ReportFile))
            {
                bool hasError = false;
                try
                {
                    switch (pType)
                    {
                        case ProcessType.VideoCheck:
                            Console.WriteLine(VideoPath.Replace(WorkingPath, "") + " checking...");
                            Report.WriteLine(VideoPath.Replace(WorkingPath, "") + " checking...");
                            break;
                        case ProcessType.FrameTrace:
                            Console.WriteLine(VideoPath.Replace(WorkingPath, "") + " frame tracing...");
                            Report.WriteLine(VideoPath.Replace(WorkingPath, "") + " frame tracing...");
                            break;
                    }
                    using (Process p = new Process())
                    {
                        p.StartInfo.FileName = @"C:\ffmpeg\bin\ffmpeg.exe";
                        string Arguments = null;
                        switch (pType)
                        {
                            case ProcessType.VideoCheck:
                                Arguments = @" -v error -i """ + VideoPath + @""" -f null -xerror -";
                                break;
                            case ProcessType.FrameTrace:
                                Arguments = @" -i """ + VideoPath + @""" -vf showinfo -f null -";
                                break;
                        }
                        p.StartInfo.Arguments = Arguments;
                        p.StartInfo.UseShellExecute = false;
                        p.StartInfo.WorkingDirectory = WorkingPath;
                        p.StartInfo.RedirectStandardOutput = true;
                        p.StartInfo.RedirectStandardError = true;
                        p.StartInfo.CreateNoWindow = false; //Default:true

                        p.EnableRaisingEvents = true;
                        p.OutputDataReceived += (a, e) =>
                        {
                            if (!string.IsNullOrEmpty(e.Data))
                            {
                                Console.WriteLine(e.Data);
                                Report.WriteLine(e.Data);
                            }
                        };
                        p.ErrorDataReceived += (a, e) =>
                        {
                            if (!string.IsNullOrEmpty(e.Data))
                            {
                                switch (pType)
                                {
                                    case ProcessType.VideoCheck:
                                        if (!e.Data.Contains("Application provided invalid, non monotonically increasing dts to muxer in stream"))
                                        {
                                            hasError = true;
                                        }
                                        Console.WriteLine(e.Data);
                                        Report.WriteLine(e.Data);
                                        break;
                                    case ProcessType.FrameTrace:
                                        if (!e.Data.Contains("Application provided invalid, non monotonically increasing dts to muxer in stream"))
                                        {
                                            hasError = true;
                                            Report.WriteLine($@"Error: {e.Data}");
                                        }
                                        break;
                                }
                            }
                        };
                        p.Start();
                        p.BeginOutputReadLine();
                        p.BeginErrorReadLine();
                        p.WaitForExit();
                        if (p.HasExited)
                        {
                            p.CancelErrorRead();
                            p.CancelOutputRead();
                            p.Close();
                            switch (pType)
                            {
                                case ProcessType.VideoCheck:
                                    Console.WriteLine(VideoPath.Replace(WorkingPath, "") + " check Completed");
                                    Report.WriteLine(VideoPath.Replace(WorkingPath, "") + " check Completed");
                                    break;
                                case ProcessType.FrameTrace:
                                    Console.WriteLine(VideoPath.Replace(WorkingPath, "") + " frame trace Completed");
                                    Report.WriteLine(VideoPath.Replace(WorkingPath, "") + " frame trace Completed");
                                    break;
                            }
                        }
                    }
                }
                catch (UnauthorizedAccessException uaex)
                {
                    Console.WriteLine(uaex.ToString());
                    Report.Write(uaex.ToString());
                }
                catch (Exception gex)
                {
                    Console.WriteLine(gex.ToString());
                    Report.Write(gex.ToString());
                }
                Report.Flush();
                return hasError;
            }
        }
        private static void SetProgressBar(int CurrentPos, int Total)
        {
            TaskbarProgress.SetValue(Process.GetCurrentProcess().MainWindowHandle, ((CurrentPos + 1) * 200 + Total) / (Total * 2), 100);
            TaskbarProgress.SetState(Process.GetCurrentProcess().MainWindowHandle, TaskbarProgress.TaskbarStates.Normal);
        }
    }
    #endregion
}