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

        private void PenddingJob(Job job)
        {
            allJobs.Add(job);

            if (!StartJob(job))
            {
                job.Pendding();
                penddingQueue.Enqueue(job);
            }
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
                if (job.IsFailed)
                {
                    failedJobs.Enqueue(job);
                }

                Task nt;
                tasks.TryRemove(job, out nt);

                semaphore.Release();

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
                if (!failedJobs.TryDequeue(out job))
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

        public ConcurrentBag<Job> AllJobs { get { return allJobs; } }
        public ConcurrentQueue<Job> PenddingQueue { get { return penddingQueue; } }
        public ConcurrentQueue<Job> FailedJobs { get { return failedJobs; } }
        public ConcurrentDictionary<Job, Task> AllTasks { get { return tasks; } }
    }
}
