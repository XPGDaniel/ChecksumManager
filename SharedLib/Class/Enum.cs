namespace SharedLib.Class
{
    public enum Mode
    {
        Verify_Checksums = 'a',
        Verify_Path_Length_and_Existences = 'b',
        Combine_All_MD5 = 'c',
        Create_Checksum_for_Every_Folder = 'd',
        Check_Video_Corruption = 'e'
    };

    public enum ArgumentType
    {
        None = 0,
        Date = 1,
        Index = 2
    }

    public enum ChecksumStat
    {
        Init = 9,
        ExistingFileChecked = 1,
        DuplicateNameOrCorrupted = 2,
        NewFileChecked = 0
    }

    public enum ListType
    {
        Healthy = 0,
        Damaged = 1,
        Missing = 2
    }

    public enum ProcessType
    {
        VideoCheck = 0,
        FrameTrace = 1
    }
}
