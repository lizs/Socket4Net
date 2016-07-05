using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace socket4net
{
    /// <summary>
    ///     extension for file system
    /// </summary>
    public static class FileSys
    {
        public static IEnumerable<string> EnumerateFiles(
            string dir, string searchPattern, SearchOption searchOption = SearchOption.AllDirectories)
        {
            try
            {
#if NET45
                return Directory.EnumerateFiles(dir, searchPattern, searchOption).Select(x=>x.Replace("\\", "/"));
#else
                return Directory.GetFiles(dir, searchPattern, searchOption).Select(x=>x.Replace("\\", "/"));
#endif
            }
            catch (Exception e)
            {
                Logger.Ins.Exception("EnumerateFiles", e);
                return new string[]{};
            }
        }

        public static IEnumerable<string> EnumerateDirectories(
            string dir, string searchPattern, SearchOption searchOption = SearchOption.AllDirectories)
        {
            try
            {
#if NET45
                return Directory.EnumerateDirectories(dir, searchPattern, searchOption).Select(x=>x.Replace("\\", "/"));
#else
                return Directory.GetDirectories(dir, searchPattern, searchOption).Select(x => x.Replace("\\", "/"));
#endif
            }
            catch (Exception e)
            {
                Logger.Ins.Exception("EnumerateDirectories", e);
                return new string[] { };
            }
        }

        public static IEnumerable<string> EnumerateFilesAndDirectories(
            string dir, string searchPattern, SearchOption searchOption = SearchOption.AllDirectories)
        {
            try
            {
#if NET45
                return Directory.EnumerateFileSystemEntries(dir, searchPattern, searchOption).Select(x=>x.Replace("\\", "/"));
#else
                return EnumerateDirectories(dir, searchPattern, searchOption).Concat(EnumerateFiles(dir, searchPattern, searchOption)).Select(x => x.Replace("\\", "/"));
#endif
            }
            catch (Exception e)
            {
                Logger.Ins.Exception("EnumerateFilesAndDirectories", e);
                return new string[] { };
            }
        }

        public static IEnumerable<string> EnumerateFilesRelative(
            string dir, string searchPattern, SearchOption searchOption = SearchOption.AllDirectories)
        {
            return EnumerateFiles(dir, searchPattern, searchOption).Select(x => x.Replace(dir, ""));
        }

        public static IEnumerable<string> EnumerateDirectoriesRelative(
            string dir, string searchPattern, SearchOption searchOption = SearchOption.AllDirectories)
        {
            return EnumerateDirectories(dir, searchPattern, searchOption).Select(x => x.Replace(dir, ""));
        }

        public static IEnumerable<string> EnumerateFilesAndDirectoriesRelative(
            string dir, string searchPattern, SearchOption searchOption = SearchOption.AllDirectories)
        {
            return EnumerateFilesAndDirectories(dir, searchPattern, searchOption).Select(x => x.Replace(dir, ""));
        }

        public static IEnumerable<string> EnumerateFilesRelativeWithoutExt(
            string dir, string searchPattern, SearchOption searchOption = SearchOption.AllDirectories)
        {
            return EnumerateFilesRelative(dir, searchPattern, searchOption).Select(x => x.Replace(Path.GetExtension(x), "")).Select(x => x.Replace(dir, ""));
        }

        public static IEnumerable<string> EnumerateFilesFull(
            string dir, string searchPattern, SearchOption searchOption = SearchOption.AllDirectories)
        {
            return EnumerateFiles(dir, searchPattern, searchOption).Select(x => GetFullPath(x).Replace("\\", "/"));
        }

        public static IEnumerable<string> EnumerateDirectoriesFull(
            string dir, string searchPattern, SearchOption searchOption = SearchOption.AllDirectories)
        {
            return EnumerateDirectories(dir, searchPattern, searchOption).Select(x => GetFullPath(x).Replace("\\", "/"));
        }

        public static IEnumerable<string> EnumerateFilesAndDirectoriesFull(
            string dir, string searchPattern, SearchOption searchOption = SearchOption.AllDirectories)
        {
            return EnumerateFilesAndDirectories(dir, searchPattern, searchOption).Select(x => GetFullPath(x).Replace("\\", "/"));
        }

        public static void Copy(string source, string dest, bool overwrite)
        {
            File.Copy(source, dest, overwrite);
        }

        public static void CopyDirectory(string source, string dest, bool overwrite)
        {
            if (!Directory.Exists(dest))
            {
                Directory.CreateDirectory(dest);
            }

            foreach (var file in Directory.GetFiles(source))
            {
                File.Copy(file, Path.Combine(dest, Path.GetFileName(file)), overwrite);
            }

            foreach (var directory in Directory.GetDirectories(source))
            {
                CopyDirectory(directory, Path.Combine(dest, Path.GetFileName(directory)), overwrite);
            }
        }

        public static bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        public static void CreateDirectory(string path, bool hidden)
        {
            var directory = Directory.CreateDirectory(path);

            if (hidden)
            {
                directory.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            }
        }

        public static void DeleteDirectory(string path)
        {
            Directory.Delete(path, true);
        }

        public static string ReadFile(string path)
        {
            return File.ReadAllText(path);
        }

        public static string[] ReadFileLines(string path)
        {
            return File.ReadAllLines(path);
        }

        public static bool IsPathRooted(string path)
        {
            return Path.IsPathRooted(path);
        }

        public static string CurrentDirectory
        {
            get { return System.Environment.CurrentDirectory; }
            set { System.Environment.CurrentDirectory = value; }
        }

        public static string NewLine
        {
            get { return System.Environment.NewLine; }
        }

        public static DateTime GetLastWriteTime(string file)
        {
            return File.GetLastWriteTime(file);
        }

        public static void Move(string source, string dest)
        {
            File.Move(source, dest);
        }

        public static void MoveDirectory(string source, string dest)
        {
            Directory.Move(source, dest);
        }

        public static bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public static void FileDelete(string path)
        {
            File.Delete(path);
        }

        public static IEnumerable<string> SplitLines(string value)
        {
            return value.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        }

        public static void WriteToFile(string path, string text)
        {
            File.WriteAllText(path, text);
        }

        public static Stream CreateFileStream(string filePath, FileMode mode)
        {
            return new FileStream(filePath, mode);
        }

        public static void WriteAllBytes(string filePath, byte[] bytes)
        {
            File.WriteAllBytes(filePath, bytes);
        }

        public static string GetWorkingDirectory(string path)
        {
            if (path.IsNullOrWhiteSpace())
            {
                return CurrentDirectory;
            }

            var realPath = GetFullPath(path);

            if (FileExists(realPath) || DirectoryExists(realPath))
            {
                if ((File.GetAttributes(realPath) & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    return realPath;
                }

                return Path.GetDirectoryName(realPath);
            }

            return Path.GetDirectoryName(realPath);
        }

        public static string GetFullPath(string path)
        {
            return Path.GetFullPath(path);
        }

        public static string GetRelativeWorkingDirectory(string path)
        {
            if (path.IsNullOrWhiteSpace())
            {
                return path;
            }

            if (FileExists(path) || DirectoryExists(path))
            {
                if ((File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    return path;
                }

                return Path.GetDirectoryName(path);
            }

            return Path.GetDirectoryName(path);
        }

        public static string[] GetNodes(string path)
        {
            var dir = GetRelativeWorkingDirectory(path);
            return dir.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public static string WorkingDirectory
        {
            get { return AppDomain.CurrentDomain.BaseDirectory; }
        }
    }
}


