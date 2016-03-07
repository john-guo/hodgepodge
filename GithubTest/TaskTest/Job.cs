using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TaskTest
{
    public enum JobStatus { Pendding, PrepareToRun, Running, PrepareToStop, Stop, Fail, Done }

    public delegate void JobDelegate(Job sender);

    public sealed class Job
    {
        public JobStatus Status { get; private set; }
        public DateTime CreateTime { get; private set; }
        public DateTime StartTime { get; private set; }
        public DateTime FinishTime { get; private set; }
        public DateTime StopTime { get; private set; }

        internal JobDelegate JobAction { get; private set; }

        public Exception LastException { get; private set; }

        internal event JobDelegate JobStart = delegate { };
        internal event JobDelegate JobFinish = delegate { };
        internal event JobDelegate JobFail = delegate { };
        internal event JobDelegate JobStop = delegate { };

        private object _tag;
        private CancellationTokenSource _tokenSource;
        private object _savePoint;

        private Job()
        {
            CreateTime = DateTime.Now;
            Status = JobStatus.Pendding;
            LastException = null;
            _tag = null;
        }

        internal Job(JobDelegate action) : this()
        {
            JobAction = job =>
            {
                job.JobStart(job);
                try
                {
                    action(job);
                }
                catch (OperationCanceledException ex)
                {
                    job.LastException = ex;
                    job.Stop();
                    return;
                }
                catch (Exception ex)
                {
                    job.LastException = ex;
                    job.Status = JobStatus.Fail;
                    job.StopTime = DateTime.Now;
                    job.JobFail(job);
                    return;
                }
                job.FinishTime = DateTime.Now;
                job.Status = JobStatus.Done;
                job.JobFinish(job);
            };
        }

        internal void Prepare()
        {
            Status = JobStatus.PrepareToRun;
        }

        internal void Pendding()
        {
            Status = JobStatus.Pendding;
        }

        internal Task Start()
        {
            StartTime = DateTime.Now;
            Status = JobStatus.Running;
            _tokenSource = new CancellationTokenSource();

            return new Task(o => JobAction((Job)o), this, _tokenSource.Token);
        }

        internal void Stop()
        {
            Status = JobStatus.Stop;
            StopTime = DateTime.Now;
            JobStop(this);
        }

        internal void PrepareStop()
        {
            Status = JobStatus.PrepareToStop;
            _tokenSource.Cancel();
        }

        public void CancelProcess(Func<object> savePointGetter)
        {
            if (Status != JobStatus.PrepareToStop)
                return;

            _savePoint = savePointGetter();
            _tokenSource.Token.ThrowIfCancellationRequested();
        }

        internal bool IsStop
        {
            get
            {
                return Status == JobStatus.Stop;
            }
        }

        internal bool IsFailed
        {
            get
            {
                return Status == JobStatus.Fail;
            }
        }

        public void SetTag<T>(T tag)
        {
            _tag = tag;
        }

        public T GetTag<T>()
        {
            return (T)_tag;
        }

        public T GetSavePoint<T>() where T : class
        {
            return _savePoint as T;
        }
    }
}
