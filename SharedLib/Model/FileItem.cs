using SharedLib.Class;

namespace SharedLib.Model
{
    public class FileItem
    {
        public string Hash { get; set; }
        public string Filepath { get; set; }
        public ChecksumStat IsChecksummed { get; set; }
    }
}
