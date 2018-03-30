using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Windows.Threading;

namespace ZumtenSoft.Linq2ObsCollection.Threading
{
    /// <summary>
    /// This class is used, in conjunction with DispatchingObservatorCollection, to allow models to be updated freely
    /// on the processing threads, without breaking the possibility of observing the objects from the UI thread.
    /// </summary>
    public class DispatcherQueue
    {
        /// <summary>
        /// Dispatcher associated with a specific thread (usually the UI thread).
        /// </summary>
        public Dispatcher Dispatcher { get; private set; }

        /// <summary>
        /// Thread priority used when starting to process the queue.
        /// </summary>
        public DispatcherPriority Priority { get; private set; }

        /// <summary>
        /// Maximum duration of a processing done on the dispatcher thread.
        /// </summary>
        public TimeSpan TimeLimit { get; private set; }
        private readonly ConcurrentQueue<Action> _actionsQueue = new ConcurrentQueue<Action>();

        /// <summary>
        /// Allows to track if the processing has already been queued.
        /// </summary>
        private readonly Semaphore _isSyncing = new Semaphore(1, 1);

        public DispatcherQueue(Dispatcher dispatcher, DispatcherPriority priority = DispatcherPriority.Normal) : this(dispatcher, priority, TimeSpan.FromSeconds(0.1))
        {

        }

        public DispatcherQueue(Dispatcher dispatcher, DispatcherPriority priority, TimeSpan timeLimit)
        {
            Dispatcher = dispatcher;
            Priority = priority;
            TimeLimit = timeLimit;
        }

        public void Push(Action action)
        {
            _actionsQueue.Enqueue(action);

            // Invoke the processing on the dispatcher queue, in case it is not already invoked.
            if (_isSyncing.WaitOne(0))
                Dispatcher.BeginInvoke(Priority, new Action(RaiseChangedEvent));
        }

        private void RaiseChangedEvent()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            try
            {
                // Process the whole queue on the Dispatcher thread
                Action action;
                while (watch.Elapsed < TimeLimit && _actionsQueue.TryDequeue(out action))
                    action();
            }
            finally
            {
                // If the time has been elapsed, we need to queue a new processing
                if (!_actionsQueue.IsEmpty)
                {
                    Dispatcher.BeginInvoke(Priority, new Action(RaiseChangedEvent));
                }
                else
                {
                    _isSyncing.Release();
                    // There is a possibility an item has been queued here. We have to verify if the queue is still empty;
                    if (!_actionsQueue.IsEmpty && _isSyncing.WaitOne(0))
                        Dispatcher.BeginInvoke(Priority, new Action(RaiseChangedEvent));
                }
            }
        }
    }
}
