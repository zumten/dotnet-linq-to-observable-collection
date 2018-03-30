using System;
using System.Collections.Concurrent;
using System.Threading;

namespace ZumtenSoft.WpfUtils.Samples.FolderSize.Threading
{
    public class TasksQueue
    {
        private readonly ConcurrentQueue<Action> _queue = new ConcurrentQueue<Action>();
        private readonly Semaphore _semaphore;
        private int _nbThreadsRunning;

        public TasksQueue(int nbThreads)
        {
            _semaphore = new Semaphore(nbThreads, nbThreads);
        }

        public void Push(Action task)
        {
            _queue.Enqueue(task);
            if (_semaphore.WaitOne(0))
            {
                Interlocked.Increment(ref _nbThreadsRunning);
                ThreadPool.QueueUserWorkItem(StartProcessing);
            }
        }

        private void StartProcessing(object state)
        {
            do
            {
                // Run actions as long as there are items
                Action task;
                while (_queue.TryDequeue(out task))
                    task();

                // There is a possibility an item has been queued here.
                // If the queue is not empty, we have to retry processing the queue just to be safe.

                _semaphore.Release();

            } while (!_queue.IsEmpty && _semaphore.WaitOne(0));
        }
    }
}
