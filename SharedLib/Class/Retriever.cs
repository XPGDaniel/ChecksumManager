using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SharedLib.Class
{
    public class Retriever
    {
        public static List<string> GetFiles(string path, string pattern, DateTime? FilterDate, bool UseDateFilter)
        {
            var files = new List<string>();
            try
            {
                if (!path.Contains("$RECYCLE.BIN") && !path.Contains("#recycle") && !path.Contains("@Recycle") && !path.Contains("@eaDir") && !path.Contains(".@__thumb"))
                {
                    string[] candidates = Directory.GetFiles(path, pattern, SearchOption.TopDirectoryOnly);
                    foreach (string c in candidates)
                    {
                        FileInfo fi = new FileInfo(c);
                        if (!fi.Attributes.HasFlag(FileAttributes.Hidden))
                        {
                            if (UseDateFilter)
                            {
                                DateTime FileDate = DateTime.MinValue;
                                if (fi.LastWriteTime > fi.CreationTime)
                                    FileDate = fi.LastWriteTime;
                                else
                                    FileDate = fi.CreationTime;
                                if (FileDate.Date >= FilterDate)
                                {
                                    files.Add(c);
                                }
                            }
                            else
                            {
                                files.Add(c);
                            }
                        }
                    }
                    foreach (var directory in Directory.GetDirectories(path))
                        files.AddRange(GetFiles(directory, pattern, FilterDate, UseDateFilter));
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
        public static List<string> Getfolders(string path, DateTime? FilterDate, bool UseDateFilter)
        {
            var folders = new List<string>();

            try
            {
                string[] candidates = Directory.GetDirectories(path);
                foreach (string c in candidates)
                {
                    DirectoryInfo di = new DirectoryInfo(c);
                    if ((di.Attributes & FileAttributes.Hidden) == 0)
                    {
                        if (UseDateFilter)
                        {
                            DateTime DirectoryDate = DateTime.MinValue;
                            if (di.LastWriteTime > di.CreationTime)
                                DirectoryDate = di.LastWriteTime;
                            else
                                DirectoryDate = di.CreationTime;
                            if (DirectoryDate.Date >= FilterDate)
                            {
                                folders.Add(c);
                            }
                        }
                        else
                        {
                            folders.Add(c);
                        }
                    }
                }
                folders = folders.Where(x => 
                !x.Contains("$RECYCLE.BIN") && 
                !x.Contains("#recycle") && 
                !x.Contains("System Volume Information") && 
                !x.Contains("@Recycle") && 
                !x.Contains("@eaDir") && 
                !path.Contains(".@__thumb")
                ).ToList();
            }
            catch (UnauthorizedAccessException) { }

            return folders;
        }
        public static List<string> GetVideoFiles(string path)
        {
            var extensions = new[] { "*.mkv", "*.mp4", "*.avi", "*.wmv", "*.rmvb", "*.ogg", "*.webm" };
            List<string> files = new List<string>();
            try
            {
                if (!path.Contains("$RECYCLE.BIN") && !path.Contains("#recycle") && !path.Contains("@Recycle"))
                {
                    List<string> candidates = extensions.SelectMany(ext => Directory.GetFiles(path, ext, SearchOption.AllDirectories)).ToList();
                    foreach (string c in candidates)
                    {
                        FileInfo fi = new FileInfo(c);
                        files.Add(fi.FullName);
                    }
                }
            }
            catch (UnauthorizedAccessException) { }
            if (files.Count > 0)
                return files.OrderBy(r => Path.GetDirectoryName(r)).ToList();
            else
                return null;
        }
    }
}
