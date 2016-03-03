using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;

namespace TaskTest
{
    public class JobScheduler : IDisposable
    {
        private ConcurrentBag<Job> allJobs;
        private ConcurrentBag<Job> penddingQueue;
        private ConcurrentDictionary<Job, Task> tasks;
        private ConcurrentBag<Job> failedJobs;

        private readonly int maxRunningCount;
        private readonly object addJobLock = new object();
        private const int defaultMaxRunningCount = 10;


        public JobScheduler() :
            this(defaultMaxRunningCount)
        {

        }

        public JobScheduler(int maxRunning)
        {
            allJobs = new ConcurrentBag<Job>();
            penddingQueue = new ConcurrentBag<Job>();
            tasks = new ConcurrentDictionary<Job, Task>();
            failedJobs = new ConcurrentBag<Job>();
            maxRunningCount = maxRunning;
        }


        private bool CanStart
        {
            get
            {
                return tasks.Count < maxRunningCount;
            }
        }

        private void Schedule()
        {
            if (penddingQueue.IsEmpty)
                return;

            Job job;
            if (penddingQueue.TryTake(out job))
            {
                SpinWait.SpinUntil(() => StartJob(job));
            }
        }

        private void PenddingJob(Job job)
        {
            if (CanStart)
            {
                StartJob(job);
            }
            else
            {
                penddingQueue.Add(job);
            }
        }

        public Job PenddingJob(Action action)
        {
            var job = new Job(action);
            allJobs.Add(job);

            PenddingJob(job);

            return job;
        }

        public Job PenddingJob(Action action, Action start, Action finish)
        {
            var job = new Job(action);
            job.JobStart += start;
            job.JobFinish += finish;

            allJobs.Add(job);

            if (CanStart)
            {
                StartJob(job);
            }
            else
            {
                penddingQueue.Add(job);
            }

            return job;
        }

        private bool StartJob(Job job)
        {
            Task task;

            if (!CanStart)
                return false;
            lock (addJobLock)
            {
                if (!CanStart)
                    return false;
                task = job.Start();
                tasks.TryAdd(job, task);
            }

            task.ContinueWith(t =>
            {
                if (job.IsFailed)
                {
                    failedJobs.Add(job);
                }

                Task nt;
                tasks.TryRemove(job, out nt);
                Schedule();
            });
            task.Start();
            return true;
        }

        public async Task WaitAll()
        {
            while (!IsEmpty)
            {
                await Task.Yield();
            }
        }

        public bool IsEmpty
        {
            get
            {
                return tasks.IsEmpty && penddingQueue.IsEmpty;
            }
        }

        public void Retry()
        {
            while (!failedJobs.IsEmpty)
            {
                Job job;
                if (!failedJobs.TryTake(out job))
                    continue;

                PenddingJob(job);
            }
        }

        public bool HasFailedJobs
        {
            get
            {
                return !failedJobs.IsEmpty;
            }
        }

        public void Dispose()
        {
            WaitAll().Wait();
        }
    }
}
