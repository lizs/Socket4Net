namespace socket4net
{
    /// <summary>
    ///     performance monitor
    /// </summary>
    public class PerformanceMonitor : Obj
    {
        public static PerformanceMonitor Ins => GlobalVarPool.Ins.Monitor;

        public int ExcutedJobsPerSec { get; private set; }
        public int ReadBytesPerSec { get; private set; }
        public int WriteBytesPerSec { get; private set; }
        public int ReadPackagesPerSec { get; private set; }
        public int WritePackagesPerSec { get; private set; }

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
            ReadBytesPerSec += len;
            ++ReadPackagesPerSec;
        }

        public void RecordWrite(int len)
        {
            WriteBytesPerSec += len;
            ++WritePackagesPerSec;
        }

        public void RecordJob()
        {
            ++ExcutedJobsPerSec;
        }

        private void Refresh()
        {
            Logger.Ins.Debug(
                $"Jobs : {ExcutedJobsPerSec}/s\tWriteBytes : {WriteBytesPerSec/1024}KB/s\tWritePackages : {WritePackagesPerSec}/s\tReadBytes : {ReadBytesPerSec/1024}KB/s\tReadPackages : {ReadPackagesPerSec}/s");

            ExcutedJobsPerSec = 0;
            WriteBytesPerSec = 0;
            WritePackagesPerSec = 0;
            ReadBytesPerSec = 0;
            ReadPackagesPerSec = 0;
        }
    }
}