using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;

namespace ZumtenSoft.WpfUtils.Collections
{
    /// <summary>
    /// Allows to modify the values of each items in the same way Enumerable.Select works, but with the
    /// ability to observe it.
    /// </summary>
    /// <typeparam name="TSource">Type of the source collection items</typeparam>
    /// <typeparam name="T">Type of the collection items</typeparam>
    [DebuggerDisplay(@"\{ObservatorCollection Count={Count}\}")]
    public sealed class SelectionObservatorCollection<TSource, T> : BaseObservatorCollection<T>
    {
        private IObservableCollection<TSource> _source;
        public IObservableCollection<TSource> Source
        {
            get { return _source; }
            set
            {
                value = value ?? ReadOnlyObservableCollection<TSource>.Empty;
                if (_source != value)
                {
                    if (_items != null)
                    {
                        _source.CollectionChanged -= SourceOnCollectionChanged;
                        _source = value;
                        Reset();
                        _source.CollectionChanged += SourceOnCollectionChanged;
                    }
                    else
                    {
                        _source = value;
                    }
                }
            }
        }

        private Func<TSource, T> _selector;
        public Func<TSource, T> Selector
        {
            get { return _selector; }
            set
            {
                if (_selector != value)
                {
                    _selector = value;
                    if (_items != null)
                        Reset();
                }
            }
        }
        private List<T> _items; 

        public SelectionObservatorCollection(IObservableCollection<TSource> source, Func<TSource, T> selector)
        {
            _source = source ?? ReadOnlyObservableCollection<TSource>.Empty;
            _selector = selector;
        }

        /// <summary>
        /// Initializes the collection if it not already initialized (in a lazy-loading fashion)
        /// </summary>
        internal void TryInitialize()
        {
            if (_items == null)
            {
                _items = new List<T>(((ICollection<TSource>)_source).Select(_selector));
                _source.CollectionChanged += SourceOnCollectionChanged;
            }
        }

        /// <summary>
        /// Resets the collection. Used when all the observators are removed, allowing to possibly clean
        /// the collection. If a new call is made, the collection will be re-initialized.
        /// </summary>
        private void TryDispose() 
        {
            if (_items != null)
            {
                foreach (IDisposable item in _items.OfType<IDisposable>())
                    item.Dispose();

                _items = null;
                _source.CollectionChanged -= SourceOnCollectionChanged;
            }
        }

        private void Reset()
        {
            foreach (IDisposable item in _items.OfType<IDisposable>())
                item.Dispose();

            _items = new List<T>(((ICollection<TSource>) _source).Select(_selector));

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            OnPropertyChanged("Count");
        }

        private void SourceOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            List<T> itemsToDispose = null;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        List<T> newItems = e.NewItems.Cast<TSource>().Select(_selector).ToList();
                        _items.InsertRange(e.NewStartingIndex, newItems);
                        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItems, e.NewStartingIndex));
                        OnPropertyChanged("Count");
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    {
                        List<T> oldItems = itemsToDispose = _items.GetRange(e.OldStartingIndex, e.OldItems.Count);
                        _items.RemoveRange(e.OldStartingIndex, e.OldItems.Count);

                        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItems, e.OldStartingIndex));
                        OnPropertyChanged("Count");

                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    {
                        List<T> oldItems = itemsToDispose = _items.GetRange(e.OldStartingIndex, e.OldItems.Count);
                        _items.RemoveRange(e.OldStartingIndex, e.OldItems.Count);
                        List<T> newItems = e.NewItems.Cast<TSource>().Select(_selector).ToList();
                        _items.InsertRange(e.NewStartingIndex, newItems);

                        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItems, oldItems, e.NewStartingIndex));
                        if (e.OldItems.Count != e.NewItems.Count)
                            OnPropertyChanged("Count");
                    }
                    break;

                case NotifyCollectionChangedAction.Move:
                    {
                        List<T> items = _items.GetRange(e.OldStartingIndex, e.OldItems.Count);
                        _items.RemoveRange(e.OldStartingIndex, e.OldItems.Count);
                        _items.InsertRange(e.NewStartingIndex, items);

                        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, items, e.NewStartingIndex, e.OldStartingIndex));
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    Reset();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            // If there are removed item, we have to dispose them
            if (itemsToDispose != null)
                foreach (IDisposable item in itemsToDispose.OfType<IDisposable>())
                    item.Dispose();
        }

        #region BaseObservatorCollection<T> Implementation

        protected override void OnObservatorsChanged(bool hasCollectionChanged, bool hasPropertyChanged)
        {
            // If there are new observers, make sure it is initialized.
            if (hasCollectionChanged || hasPropertyChanged)
            {
                TryInitialize();
            }
            else
            {
                TryDispose();
            }
        }

        public override IEnumerator<T> GetEnumerator()
        {
            TryInitialize();
            return _items.GetEnumerator();
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
            get { return _source.Count; }
        }
        
        public override int IndexOf(T item)
        {
            TryInitialize();
            return _items.IndexOf(item);
        }

        public override T this[int index]
        {
            get
            {
                TryInitialize();
                return _items[index];
            }
            set { throw new NotSupportedException(); }
        }

        #endregion BaseObservatorCollection<T> Implementation
    }
}