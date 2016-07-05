using System;
using System.IO;
using System.Text;

namespace socket4net
{
    /// <summary>
    /// 
    /// </summary>
    public static class FileExt
    {
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
    }
}
