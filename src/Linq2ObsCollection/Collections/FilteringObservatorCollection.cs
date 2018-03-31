using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using ZumtenSoft.Linq2ObsCollection.Comparison;

namespace ZumtenSoft.Linq2ObsCollection.Collections
{
    /// <summary>
    /// Allows to filter items in the same way Enumerable.Where works, but with the
    /// ability to observe it.
    /// </summary>
    /// <typeparam name="T">Type of the collection items</typeparam>
    public class FilteringObservatorCollection<T> : BaseObservatorCollection<T>
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

        private Func<T, bool> _predicate;
        public Func<T, bool> Predicate
        {
            get { return _predicate; }
            set
            {
                if (_predicate != value)
                {
                    _predicate = value;
                    if (_items != null)
                        Reset();
                }
            }
        }

        private Dictionary<T, uint> _orderByItem;
        private ExtendedList<T> _items; 

        public FilteringObservatorCollection(IObservableCollection<T> source, Func<T, bool> predicate)
        {
            _source = source ?? ReadOnlyObservableCollection<T>.Empty;
            _predicate = predicate;
        }

        /// <summary>
        /// Initializes the collection if it not already initialized (in a lazy-loading fashion)
        /// </summary>
        private void TryInitialize()
        {
            if (_items == null)
            {
                IEqualityComparer<T> comparer = typeof(T).IsValueType ? EqualityComparer<T>.Default : ReferenceComparer<T>.EqualityComparer;
                _orderByItem = new Dictionary<T, uint>(_source.Count * 2, comparer);
                _source.CollectionChanged += SourceOnCollectionChanged;

                _items = new ExtendedList<T>(((ICollection<T>)_source).Where(_predicate));

                ResetOrdering();
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
                _orderByItem = null;
                _items = null;
                _source.CollectionChanged -= SourceOnCollectionChanged;
            }
        }

        private void Reset()
        {
            _items.Clear();
            _items.AddRange(((ICollection<T>)_source).Where(_predicate));

            ResetOrdering();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            OnPropertyChanged("Count");
        }

        private void ResetOrdering()
        {
            _orderByItem.Clear();
            uint partsLength = uint.MaxValue/((uint) _source.Count + 1);
            for (int index = 0; index < _source.Count; index++)
            {
                INotifyPropertyChanged changedItem = _source[index] as INotifyPropertyChanged;
                if (changedItem != null)
                    changedItem.PropertyChanged += ItemPropertyChanged;

                _orderByItem.Add(_source[index], (1 + (uint)index) * partsLength);
            }
        }

        private void SourceOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch(e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddItems(e.NewItems, e.NewStartingIndex);
                    OnPropertyChanged("Count");
                    break;
                case NotifyCollectionChangedAction.Remove:
                    RemoveItems(e.OldItems);
                    OnPropertyChanged("Count");
                    break;
                case NotifyCollectionChangedAction.Replace:
                    RemoveItems(e.OldItems);
                    AddItems(e.NewItems, e.NewStartingIndex);
                    OnPropertyChanged("Count");
                    break;
                case NotifyCollectionChangedAction.Move:
                    MoveItem(e.OldItems, e.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    Reset();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void AddItems(IList items, int startingIndex)
        {
            AppendToIndex(items, startingIndex);

            List<T> itemsToAdd = items.Cast<T>().Where(item => _predicate(item)).ToList();

            // There could be no items to add if they were all filtered.
            if (itemsToAdd.Count > 0)
            {
                uint order = _orderByItem[itemsToAdd[0]];
                int newIndex = ~BinarySearch(order);
                _items.InsertRange(newIndex, itemsToAdd);

                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, itemsToAdd, newIndex));
            }

            // We still need to observe all of the observable items in case the visibility property changes
            foreach (INotifyPropertyChanged item in items.OfType<INotifyPropertyChanged>())
                item.PropertyChanged += ItemPropertyChanged;
        }

        private void RemoveItems(IList items)
        {
            foreach (T item in items)
            {
                uint order = _orderByItem[item];

                int index = BinarySearch(order);
                // If the item was visible, we have to remove it. Otherwise, just cleanup.
                if (index >= 0)
                {
                    _items.RemoveAt(index);
                    _orderByItem.Remove(item);
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
                }
                else
                {
                    _orderByItem.Remove(item);
                }

                INotifyPropertyChanged changedItem = item as INotifyPropertyChanged;
                if (changedItem != null)
                    changedItem.PropertyChanged -= ItemPropertyChanged;
            }
        }

        private void MoveItem(IList items, int newSourceIndex)
        {
            // Move only the items that are visible.
            List<T> itemsToMove = items.Cast<T>().Where(x => _predicate(x)).ToList();
            if (itemsToMove.Count > 0)
            {
                uint order = _orderByItem[itemsToMove[0]];
                int oldIndex = BinarySearch(order);
            
                // Reset the order of all the moved items (even those not visible)
                foreach (T item in items)
                    _orderByItem.Remove(item);

                AppendToIndex(items, newSourceIndex);
                order = _orderByItem[itemsToMove[0]];

                int newIndex = ~BinarySearch(order, oldIndex, itemsToMove.Count);
                _items.MoveRange(oldIndex, newIndex, itemsToMove.Count);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, itemsToMove, newIndex, oldIndex));
            }
            else
            {
                // Reset the order of all the moved items (even those not visible)
                foreach (T item in items)
                    _orderByItem.Remove(item);

                AppendToIndex(items, newSourceIndex);
            }
        }

        private void ItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            T item = (T) sender;
            bool newVisibility = _predicate(item);
            uint order = _orderByItem[(T)sender];
            int currentIndex = BinarySearch(order);
            
            // Is visible but should not be
            if (currentIndex >= 0 && !newVisibility)
            {
                _items.RemoveAt(currentIndex);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, sender, currentIndex));
                OnPropertyChanged("Count");
            }

            // Is not visible but should be
            else if (currentIndex < 0 && newVisibility)
            {
                _items.Insert(~currentIndex, (T)sender);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, sender, ~currentIndex));
                OnPropertyChanged("Count");
            }
        }

        private void AppendToIndex(IList items, int startingIndex)
        {
            int nbNewItems = items.Count;

            // Search for the available order number for the new items.
            int previousIndex = startingIndex - 1;
            int nextIndex = startingIndex + nbNewItems;
            uint minOrder = (previousIndex < 0 ? 0 : _orderByItem[_source[previousIndex]]) + 1;
            uint maxOrder = (nextIndex >= _source.Count ? uint.MaxValue : _orderByItem[_source[nextIndex]]) - 1;

            // If there is enough space to add the new items, proceed.
            if (1 + maxOrder - minOrder >= nbNewItems)
            {
                uint partsLength = (maxOrder - minOrder + 1) / ((uint)nbNewItems + 1);
                for (int i = 0; i < nbNewItems; i++)
                    _orderByItem.Add((T) items[i], minOrder + (1 + (uint) i)*partsLength);
            }
            else
            {
                // Otherwise we have to reset the ordering to make some space
                ResetOrdering();
            }
        }

        private int BinarySearch(uint customIndex, int indexToExclude = Int32.MaxValue, int nbItemsToExclude = 0)
        {
            Comparer<uint> comparer = Comparer<uint>.Default;
            int low = 0, hi = _items.Count - 1 - nbItemsToExclude;
            while (low <= hi)
            {
                int median = low + (hi - low >> 1);
                int num = comparer.Compare(_orderByItem[_items[(median >= indexToExclude ? median + nbItemsToExclude : median)]], customIndex);

                if (num == 0)
                    return median;
                if (num < 0)
                    low = median + 1;
                else
                    hi = median - 1;
            }

            return ~low;
        }

        #region IList<T> Implementation

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

        public override T this[int index]
        {
            get
            {
                TryInitialize();
                return _items[index];
            }
            set { throw new NotSupportedException(); }
        }

        #endregion IList<T> Implementation
    }
}
