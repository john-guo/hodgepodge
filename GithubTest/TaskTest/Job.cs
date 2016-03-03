using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskTest
{
    public enum JobStatus { Pendding, Running, Done }

    public sealed class Job
    {
        public JobStatus Status { get; private set; }
        public DateTime CreateTime { get; private set; }
        public DateTime StartTime { get; private set; }
        public DateTime FinishTime { get; private set; }

        public Action JobAction { get; private set; }

        public Exception exception { get; private set; }

        public event Action JobStart = delegate { };
        public event Action JobFinish = delegate { };

        private Job()
        {
            CreateTime = DateTime.Now;
            Status = JobStatus.Pendding;
            exception = null;
        }

        public Job(Action action) : this()
        {
            JobAction = delegate
            {
                JobStart();

                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
                FinishTime = DateTime.Now;
                Status = JobStatus.Done;

                JobFinish();
            };
        }

        public Task Start()
        {
            StartTime = DateTime.Now;
            Status = JobStatus.Running;
            return new Task(JobAction);
        }

        public bool IsFailed
        {
            get
            {
                return exception != null;
            }
        }
    }
}
