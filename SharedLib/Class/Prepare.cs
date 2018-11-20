using System;
using System.IO;

namespace SharedLib.Class
{
    public class Prepare
    {
        public static Input_ModeSection ParseModeSection(string input)
        {
            Input_ModeSection Desire = new Input_ModeSection();
            if (!string.IsNullOrEmpty(input))
            {
                if (input.Contains(","))
                {
                    string[] splitter = input.Split(',');
                    Desire.Mode = (Mode)(splitter[0]).ToLowerInvariant().ToCharArray()[0];
                    Desire.ProcessingPower = (ProcessingPower)(splitter[1]).ToLowerInvariant().ToCharArray()[0];
                    if (splitter.Length > 2)
                    {
                        Desire.Optional_Parameter = input.Split(',')[2];
                    }
                }
                else
                {
                    Desire.Mode = (Mode)Convert.ToChar(input);
                }
            }
            return Desire;
        }
        public static string ProduceReportName(string WorkingDirectory, Mode Mode, ArgumentType aType = 0, string Argument = null)
        {
            string TimeStamp = DateTime.Now.ToString(TimeFormat.ForReport);
            switch (Mode)
            {
                default:
                case Mode.Verify_Checksums:
                    if (!string.IsNullOrEmpty(Argument))
                    {
                        switch (aType)
                        {
                            case ArgumentType.Date:
                                return Path.Combine(WorkingDirectory, $@"Report_ChecksumVerify_Date-{ Argument }_" + TimeStamp + OutputExtension.TXT);
                            case ArgumentType.Index:
                                return Path.Combine(WorkingDirectory, $@"Report_ChecksumVerify_Index-{ Argument }_" + TimeStamp + OutputExtension.TXT);
                            case ArgumentType.None:
                                break;
                        }
                    }
                    return Path.Combine(WorkingDirectory, "Report_ChecksumVerify_" + TimeStamp + OutputExtension.TXT);
                case Mode.Verify_Path_Length_and_Existences:
                    return Path.Combine(WorkingDirectory, "Report_PathVerify_" + TimeStamp + OutputExtension.TXT);
                case Mode.Combine_All_MD5:
                    return Path.Combine(WorkingDirectory, "Report_Combined_" + TimeStamp + OutputExtension.MD5);
                case Mode.Create_Checksum_for_Every_Folder:
                    return Path.Combine(WorkingDirectory, "Report_ChecksumRefresh_" + TimeStamp + OutputExtension.TXT);
                case Mode.Check_Video_Corruption:
                    return Path.Combine(WorkingDirectory, "Report_VideoCorruptionChecks_" + TimeStamp + OutputExtension.TXT);
            }
        }
    }
}
