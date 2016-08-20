using System.Threading;

namespace socket4net
{
    /// <summary>
    ///     performance monitor
    /// </summary>
    public class PerformanceMonitor : Obj
    {
        public static PerformanceMonitor Ins => GlobalVarPool.Ins.Monitor;

        public int ExcutedJobsPerSec;
        public int ReadBytesPerSec;
        public int WriteBytesPerSec;
        public int ReadPackagesPerSec;
        public int WritePackagesPerSec;

        protected override void OnStart()
        {
            base.OnStart();
            InvokeRepeating(Refresh, 0, 1000);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="len"></param>
        public void RecordRead(int len)
        {
            Interlocked.Add(ref ReadBytesPerSec, len);
            Interlocked.Increment(ref ReadPackagesPerSec);
        }

        public void RecordWrite(int len, int packages = 1)
        {
            Interlocked.Add(ref WriteBytesPerSec, len);
            Interlocked.Add(ref WritePackagesPerSec, packages);
        }

        public void RecordJob()
        {
            Interlocked.Increment(ref ExcutedJobsPerSec);
        }

        private void Refresh()
        {
            Logger.Ins.Debug(
                $"Jobs : {ExcutedJobsPerSec}/s\tWriteBytes : {WriteBytesPerSec/1024}KB/s\tWritePackages : {WritePackagesPerSec}/s\tReadBytes : {ReadBytesPerSec/1024}KB/s\tReadPackages : {ReadPackagesPerSec}/s");

            Interlocked.Exchange(ref ExcutedJobsPerSec, 0);
            Interlocked.Exchange(ref WriteBytesPerSec, 0);
            Interlocked.Exchange(ref WritePackagesPerSec, 0);
            Interlocked.Exchange(ref ReadBytesPerSec, 0);
            Interlocked.Exchange(ref ReadPackagesPerSec, 0);
        }
    }
}