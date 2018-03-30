using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using ZumtenSoft.WpfUtils.Threading;

namespace ZumtenSoft.WpfUtils.Collections
{
    /// <summary>
    /// Observable list ensuring any notification is fired on the dispatcher thread.
    /// This object is useful when a list of view model is observing a list
    /// of models, marking most of the job for transfering the events on the UI Thread.
    /// </summary>
    [DebuggerDisplay(@"\{ObservableSynchronizedCollection Count={Count}\}")]
    public class DispatchingObservatorCollection<T> : BaseObservatorCollection<T>
    {
        private readonly DispatcherQueue _dispatcherQueue;
        public IObservableCollection<T> Source { get; private set; }
        
        // Copy of the Source list. Has to be preserved to isolate the threads
        private List<T> _items;

        public DispatchingObservatorCollection(DispatcherQueue dispatcherQueue, IObservableCollection<T> source)
        {
            _dispatcherQueue = dispatcherQueue;
            Source = source;
        }

        /// <summary>
        /// Initializes the collection if it not already initialized (in a lazy-loading fashion)
        /// </summary>
        private void TryInitialize() 
        {
            if (_items == null)
            {
                _items = new List<T>(Source);
                Source.CollectionChanged += SourceOnCollectionChanged;
            }
        }

        /// <summary>
        /// Resets the collection. Used when all the observators are removed, allowing to possibly clean
        /// the collection. If a new call is made, the collection will be re-initialized.
        /// </summary>
        private void TryReset()
        {
            if (_items == null)
            {
                Source.CollectionChanged -= SourceOnCollectionChanged;
                _items = null;
            }
        }

        private void SourceOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Push the event to be executed on the dispatcher thread (in the other they were added to the queue)
            _dispatcherQueue.Push(() =>
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        {
                            _items.InsertRange(e.NewStartingIndex, e.NewItems.Cast<T>());
                            OnPropertyChanged("Count");
                        }
                        break;

                    case NotifyCollectionChangedAction.Remove:
                        {
                            _items.RemoveRange(e.OldStartingIndex, e.OldItems.Count);
                            OnPropertyChanged("Count");
                        }
                        break;

                    case NotifyCollectionChangedAction.Replace:
                        {
                            _items.RemoveRange(e.OldStartingIndex, e.OldItems.Count);
                            _items.InsertRange(e.NewStartingIndex, e.NewItems.Cast<T>());
                            if (e.OldItems.Count != e.NewItems.Count)
                                OnPropertyChanged("Count");
                        }
                        break;

                    case NotifyCollectionChangedAction.Move:
                        {
                            _items.RemoveRange(e.OldStartingIndex, e.OldItems.Count);
                            _items.InsertRange(e.NewStartingIndex, e.NewItems.Cast<T>());
                        }
                        break;

                    case NotifyCollectionChangedAction.Reset:
                        {
                            _items.Clear();
                            _items.AddRange(Source);

                            OnPropertyChanged("Count");
                        }
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
                OnCollectionChanged(e);
            });
        }

        #region BaseObservatorCollection<T> Implementation

        public override T this[int index]
        {
            get
            {
                TryInitialize();
                return _items[index];
            }
            set { throw new NotSupportedException(); }
        }

        public override IEnumerator<T> GetEnumerator()
        {
            TryInitialize();
            return _items.GetEnumerator();
        }

        protected override void OnObservatorsChanged(bool hasCollectionChanged, bool hasPropertyChanged)
        {
            // If all observers have been removed, we can think it is safe to dispose the collection.
            // If this was not the case, the collection will be reinitialized, which is fine.
            if (hasCollectionChanged || hasPropertyChanged)
            {
                TryInitialize();
            }
            else
            {
                TryReset();
            }
        }

        public override bool Contains(T item)
        {
            TryInitialize();
            return _items.Contains(item);
        }

        public override void CopyTo(T[] array, int arrayIndex)
        {
            TryInitialize();
            _items.CopyTo(array, arrayIndex);
        }

        public override int Count
        {
            get
            {
                TryInitialize();
                return _items.Count;
            }
        }

        public override int IndexOf(T item)
        {
            TryInitialize();
            return _items.IndexOf(item);
        }

        #endregion BaseObservatorCollection<T> Implementation
    }
}