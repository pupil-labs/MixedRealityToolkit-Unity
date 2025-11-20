using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace PupilLabs
{
    public class CsvLogger : Disposable
    {
        private StringBuilder activeSb;
        private StringBuilder toWriteSb;
        private readonly string path;
        private int flushThreshold;
        private volatile bool canWrite = true;
        private readonly object swapLock = new object();


        public CsvLogger(string path, int bufferCapacity = 16384)
        {
            this.path = path;
            flushThreshold = bufferCapacity;
            activeSb = new StringBuilder(bufferCapacity, bufferCapacity << 1);
            toWriteSb = new StringBuilder(bufferCapacity, bufferCapacity << 1);
        }

        public void EnqueueRow(string row)
        {
            string toWrite = null;
            lock (swapLock)
            {
                if (activeSb.Length > flushThreshold && canWrite)
                {
                    canWrite = false;
                    (activeSb, toWriteSb) = (toWriteSb, activeSb);
                    toWrite = toWriteSb.ToString();
                    toWriteSb.Clear();
                }
                activeSb.Append(row);
            }
            if (toWrite != null)
            {
                Task.Run(() =>
                {
                    File.AppendAllText(path, toWrite);
                    canWrite = true;
                }).Forget();
            }
        }
        protected override void DisposeUnmanagedResources()
        {
            string toWrite = null;
            lock (swapLock)
            {
                if (canWrite && activeSb.Length > 0)
                {
                    canWrite = false;
                    toWrite = activeSb.ToString();
                }
            }
            if (toWrite != null)
            {
                File.AppendAllText(path, toWrite);
            }
        }
    }
}
