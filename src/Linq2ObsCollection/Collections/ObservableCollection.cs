using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;

namespace ZumtenSoft.Linq2ObsCollection.Collections
{
    /// <summary>
    /// Represents a dynamic data collection that provides notifications
    /// when items get added, removed, moved or when the whole list is refreshed.
    /// 
    /// This class has been extracted from System.Collections.ObjectModel
    /// and the following modifications has been applied:
    /// - Removed the inheritance to Collection&lt;T&gt;
    /// - Added implementation to IObservableCollection&lt;T&gt; and IList
    /// - Added the bulk changes methods: AddRange, InsertRange, RemoveRange and MoveRange
    /// - Simplified the class SimpleMonitor
    /// - Merged OnObservableCollection overloads
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    [Serializable]
    public class ObservableCollection<T> : IObservableCollection<T>, IList
    {
        private readonly SimpleMonitor _monitor = new SimpleMonitor();
        private readonly ExtendedList<T> _items;

        #region Events

        [NonSerialized] private NotifyCollectionChangedEventHandler _collectionChanged;
        [NonSerialized] private PropertyChangedEventHandler _propertyChanged;
        
        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add { PropertyChanged += value; }
            remove { PropertyChanged -= value; }
        }

        /// <summary>
        /// Occurs when an item is added, removed, changed, moved, or the entire list is refreshed.
        /// </summary>
        public virtual event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add
            {
                NotifyCollectionChangedEventHandler changedEventHandler = _collectionChanged;
                NotifyCollectionChangedEventHandler comparand;
                do
                {
                    comparand = changedEventHandler;
                    changedEventHandler = Interlocked.CompareExchange(ref _collectionChanged, comparand + value, comparand);
                }
                while (changedEventHandler != comparand);
            }
            remove
            {
                NotifyCollectionChangedEventHandler changedEventHandler = _collectionChanged;
                NotifyCollectionChangedEventHandler comparand;
                do
                {
                    comparand = changedEventHandler;
                    changedEventHandler = Interlocked.CompareExchange(ref _collectionChanged, comparand - value, comparand);
                }
                while (changedEventHandler != comparand);
            }
        }

        public IObservableCollection<TResult> Cast<TResult>()
        {
            return new CastingObservatorCollection<TResult>(this);
        }

        public IObservableCollection<TResult> OfType<TResult>()
        {
            return new TypeFilteringObservatorCollection<TResult>(this);
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        protected event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                PropertyChangedEventHandler changedEventHandler = _propertyChanged;
                PropertyChangedEventHandler comparand;
                do
                {
                    comparand = changedEventHandler;
                    changedEventHandler = Interlocked.CompareExchange(ref _propertyChanged, comparand + value, comparand);
                }
                while (changedEventHandler != comparand);
            }
            remove
            {
                PropertyChangedEventHandler changedEventHandler = _propertyChanged;
                PropertyChangedEventHandler comparand;
                do
                {
                    comparand = changedEventHandler;
                    changedEventHandler = Interlocked.CompareExchange(ref _propertyChanged, comparand - value, comparand);
                }
                while (changedEventHandler != comparand);
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Collections.ObjectModel.ObservableCollection`1.PropertyChanged"/> event with the provided arguments.
        /// </summary>
        /// <param name="e">Arguments of the event being raised.</param>
        private void OnPropertyChanged(string propertyName)
        {
            if (_propertyChanged != null)
                _propertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Raises the <see cref="E:System.Collections.ObjectModel.ObservableCollection`1.CollectionChanged"/> event with the provided arguments.
        /// </summary>
        /// <param name="e">Arguments of the event being raised.</param>
        private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (_collectionChanged != null)
                using (_monitor.Enter())
                    _collectionChanged(this, e);
        }

        #endregion Events

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Collections.ObjectModel.ObservableCollection`1"/> class.
        /// </summary>
        public ObservableCollection()
        {
            _items = new ExtendedList<T>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Collections.ObjectModel.ObservableCollection`1"/> class that contains elements copied from the specified collection.
        /// </summary>
        /// <param name="collection">The collection from which the elements are copied.</param><exception cref="T:System.ArgumentNullException">The <paramref name="collection"/> parameter cannot be null.</exception>
        public ObservableCollection(IEnumerable<T> collection)
        {
            _items = new ExtendedList<T>(collection);
        }


        public T this[int index]
        {
            get { return _items[index]; }
            set
            {
                CheckReentrancy();
                T obj = this[index];
                _items[index] = value;
                OnPropertyChanged("Item[]");
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, obj, index));
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return _items.IndexOf(item);
        }

        public bool Contains(T item)
        {
            return _items.Contains(item);
        }

        public void Add(T item)
        {
            Insert(_items.Count, item);
        }

        public void AddRange(IEnumerable<T> items)
        {
            InsertRange(_items.Count, items);
        }

        /// <summary>
        /// Inserts an item into the collection at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param><param name="item">The object to insert.</param>
        public void Insert(int index, T item)
        {
            CheckReentrancy();
            _items.Insert(index, item);
            OnPropertyChanged("Count");
            OnPropertyChanged("Item[]");
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        public void InsertRange(int index, IEnumerable<T> items)
        {
            CheckReentrancy();
            T[] itemsToInsert = items.ToArray();
            if (itemsToInsert.Length > 0)
            {
                _items.InsertRange(index, itemsToInsert);
                OnPropertyChanged("Count");
                OnPropertyChanged("Item[]");
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, itemsToInsert, index));
            }
        }

        public bool Remove(T item)
        {
            int index = _items.IndexOf(item);
            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes the item at the specified index of the collection.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        public void RemoveAt(int index)
        {
            CheckReentrancy();
            T obj = _items[index];
            _items.RemoveAt(index);
            OnPropertyChanged("Count");
            OnPropertyChanged("Item[]");
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, obj, index));
        }

        public void RemoveRange(int index, int count)
        {
            CheckReentrancy();
            if (count > 0)
            {
                T[] itemsToRemove = new T[count];
                for (int i = 0; i < count; i++)
                    itemsToRemove[i] = _items[index + i];
                _items.RemoveRange(index, count);
                OnPropertyChanged("Count");
                OnPropertyChanged("Item[]");
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, itemsToRemove, index));
            }
        }

        /// <summary>
        /// Removes all items from the collection.
        /// </summary>
        public void Clear()
        {
            CheckReentrancy();
            if (_items.Count > 0)
            {
                _items.Clear();
                OnPropertyChanged("Count");
                OnPropertyChanged("Item[]");
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        /// <summary>
        /// Moves the item at the specified index to a new location in the collection.
        /// </summary>
        /// <param name="oldIndex">The zero-based index specifying the location of the item to be moved.</param><param name="newIndex">The zero-based index specifying the new location of the item.</param>
        public void Move(int oldIndex, int newIndex)
        {
            CheckReentrancy();
            T obj = _items[oldIndex];
            _items.Move(oldIndex, newIndex);
            OnPropertyChanged("Item[]");
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, obj, newIndex, oldIndex));
        }

        public void MoveRange(int oldIndex, int newIndex, int count)
        {
            CheckReentrancy();
            if (count > 0)
            {
                T[] itemsToMove = new T[count];
                for (int i = 0; i < count; i++)
                    itemsToMove[i] = _items[oldIndex + i];
                _items.MoveRange(oldIndex, newIndex, count);
                OnPropertyChanged("Item[]");
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, itemsToMove, newIndex, oldIndex));
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _items.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _items.Count; }
        }

        #region Explicit implementation

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        int IList.Add(object value)
        {
            int count = Count;
            Add((T)value);
            return count;
        }

        object IList.this[int index]
        {
            get { return this[index]; }
            set { this[index] = (T)value; }
        }

        bool IList.Contains(object value)
        {
            return Contains((T)value);
        }

        int IList.IndexOf(object value)
        {
            return IndexOf((T)value);
        }

        void IList.Insert(int index, object value)
        {
            Insert(index, (T)value);
        }

        void IList.Remove(object value)
        {
            Remove((T)value);
        }

        object ICollection.SyncRoot
        {
            get { return ((IList) _items).SyncRoot; }
        }

        bool ICollection.IsSynchronized
        {
            get { return ((IList) _items).IsSynchronized; }
        }

        bool IList.IsReadOnly
        {
            get { return false; }
        }

        bool ICollection<T>.IsReadOnly
        {
            get { return false; }
        }

        bool IList.IsFixedSize
        {
            get { return false; }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            _items.CopyTo((T[])array, index);
        }

        #endregion Explicit implementation

        #region Monitor

        /// <summary>
        /// Checks for reentrant attempts to change this collection.
        /// </summary>
        /// <exception cref="T:System.InvalidOperationException">If there was a call to <see cref="M:System.Collections.ObjectModel.ObservableCollection`1.BlockReentrancy"/> of which the <see cref="T:System.IDisposable"/> return value has not yet been disposed of. Typically, this means when there are additional attempts to change this collection during a <see cref="E:System.Collections.ObjectModel.ObservableCollection`1.CollectionChanged"/> event. However, it depends on when derived classes choose to call <see cref="M:System.Collections.ObjectModel.ObservableCollection`1.BlockReentrancy"/>.</exception>
        private void CheckReentrancy()
        {
            if (_monitor.IsBusy && _collectionChanged != null && _collectionChanged.GetInvocationList().Length > 1)
                throw new InvalidOperationException("ObservableCollectionReentrancyNotAllowed");
        }

        [Serializable]
        private class SimpleMonitor : IDisposable
        {
            private int _busyCount;

            public bool IsBusy
            {
                get { return _busyCount > 0; }
            }

            public IDisposable Enter()
            {
                ++_busyCount;
                return this;
            }

            public void Dispose()
            {
                --_busyCount;
            }
        }

        #endregion Monitor
    }
}
