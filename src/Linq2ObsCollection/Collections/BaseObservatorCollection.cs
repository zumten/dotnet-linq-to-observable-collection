using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;

namespace ZumtenSoft.Linq2ObsCollection.Collections
{
    /// <summary>
    /// Base implementation of an IObservableCollection for a read-only ObservatorCollection. This kind of collection is supposed
    /// to observe another ObservableCollection with added value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BaseObservatorCollection<T> : IObservableCollection<T>, IList
    {
        public abstract IEnumerator<T> GetEnumerator();
        public abstract bool Contains(T item);
        public abstract void CopyTo(T[] array, int arrayIndex);
        public abstract int IndexOf(T item);
        public abstract int Count { get; }
        public abstract T this[int index] { get; set; }
        protected abstract void OnObservatorsChanged(bool hasCollectionChanged, bool hasPropertyChanged);

        #region IList Implementation

        void ICollection.CopyTo(Array array, int index)
        {
            CopyTo((T[]) array, index);
        }

        private object _syncRoot;
        object ICollection.SyncRoot
        {
            get
            {
                if (_syncRoot == null)
                    Interlocked.CompareExchange<object>(ref _syncRoot, new object(), null);
                return _syncRoot;
            }
        }

        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        void IList.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        object IList.this[int index]
        {
            get { return this[index]; }
            set { throw new NotSupportedException(); }
        }

        bool IList.IsReadOnly
        {
            get { return true; }
        }

        bool IList.IsFixedSize
        {
            get { return false; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        void ICollection<T>.Add(T item)
        {
            throw new NotSupportedException();
        }

        int IList.Add(object value)
        {
            throw new NotSupportedException();
        }

        bool IList.Contains(object value)
        {
            return Contains((T) value);
        }

        void IList.Clear()
        {
            throw new NotSupportedException();
        }

        int IList.IndexOf(object value)
        {
            return IndexOf((T) value);
        }

        void IList.Insert(int index, object value)
        {
            throw new NotSupportedException();
        }

        void IList.Remove(object value)
        {
            throw new NotSupportedException();
        }

        int ICollection.Count
        {
            get { return Count; }
        }

        #endregion IList Implementation

        #region IList<T> Implementation

        void ICollection<T>.Clear()
        {
            throw new NotSupportedException();
        }

        bool ICollection<T>.Remove(T item)
        {
            throw new NotSupportedException();
        }

        bool ICollection<T>.IsReadOnly { get { return true; } }

        void IList<T>.Insert(int index, T item)
        {
            throw new NotSupportedException();
        }

        void IList<T>.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        #endregion IList<T> Implementation

        /// <summary>
        /// Raises the <see cref="E:System.Collections.ObjectModel.ObservableCollection`1.PropertyChanged"/> event with the provided arguments.
        /// </summary>
        /// <param name="propertyName">Name of the property that changed.</param>
        internal void OnPropertyChanged(string propertyName)
        {
            InnerPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Raises the <see cref="E:System.Collections.ObjectModel.ObservableCollection`1.CollectionChanged"/> event with the provided arguments.
        /// </summary>
        /// <param name="e">Arguments of the event being raised.</param>
        internal void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            InnerCollectionChanged?.Invoke(this, e);
        }

        #region Events

        private event PropertyChangedEventHandler InnerPropertyChanged;
        private event NotifyCollectionChangedEventHandler InnerCollectionChanged;

        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                InnerPropertyChanged += value;
                OnObservatorsChanged(InnerCollectionChanged != null && InnerCollectionChanged.GetInvocationList().Length > 0, InnerPropertyChanged != null && InnerPropertyChanged.GetInvocationList().Length > 0);
            }
            remove
            {
                InnerPropertyChanged -= value;
                OnObservatorsChanged(InnerCollectionChanged != null && InnerCollectionChanged.GetInvocationList().Length > 0, InnerPropertyChanged != null && InnerPropertyChanged.GetInvocationList().Length > 0);
            }
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add
            {
                InnerCollectionChanged += value;
                OnObservatorsChanged(InnerCollectionChanged != null && InnerCollectionChanged.GetInvocationList().Length > 0, InnerPropertyChanged != null && InnerPropertyChanged.GetInvocationList().Length > 0);
            }
            remove
            {
                InnerCollectionChanged -= value;
                OnObservatorsChanged(InnerCollectionChanged != null && InnerCollectionChanged.GetInvocationList().Length > 0, InnerPropertyChanged != null && InnerPropertyChanged.GetInvocationList().Length > 0);
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

        #endregion Events
    }
}
