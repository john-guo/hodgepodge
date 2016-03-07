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
        private ConcurrentQueue<Job> penddingQueue;
        private ConcurrentQueue<Job> failedJobs;
        private ConcurrentDictionary<Job, Job> stopJobs;
        private ConcurrentDictionary<Job, Task> tasks;
        private SemaphoreSlim semaphore;

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
            penddingQueue = new ConcurrentQueue<Job>();
            tasks = new ConcurrentDictionary<Job, Task>();
            failedJobs = new ConcurrentQueue<Job>();
            stopJobs = new ConcurrentDictionary<Job, Job>();

            maxRunningCount = maxRunning;
            semaphore = new SemaphoreSlim(maxRunningCount);
        }


        private bool CanStart
        {
            get
            {
                return semaphore.CurrentCount > 0;
            }
        }

        private void Schedule()
        {
            if (penddingQueue.IsEmpty)
                return;

            Job job;
            if (penddingQueue.TryDequeue(out job))
            {
                SpinWait.SpinUntil(() => StartJob(job));
            }
        }

        private void _StartJob(Job job)
        {
            if (StartJob(job))
                return;

            job.Pendding();
            penddingQueue.Enqueue(job);

        }

        private void PenddingJob(Job job)
        {
            allJobs.Add(job);

            _StartJob(job);
        }

        public Job PenddingJob(JobDelegate action)
        {
            var job = new Job(action);

            PenddingJob(job);

            return job;
        }

        public Job PenddingJob<T>(JobDelegate action, T tag)
        {
            var job = new Job(action);
            job.SetTag(tag);

            PenddingJob(job);

            return job;
        }

        public Job PenddingJob(
            JobDelegate action,
            JobDelegate start,
            JobDelegate finish,
            JobDelegate fail)
        {
            var job = new Job(action);
            job.JobStart += start;
            job.JobFinish += finish;
            job.JobFail += fail;

            PenddingJob(job);

            return job;
        }

        public Job PenddingJob<T>(
            JobDelegate action, T tag,
            JobDelegate start,
            JobDelegate finish,
            JobDelegate fail)
        {
            var job = new Job(action);
            job.SetTag(tag);
            job.JobStart += start;
            job.JobFinish += finish;
            job.JobFail += fail;

            PenddingJob(job);

            return job;
        }

        private bool StartJob(Job job)
        {
            job.Prepare();
            if (!semaphore.Wait(0))
                return false;
            var task = job.Start();
            tasks.TryAdd(job, task);

            task.ContinueWith(t =>
            {
                var j = t.AsyncState as Job;
                if (t.IsCanceled && !j.IsStop)
                {
                    j.Stop();
                }

                if (j.IsFailed)
                {
                    failedJobs.Enqueue(j);
                }
                else if (j.IsStop)
                {
                    stopJobs[j] = j;
                }

                Task nt;
                tasks.TryRemove(j, out nt);

                semaphore.Release();

                Schedule();
            });
            task.Start();

            return true;
        }

        public void StopJob(Job job)
        {
            if (job.Status != JobStatus.Running)
                return;

            job.PrepareStop();
        }

        public void RestartJob(Job job)
        {
            Job j;
            stopJobs.TryRemove(job, out j);

            if (job.Status != JobStatus.Stop)
                return;

            _StartJob(job);
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
                if (!failedJobs.TryDequeue(out job))
                    continue;

                _StartJob(job);
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

        public IEnumerable<Job> AllJobs { get { return allJobs.AsEnumerable(); } }
        public IEnumerable<Job> PenddingQueue { get { return penddingQueue.AsEnumerable(); } }
        public IEnumerable<Job> FailedJobs { get { return failedJobs.AsEnumerable(); } }
        public IEnumerable<Job> StopJobs { get { return stopJobs.Keys.AsEnumerable(); } }
        public IEnumerable<Job> AllTasks { get { return tasks.Keys.AsEnumerable(); } }
    }
}
