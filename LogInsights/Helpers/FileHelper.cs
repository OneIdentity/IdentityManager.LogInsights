using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LogInsights.Helpers
{
    public class FileHelper
    { 

        public static string ToHumanBytes(float number)
        {
            if (number < 1024)
                return $"{System.Convert.ToInt32(number).ToString()} B";

            if (number < 1024f * 1014)
                return $"{System.Math.Round(number / 1024f, 1).ToString()} KB";

            if (number < 1024f * 1014 * 1024)
                return $"{System.Math.Round(number / (1024f * 1024), 1).ToString()} MB";

            if (number < 1024f * 1014 * 1024 * 1024)
                return $"{System.Math.Round(number / (1024f * 1024 * 1024), 1).ToString()} GB";

            return $"{System.Math.Round(number / (1024f * 1024 * 1024 * 1024), 1).ToString()} TB";
        }

        public static string[] OrderByContentTimestamp(string[] fileNames, out string[] filesWithoutTimestamp)
        {
            Dictionary<DateTime, string> fileHash = new Dictionary<DateTime, string>();
            List<string> filesWithoutTimestampLst = new List<string>();
            char[] buffer = new char[1024];
                       

            foreach (string filename in fileNames)
            {
                if (!File.Exists(filename))
                    continue;

                int trycnt = 0;
                bool fnd = false;

                try
                {
                    using (StreamReader sr = new StreamReader(filename, true))
                    {
                        while (!sr.EndOfStream && !fnd && trycnt < 128)
                        {
                            trycnt++;

                            string s = sr.ReadLine();

                            var match = Constants.regexTimeStampAtLinestart.Match(s);
                            if (match.Success)
                            {
                                string ts = match.Groups["Timestamp"].Value;
                                DateTime tss;
                                if (DateTime.TryParse(ts, out tss))
                                {
                                    while (fileHash.ContainsKey(tss))  //this could happen when you analyse two files starting at the exact same time
                                        tss = tss.AddTicks(3);

                                    fileHash.Add(tss, filename);
                                    fnd = true;
                                }
                            }
                        }
                    }
                }
                catch { }

                if (!fnd)
                    filesWithoutTimestampLst.Add(filename);
            }

            filesWithoutTimestamp = filesWithoutTimestampLst.ToArray();

            return fileHash.OrderBy(t => t.Key).Select(t => t.Value).ToArray();
        }

        public static string[] DirectoryGetFilesSave(string path, string searchpattern, SearchOption searchopt)
        {
            try
            {
                return Directory.GetFiles(path, searchpattern, searchopt);
            }
            catch
            {
                return new string[] { };
            }
        }

        public static long GetFileSizes(string [] fileNames)
        {
            long res = 0;
            foreach (string f in fileNames)
                try
                {
                    FileInfo fi = new FileInfo(f);
                    res += fi.Length;
                }
                catch { }

            return (res);
        }


        public static DirectoryFilenameInformation[] GetFileAndDirectory(string[] filesOrDirectories)
        {
            List<DirectoryFilenameInformation> res = new List<DirectoryFilenameInformation>();

            foreach (string fileOrDirectory in filesOrDirectories)
            {
                if (System.IO.Directory.Exists(fileOrDirectory))  //fileOrDirectory is a folder
                    res.Add(new DirectoryFilenameInformation() {
                                directoryname = fileOrDirectory,
                                filenames = new string[] { "*.txt", "*.log" } 
                        });
                else //fileOrDirectory is a file name
                {
                    var d = System.IO.Path.GetDirectoryName(fileOrDirectory);
                    var f = System.IO.Path.GetFileName(fileOrDirectory);
                    res.Add(new DirectoryFilenameInformation() {
                                directoryname = d != "" ? d : ".",
                                filenames = new string[] { f } 
                    });
                }
            }

            return res.ToArray();
        } 

       
        public static string GetBestRelativeFilename(string filename, string[] allfiles)
        {
            /*
                C:\Data\FolderX\Archive\file1.log
                C:\Data\FolderX\file2.log  ==> FolderX\file2.log
                C:\Data\blaa.txt
            */

            string[] subfolders = GetFileFolderparts(filename);

            if (subfolders == null)
                return (filename);

            List<string[]> allfilesSubfolder = allfiles.Select(t => GetFileFolderparts(t)).ToList();

            for (int pos = 0; pos < subfolders.Length; pos++)
            {
                if (allfilesSubfolder.Any(t => ! (t.Length > pos && t[pos] == subfolders[pos])))
                {
                    if (pos == 0)
                        return (filename);

                    string final = "";
                    for (int i = pos; i < subfolders.Length; i++)
                        final += subfolders[i] + @"\";

                    return (final + System.IO.Path.GetFileName(filename));
                }
            }

            return (System.IO.Path.GetFileName(filename));
        }

        public static string[] GetFileFolderparts(string fn)
        {
            var fullfolder = System.IO.Path.GetDirectoryName(fn);

            if (fullfolder == "")
                return null;

            return (fullfolder.Split('\\'));
        }

        public static string GetNewFilename(string oldFilename, string postfix, string newfoldername = null)
        {
            if (newfoldername == null || newfoldername == "." || newfoldername == ".\\")
                newfoldername = "";

            string newFileFolder = (newfoldername == "") ? Path.GetDirectoryName(oldFilename) : newfoldername;

            foreach (char c in Path.GetInvalidPathChars())
                newFileFolder = newFileFolder.Replace(c, '_');

            newFileFolder = Regex.Replace(newFileFolder, @"https?:..", "_", RegexOptions.IgnoreCase | RegexOptions.Compiled);

            if (!EnsurePathElementsExists(newFileFolder))
                newFileFolder = Path.GetDirectoryName(oldFilename);

            string oldFileBase = Path.GetFileNameWithoutExtension(oldFilename);
            string oldFileExt = Path.GetExtension(oldFilename);

            foreach (char c in Path.GetInvalidFileNameChars())
                oldFileBase = oldFileBase.Replace(c, '_');

            string newname = Path.Combine(newFileFolder, $"{oldFileBase}{postfix}{oldFileExt}");

            int i = 1;
            while (File.Exists(newname))
                newname = Path.Combine(newFileFolder, $"{oldFileBase}{postfix}_{i++}{oldFileExt}");

            return (newname);
        }

        public static bool EnsurePathElementsExists(string folder)
        {
            if (string.IsNullOrEmpty(folder))
                return false;

            //we do not check or create folders on a share
            if (folder.StartsWith(@"\\"))
                return true; 

            char sep = Path.DirectorySeparatorChar;

            //are we on top of file system OR is this a relative path where we cannot climp up
            if (Path.IsPathRooted(folder) || !folder.Contains(sep) || folder.Length <= 3)
            {
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                return Directory.Exists(folder);
            }

            //we are not on top of file system             //C:\folder\sub\abc
            string parentFolder = folder.Substring(0, folder.LastIndexOf(sep));

            //check for parent
            if (EnsurePathElementsExists(parentFolder))            
                //now we know that parent folder exists, so create this folder if not yet available
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

            return Directory.Exists(folder);
        }

        public static string EnsureFolderisWritableOrReturnDefault(string foldername, string defaultfolder = ".")
        {
            bool takeDefault = false;
            try
            {
                System.IO.DirectoryInfo di = new DirectoryInfo(foldername);
                if (!di.Exists || di.Attributes.HasFlag(FileAttributes.ReadOnly) || di.Attributes.HasFlag(FileAttributes.Offline))
                    takeDefault = true;
                
                //try get ACL, if this folder is not accessable, the method fails
                di.GetAccessControl();
            }
            catch {
                takeDefault = true;
            }

            if (!takeDefault)
                return foldername;

            if (string.IsNullOrEmpty(defaultfolder))
                return ".";

            defaultfolder = defaultfolder.Trim();

            if (Directory.Exists(defaultfolder))
                return defaultfolder;

            return ".";
        }
    }

    public class DirectoryFilenameInformation
    {
        public string directoryname;
        public string[] filenames;
    }

    public class FileInformationContext
    {
        public string filename;
        public long filepos;

        public int fileposRelativeInView;

        public FileInformationContext(string filename, long filepos)
        {
            this.filename = filename;
            this.filepos = filepos;
        }

        public FileInformationContext(string filename, long filepos, int fileposRelativeInView)
        {
            this.filename = filename;
            this.filepos = filepos;
            this.fileposRelativeInView = fileposRelativeInView;
        }
    }
}
