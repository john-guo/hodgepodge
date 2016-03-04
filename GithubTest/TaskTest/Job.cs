using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskTest
{
    public enum JobStatus { Pendding, PrepareToRun, Running, Fail, Done }

    public delegate void JobDelegate(Job sender);

    public sealed class Job
    {
        public JobStatus Status { get; private set; }
        public DateTime CreateTime { get; private set; }
        public DateTime StartTime { get; private set; }
        public DateTime FinishTime { get; private set; }

        public JobDelegate JobAction { get; private set; }

        public Exception _exception { get; private set; }

        public event JobDelegate JobStart = delegate { };
        public event JobDelegate JobFinish = delegate { };
        public event JobDelegate JobFail = delegate { };

        private object _tag;

        private Job()
        {
            CreateTime = DateTime.Now;
            Status = JobStatus.Pendding;
            _exception = null;
            _tag = null;
        }

        public Job(JobDelegate action) : this()
        {
            JobAction = job =>
            {
                JobStart(job);
                try
                {
                    action(job);
                }
                catch (Exception ex)
                {
                    _exception = ex;
                    Status = JobStatus.Fail;
                    JobFail(job);
                    return;
                }
                FinishTime = DateTime.Now;
                Status = JobStatus.Done;
                JobFinish(job);
            };
        }

        public void Prepare()
        {
            Status = JobStatus.PrepareToRun;
        }

        public void Pendding()
        {
            Status = JobStatus.Pendding;
        }

        public Task Start()
        {
            StartTime = DateTime.Now;
            Status = JobStatus.Running;
            return new Task(o => JobAction((Job)o), this);
        }

        public bool IsFailed
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
    }
}
