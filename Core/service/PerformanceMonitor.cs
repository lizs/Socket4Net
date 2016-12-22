using System.Threading;

namespace socket4net
{
    /// <summary>
    ///     performance monitor
    /// </summary>
    public class PerformanceMonitor : Obj
    {
        /// <summary>
        ///     get performance monitor singlton instance
        /// </summary>
        public static PerformanceMonitor Ins => GlobalVarPool.Ins.Monitor;

        private int _refreshPeriod = 1;
        /// <summary>
        ///     refresh record every "RefreshPeriod" second
        /// </summary>
        public int RefreshPeriod
        {
            get { return _refreshPeriod; }
            set
            {
                _refreshPeriod = value;
                InvokeRepeating(Refresh, 0, (uint)_refreshPeriod * 1000);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int Connections;
        /// <summary>
        ///     excuted sta service jobs count per second
        /// </summary>
        public int ExcutedJobsPerSec;
        /// <summary>
        /// 
        /// </summary>
        public int ReadBytesPerSec;
        /// <summary>
        /// 
        /// </summary>
        public int WriteBytesPerSec;
        /// <summary>
        /// 
        /// </summary>
        public int ReadPackagesPerSec;
        /// <summary>
        /// 
        /// </summary>
        public int WritePackagesPerSec;
        
        /// <summary>
        ///     Invoked when obj started
        /// </summary>
        protected override void OnStart()
        {
            base.OnStart();
            InvokeRepeating(Refresh, 0, (uint)_refreshPeriod * 1000);
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

        /// <summary>
        /// 
        /// </summary>
        public void RecordConnection(int cnt)
        {
            Interlocked.Add(ref Connections, cnt);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="len"></param>
        /// <param name="packages"></param>
        public void RecordWrite(int len, int packages = 1)
        {
            Interlocked.Add(ref WriteBytesPerSec, len);
            Interlocked.Add(ref WritePackagesPerSec, packages);
        }

        /// <summary>
        /// 
        /// </summary>
        public void RecordJob()
        {
            Interlocked.Increment(ref ExcutedJobsPerSec);
        }

        private void Refresh()
        {
            Logger.Ins.Debug(
                $"Conections : {Connections}" +
                $"\tJobs : {ExcutedJobsPerSec/_refreshPeriod}/s" +
                $"\tJobs pending: {GlobalVarPool.Ins.Service.Jobs}" +
                $"\tWriteBytes : {WriteBytesPerSec/(_refreshPeriod*1024)}KB/s" +
                $"\tWritePackages : {WritePackagesPerSec/_refreshPeriod}/s" +
                $"\tReadBytes : {ReadBytesPerSec/(_refreshPeriod*1024)}KB/s" +
                $"\tReadPackages : {ReadPackagesPerSec/_refreshPeriod}/s");

            Interlocked.Exchange(ref ExcutedJobsPerSec, 0);
            Interlocked.Exchange(ref WriteBytesPerSec, 0);
            Interlocked.Exchange(ref WritePackagesPerSec, 0);
            Interlocked.Exchange(ref ReadBytesPerSec, 0);
            Interlocked.Exchange(ref ReadPackagesPerSec, 0);
        }
    }
}