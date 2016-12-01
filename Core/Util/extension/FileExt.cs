using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace socket4net
{
    /// <summary>
    ///     extension for file system
    /// </summary>
    public static class FileSys
    {
        /// <summary>
        ///     read as text
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string Read(string path)
        {
            try
            {
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var br = new StreamReader(fs, Encoding.UTF8))
                {
                    var bytes = br.ReadToEnd();
                    return bytes;
                }
            }
            catch (Exception e)
            {
                Logger.Ins.Error("{0}:{1}", e.Message, e.StackTrace);
                return string.Empty;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="searchPattern"></param>
        /// <param name="searchOption"></param>
        /// <returns></returns>
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

        /// <summary>
        ///     枚举所有文件夹
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="searchPattern"></param>
        /// <param name="searchOption"></param>
        /// <returns></returns>
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

        /// <summary>
        ///     枚举所有文件和文件夹
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="searchPattern"></param>
        /// <param name="searchOption"></param>
        /// <returns></returns>
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

        /// <summary>
        ///     枚举文件（以相对路径）
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="searchPattern"></param>
        /// <param name="searchOption"></param>
        /// <returns></returns>
        public static IEnumerable<string> EnumerateFilesRelative(
            string dir, string searchPattern, SearchOption searchOption = SearchOption.AllDirectories)
        {
            return EnumerateFiles(dir, searchPattern, searchOption).Select(x => x.Replace(dir, ""));
        }

        /// <summary>
        ///     枚举文件夹（以相对路径）
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="searchPattern"></param>
        /// <param name="searchOption"></param>
        /// <returns></returns>
        public static IEnumerable<string> EnumerateDirectoriesRelative(
            string dir, string searchPattern, SearchOption searchOption = SearchOption.AllDirectories)
        {
            return EnumerateDirectories(dir, searchPattern, searchOption).Select(x => x.Replace(dir, ""));
        }

        /// <summary>
        ///     枚举文件和文件夹
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="searchPattern"></param>
        /// <param name="searchOption"></param>
        /// <returns></returns>
        public static IEnumerable<string> EnumerateFilesAndDirectoriesRelative(
            string dir, string searchPattern, SearchOption searchOption = SearchOption.AllDirectories)
        {
            return EnumerateFilesAndDirectories(dir, searchPattern, searchOption).Select(x => x.Replace(dir, ""));
        }

        /// <summary>
        ///     枚举文件（剔除扩展名）
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="searchPattern"></param>
        /// <param name="searchOption"></param>
        /// <returns></returns>
        public static IEnumerable<string> EnumerateFilesRelativeWithoutExt(
            string dir, string searchPattern, SearchOption searchOption = SearchOption.AllDirectories)
        {
            return EnumerateFilesRelative(dir, searchPattern, searchOption).Select(x => x.Replace(Path.GetExtension(x), "")).Select(x => x.Replace(dir, ""));
        }

        /// <summary>
        ///     枚举文件（全路径）
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="searchPattern"></param>
        /// <param name="searchOption"></param>
        /// <returns></returns>
        public static IEnumerable<string> EnumerateFilesFull(
            string dir, string searchPattern, SearchOption searchOption = SearchOption.AllDirectories)
        {
            return EnumerateFiles(dir, searchPattern, searchOption).Select(x => GetFullPath(x).Replace("\\", "/"));
        }

        /// <summary>
        ///     枚举文件夹（全路径）
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="searchPattern"></param>
        /// <param name="searchOption"></param>
        /// <returns></returns>
        public static IEnumerable<string> EnumerateDirectoriesFull(
            string dir, string searchPattern, SearchOption searchOption = SearchOption.AllDirectories)
        {
            return EnumerateDirectories(dir, searchPattern, searchOption).Select(x => GetFullPath(x).Replace("\\", "/"));
        }

        /// <summary>
        ///     枚举文件和文件夹（全路径）
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="searchPattern"></param>
        /// <param name="searchOption"></param>
        /// <returns></returns>
        public static IEnumerable<string> EnumerateFilesAndDirectoriesFull(
            string dir, string searchPattern, SearchOption searchOption = SearchOption.AllDirectories)
        {
            return EnumerateFilesAndDirectories(dir, searchPattern, searchOption).Select(x => GetFullPath(x).Replace("\\", "/"));
        }

        /// <summary>
        ///     拷贝文件
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        /// <param name="overwrite"></param>
        public static void Copy(string source, string dest, bool overwrite)
        {
            File.Copy(source, dest, overwrite);
        }

        /// <summary>
        ///     拷贝路径
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        /// <param name="overwrite"></param>
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

        /// <summary>
        ///     是否存在文件夹路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool DirectoryExists(string path) => Directory.Exists(path);

        /// <summary>
        ///     创建文件夹
        /// </summary>
        /// <param name="path"></param>
        /// <param name="hidden"></param>
        public static void CreateDirectory(string path, bool hidden)
        {
            var directory = Directory.CreateDirectory(path);

            if (hidden)
            {
                directory.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            }
        }

        /// <summary>
        ///     删除文件夹（递归删除所有子文件夹）
        /// </summary>
        /// <param name="path"></param>
        public static void DeleteDirectory(string path)
        {
            Directory.Delete(path, true);
        }

        /// <summary>
        ///     读文件为文本
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string ReadFile(string path)
        {
            return File.ReadAllText(path);
        }

        /// <summary>
        ///     读文件为字符列表
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string[] ReadFileLines(string path)
        {
            return File.ReadAllLines(path);
        }

        /// <summary>
        ///     是否为根路径（如 D://, /home/）
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsPathRooted(string path)
        {
            return Path.IsPathRooted(path);
        }

        /// <summary>
        ///     当前工作路径
        /// </summary>
        public static string CurrentDirectory
        {
            get { return System.Environment.CurrentDirectory; }
            set { System.Environment.CurrentDirectory = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public static string NewLine => System.Environment.NewLine;

        /// <summary>
        ///     获取文件最近修改时间
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static DateTime GetLastWriteTime(string file)
        {
            return File.GetLastWriteTime(file);
        }

        /// <summary>
        ///     移动文件
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        public static void Move(string source, string dest)
        {
            File.Move(source, dest);
        }

        /// <summary>
        ///     移动文件夹
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        public static void MoveDirectory(string source, string dest)
        {
            Directory.Move(source, dest);
        }

        /// <summary>
        ///     文件是否存在
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool FileExists(string path)
        {
            return File.Exists(path);
        }

        /// <summary>
        ///     删除文件
        /// </summary>
        /// <param name="path"></param>
        public static void FileDelete(string path)
        {
            File.Delete(path);
        }

        /// <summary>
        ///     按照换行符拆分字符串为字符数组
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static IEnumerable<string> SplitLines(string value)
        {
            return value.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        }

        /// <summary>
        ///     写文本至文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="text"></param>
        public static void WriteToFile(string path, string text)
        {
            File.WriteAllText(path, text);
        }

        /// <summary>
        ///     创建文件流
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static Stream CreateFileStream(string filePath, FileMode mode)
        {
            return new FileStream(filePath, mode);
        }

        /// <summary>
        ///     写二进制数据至文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="bytes"></param>
        public static void WriteAllBytes(string filePath, byte[] bytes)
        {
            File.WriteAllBytes(filePath, bytes);
        }

        /// <summary>
        ///     获取路径的工作路径（文件？返回父目录 ：返回路径）
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
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

        /// <summary>
        ///     获取全路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetFullPath(string path)
        {
            return Path.GetFullPath(path);
        }

        /// <summary>
        ///     获取相对于指定路径的相对路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
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

        /// <summary>
        ///     获取路径中的所有节点
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string[] GetNodes(string path)
        {
            var dir = GetRelativeWorkingDirectory(path);
            return dir.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        ///     
        /// </summary>
        public static string WorkingDirectory => AppDomain.CurrentDomain.BaseDirectory;
    }
}


