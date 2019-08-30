using SharedLib.Class;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace SharedLib
{
    public class Entrance
    {
        public static string ReportFile = "";
        public static string WorkingDirectory { get; set; } = new FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location).Directory.FullName;

        public static void EntryPoint(string[] args)
        {
            try
            {
                Console.InputEncoding = Encoding.UTF8;
                Console.OutputEncoding = Encoding.UTF8;
                //string reallyLongDirectory = @"C:\Users\Daniel\Source\Repos\FolderRenameAssist\FolderRenameAssist\bin\Debug\New folder\[Ano Hi Mita Hana no Namae o Bokutachi wa Mada Shiranai][あの日見た花の名前を僕達はまだ知らない.][ANK-Raws] 劇場版 あの日見た花の名前を僕達はまだ知らない。 (BDrip x264 FLAC DTS TRUE-HD 5.1ch SUP Hi10P)\New Text Document.txt";
                //string reallyLongFile = @"D:\Downloaded\[Menelkir] THE iDOLM@STER Cinderella Girls (2012-2017) [MP3]\[2017-2017] CINDERELLA GIRLS THEATER\[2017-2017] CINDERELLA GIRLS THEATER 1st Season Theme\[2017.05.24] CINDERELLA GIRLS LITTLE STARS! エチュードは1曲だけ\08. lilac time (ピクニックライブ Acoustic Ver.) [bonus track].mp3";
                //Console.WriteLine($"Creating a directory that is {reallyLongDirectory.Length} characters long");
                //Directory.CreateDirectory(reallyLongDirectory);

                //Console.WriteLine(reallyLongFile);
                //Console.WriteLine(File.Exists(reallyLongFile));
                //Console.WriteLine(Processing.ComputeMD5(reallyLongFile));
                int StartingPoint = 0;
                Console.WriteLine("Version:" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
                Console.WriteLine("Proccessing Mode");
                Console.WriteLine("A. Verify Checksums");
                Console.WriteLine("\t(Initial index is 0, or you can input a different onem like {a,(ProcessingPower),50})");
                Console.WriteLine("\t(or input date format yyyy-MM-dd as Date filter, like {a," + DateTime.Today.AddHours(8).ToString("yyyy-MM-dd") + "} .)");
                Console.WriteLine("B. Verify File Existences and Path Length");
                Console.WriteLine("C. Combine All md5s into One");
                Console.WriteLine("D. Create Checksum for every folder.");
                Console.WriteLine("\t(Alternatively you can append input date format yyyy-MM-dd as Date filter, like {d,(ProcessingPower)," + DateTime.Today.ToString("yyyy-MM-dd") + "} .)");
                if (File.Exists(@"C:\ffmpeg\bin\ffmpeg.exe"))
                    Console.WriteLine("E. Check all video files for corruption.");
                Console.WriteLine("ProcessingPower : Single = 1, Half = 2, Full = 3");
                Console.WriteLine("Select Proccessing Mode : ");

                Input_ModeSection Input = Prepare.ParseModeSection(Console.ReadLine().ToLowerInvariant());

                switch (Input.Mode)
                {
                    default:
                    case Mode.Verify_Checksums:
                        if (!string.IsNullOrEmpty(Input.Optional_Parameter))
                        {
                            if (CheckParameter.IsDateParameter(Input.Optional_Parameter))
                            {
                                ReportFile = Prepare.ProduceReportName(WorkingDirectory, Input.Mode, ArgumentType.Date, Input.Optional_Parameter);
                                Processing.Verify(WorkingDirectory, ReportFile, Mode.Verify_Checksums, StartingPoint, Convert.ToDateTime(Input.Optional_Parameter).Date, true);
                                break;
                            }
                            else if (CheckParameter.IsIndexParameter(Input.Optional_Parameter))
                            {
                                StartingPoint = Convert.ToInt32(Input.Optional_Parameter);
                                ReportFile = Prepare.ProduceReportName(WorkingDirectory, Input.Mode, ArgumentType.Index, Input.Optional_Parameter);
                                Processing.Verify(WorkingDirectory, ReportFile, Mode.Verify_Checksums, StartingPoint);
                                break;
                            }
                        }
                        ReportFile = Prepare.ProduceReportName(WorkingDirectory, Input.Mode);
                        Processing.Verify(WorkingDirectory, ReportFile, Mode.Verify_Checksums);
                        break;
                    case Mode.Verify_Path_Length_and_Existences:
                        ReportFile = Prepare.ProduceReportName(WorkingDirectory, Input.Mode);
                        Processing.Verify(WorkingDirectory, ReportFile, Mode.Verify_Path_Length_and_Existences);
                        break;
                    case Mode.Combine_All_MD5:
                        ReportFile = Prepare.ProduceReportName(WorkingDirectory, Input.Mode);
                        Processing.Verify(WorkingDirectory, ReportFile, Mode.Combine_All_MD5);
                        break;
                    case Mode.Create_Checksum_for_Every_Folder:
                        if (CheckParameter.IsIndexParameter(Input.Optional_Parameter))
                        {
                            StartingPoint = Convert.ToInt32(Input.Optional_Parameter);
                        }
                        ReportFile = Prepare.ProduceReportName(WorkingDirectory, Input.Mode);
                        Processing.ParalleRefresh(WorkingDirectory, ReportFile, Mode.Create_Checksum_for_Every_Folder, Input.ProcessingPower, StartingPoint);
                        break;
                    case Mode.Check_Video_Corruption:
                        if (!File.Exists(@"C:\ffmpeg\bin\ffmpeg.exe"))
                            throw new Exception(@"C:\ffmpeg\bin\ffmpeg.exe not found.");
                        ReportFile = Prepare.ProduceReportName(WorkingDirectory, Input.Mode);
                        Processing.VideoCheck(WorkingDirectory, ReportFile, Mode.Check_Video_Corruption);
                        break;
                }
                Console.WriteLine("Report generated @ " + ReportFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                TaskbarProgress.SetState(Process.GetCurrentProcess().MainWindowHandle, TaskbarProgress.TaskbarStates.Error);
            }
            finally
            {
                ConsoleKeyInfo key = new ConsoleKeyInfo();
                while (key.Key != ConsoleKey.Enter)
                {
                    key = Console.ReadKey();
                }
            }
        }
    }
}
