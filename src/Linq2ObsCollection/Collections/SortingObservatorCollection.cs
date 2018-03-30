using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using ZumtenSoft.Linq2ObsCollection.Comparison;

namespace ZumtenSoft.Linq2ObsCollection.Collections
{
    /// <summary>
    /// Allows to sort items in the same way Enumerable.OrderBy works, but with the
    /// ability to observe it.
    /// </summary>
    /// <typeparam name="T">Type of the collection items</typeparam>
    public class SortingObservatorCollection<T> : BaseObservatorCollection<T>
    {
        private IObservableCollection<T> _source;
        public IObservableCollection<T> Source
        {
            get { return _source; }
            set
            {
                value = value ?? ReadOnlyObservableCollection<T>.Empty;
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

        private IComparer<T> _comparerWithFallback; 
        private IComparer<T> _comparer;
        public IComparer<T> Comparer
        {
            get { return _comparer; }
            set
            {
                if (_comparer != value)
                {
                    _comparer = value;
                    _comparerWithFallback = new MultiComparer<T>(_comparer, ReferenceComparer<T>.Comparer);

                    if (_items != null)
                    {
                        _items.Sort(_comparer);

                        ResetIndexes();
                        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    }
                }
            }
        }

        private ExtendedList<T> _items;
        private Dictionary<T, uint> _orderByItem;

        public SortingObservatorCollection(IObservableCollection<T> source, IComparer<T> comparer = null)
        {
            _source = source ?? ReadOnlyObservableCollection<T>.Empty;
            _comparer = comparer ?? Comparer<T>.Default;
            _comparerWithFallback = new MultiComparer<T>(_comparer, ReferenceComparer<T>.Comparer);
        }

        /// <summary>
        /// Initializes the collection if it not already initialized (in a lazy-loading fashion)
        /// </summary>
        internal void TryInitialize() 
        {
            if (_items == null)
            {
                _items = new ExtendedList<T>(_source);
                _items.Sort(_comparerWithFallback);

                IEqualityComparer<T> equalityComparer = ReferenceComparer<T>.EqualityComparer;
                _orderByItem = new Dictionary<T, uint>(_items.Count * 2, equalityComparer);
                ResetIndexes();

                foreach (INotifyPropertyChanged item in _items.OfType<INotifyPropertyChanged>())
                    item.PropertyChanged += ItemPropertyChanged;

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
                _items = null;
                _orderByItem = null;
                _source.CollectionChanged -= SourceOnCollectionChanged;
            }
        }

        private void Reset()
        {
            foreach (IDisposable item in _orderByItem.Keys.OfType<IDisposable>())
                item.Dispose();

            _items.Clear();
            _items.AddRange(_source);
            _items.Sort(_comparerWithFallback);

            ResetIndexes();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            OnPropertyChanged("Count");
        }

        private void ResetIndexes()
        {
            _orderByItem.Clear();
            uint partsLength = uint.MaxValue / ((uint)_items.Count + 1);
            for (int index = 0; index < _items.Count; index++)
            {
                _orderByItem.Add(_items[index], (1 + (uint)index) * partsLength);
            }
        }

        private void SourceOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (T item in e.NewItems)
                    {
                        int index = AddItem(item);
                        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
                    }
                    OnPropertyChanged("Count");
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (T item in e.OldItems)
                    {
                        int index = RemoveItem(item);
                        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
                    }
                    OnPropertyChanged("Count");
                    break;
                case NotifyCollectionChangedAction.Replace:
                    foreach (T item in e.OldItems)
                    {
                        int index = RemoveItem(item);
                        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
                    }
                    foreach (T item in e.NewItems)
                    {
                        int index = AddItem(item);
                        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
                    }
                    OnPropertyChanged("Count");
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    Reset();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private int AddItem(T item)
        {
            int newIndex = ~BinarySearch(item);
            int previousIndex = newIndex - 1;
            uint minReferenceIndex = (previousIndex < 0 ? 0 : _orderByItem[_items[previousIndex]]) + 1;
            uint maxReferenceIndex = (newIndex >= _items.Count ? uint.MaxValue : _orderByItem[_items[newIndex]]) - 1;

            _items.Insert(newIndex, item);

            if (maxReferenceIndex >= minReferenceIndex)
            {
                _orderByItem.Add(item, minReferenceIndex + (maxReferenceIndex - minReferenceIndex)/2);
            }
            else
            {
                ResetIndexes();
            }

            INotifyPropertyChanged changedItem = item as INotifyPropertyChanged;
            if (changedItem != null)
                changedItem.PropertyChanged += ItemPropertyChanged;

            return newIndex;
        }

        private int RemoveItem(T item)
        {
            uint referenceIndex = _orderByItem[item];
            int index = BinarySearch(referenceIndex);

            _items.RemoveAt(index);
            _orderByItem.Remove(item);

            INotifyPropertyChanged changedItem = item as INotifyPropertyChanged;
            if (changedItem != null)
                changedItem.PropertyChanged -= ItemPropertyChanged;

            return index;
        }

        private void ItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            T item = (T) sender;
            uint referenceIndex;

            // Item should always be there, except if the parent collection just removed it due to a PropertyChanged.
            // In this case, we have to ignore it.
            if (_orderByItem.TryGetValue(item, out referenceIndex))
            {
                int oldIndex = BinarySearch(referenceIndex);

                // Comparing position with the previous element and the next element. If index is between, the order has not being change
                if ((oldIndex != 0 && _comparerWithFallback.Compare(_items[oldIndex - 1], item) > 0) || (oldIndex != _items.Count - 1 && _comparerWithFallback.Compare(_items[oldIndex + 1], item) < 0))
                {
                    int newIndex = ~BinarySearch(item, oldIndex);
                    _items.Move(oldIndex, newIndex);
                    int previousIndex = newIndex - 1;
                    uint minReferenceIndex = (previousIndex < 0 ? 0 : _orderByItem[_items[previousIndex]]) + 1;
                    uint maxReferenceIndex = (newIndex + 1 >= _items.Count ? uint.MaxValue : _orderByItem[_items[newIndex + 1]]) - 1;

                    if (maxReferenceIndex >= minReferenceIndex)
                    {
                        _orderByItem[item] = minReferenceIndex + (maxReferenceIndex - minReferenceIndex) / 2;
                    }
                    else
                    {
                        ResetIndexes();
                    }

                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, newIndex, oldIndex));
                }
            }
        }

        private int BinarySearch(uint customIndex)
        {
            Comparer<uint> comparer = Comparer<uint>.Default;
            int low = 0, hi = _items.Count - 1;
            while (low <= hi)
            {
                int median = low + (hi - low >> 1);
                int num = comparer.Compare(_orderByItem[_items[median]], customIndex);

                if (num == 0)
                    return median;
                if (num < 0)
                    low = median + 1;
                else
                    hi = median - 1;
            }

            return ~low;
        }

        private int BinarySearch(T item)
        {
            int low = 0, hi = _items.Count - 1;
            while (low <= hi)
            {
                int median = low + (hi - low >> 1);
                int num = _comparerWithFallback.Compare(_items[median], item);

                if (num == 0)
                    return median;
                if (num < 0)
                    low = median + 1;
                else
                    hi = median - 1;
            }

            return ~low;
        }

        private int BinarySearch(T item, int indexToExclude)
        {
            int low = 0, hi = _items.Count - 2;
            while (low <= hi)
            {
                int median = low + (hi - low >> 1);
                int num = _comparerWithFallback.Compare(_items[median < indexToExclude ? median : median + 1], item);

                if (num == 0)
                    return median;
                if (num < 0)
                    low = median + 1;
                else
                    hi = median - 1;
            }

            return ~low;
        }

        #region BaseObservatorCollection<T> Implementation

        public override IEnumerator<T> GetEnumerator()
        {
            TryInitialize();
            return _items.GetEnumerator();
        }

        protected override void OnObservatorsChanged(bool hasCollectionChanged, bool hasPropertyChanged)
        {
            if (hasCollectionChanged || hasPropertyChanged)
            {
                TryInitialize();
            }
            else
            {
                TryDispose();
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
