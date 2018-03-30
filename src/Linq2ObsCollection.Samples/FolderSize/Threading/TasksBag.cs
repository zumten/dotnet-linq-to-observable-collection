using System;
using System.Collections.Concurrent;
using System.Threading;

namespace ZumtenSoft.Linq2ObsCollection.Samples.FolderSize.Threading
{
    public class TasksBag
    {
        private readonly int _nbThreads;
        private readonly ConcurrentBag<Action> _queue = new ConcurrentBag<Action>();
        private readonly Semaphore _semaphore;
        private int _nbThreadsRunning;

        public TasksBag(int nbThreads)
        {
            _nbThreads = nbThreads;
            _semaphore = new Semaphore(nbThreads, nbThreads);
        }

        public void Push(Action task)
        {
            _queue.Add(task);
            if (_semaphore.WaitOne(0))
            {
                Interlocked.Increment(ref _nbThreadsRunning);
                ThreadPool.QueueUserWorkItem(StartProcessing);
            }
        }

        public void Clear()
        {
            int nbThreadsLeft = _nbThreads;
            while (nbThreadsLeft > 0)
            {
                Action action;
                while (_queue.TryTake(out action)) { }

                while (_semaphore.WaitOne(0))
                    nbThreadsLeft--;
            }

            for (int i = 0; i < _nbThreads; i++)
                _semaphore.Release();
        }

        private void StartProcessing(object state)
        {
            do
            {
                // Run actions as long as there are items
                Action task;
                while (_queue.TryTake(out task))
                    task();

                // There is a possibility an item has been queued here.
                // If the queue is not empty, we have to retry processing the queue just to be safe.

                _semaphore.Release();

            } while (!_queue.IsEmpty && _semaphore.WaitOne(0));
            int nbThreadsRunning = Interlocked.Decrement(ref _nbThreadsRunning);

            if (nbThreadsRunning == 0 && Completed != null)
                Completed(this, new EventArgs());
        }

        public event EventHandler Completed;
    }
}
