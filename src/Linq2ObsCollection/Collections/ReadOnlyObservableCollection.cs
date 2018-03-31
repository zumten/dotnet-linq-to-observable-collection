using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace ZumtenSoft.Linq2ObsCollection.Collections
{
    /// <summary>
    /// This collection is mostly used as a placeholder until a true ObservableCollection
    /// can be used. For example, you could initialize SelectionObservatorCollection with
    /// an empty collection and replace it later.
    /// </summary>
    /// <typeparam name="T">Type of the collection items</typeparam>
    public class ReadOnlyObservableCollection<T> : IObservableCollection<T>, IList
    {
        public static ReadOnlyObservableCollection<T> Empty { get; } = new ReadOnlyObservableCollection<T>(new T[0]);

        private readonly List<T> _items;

        public ReadOnlyObservableCollection(IEnumerable<T> items)
        {
            _items = items.ToList();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item)
        {
            throw new NotSupportedException();
        }

        int IList.Add(object value)
        {
            throw new NotSupportedException();
        }

        bool IList.Contains(object value)
        {
            return Contains((T)value);
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        int IList.IndexOf(object value)
        {
            return ((IList)_items).IndexOf(value);
        }

        void IList.Insert(int index, object value)
        {
            throw new NotSupportedException();
        }

        void IList.Remove(object value)
        {
            throw new NotSupportedException();
        }

        public bool Contains(T item)
        {
            return _items.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _items.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            throw new NotSupportedException();
        }

        void ICollection.CopyTo(Array array, int index)
        {
            CopyTo((T[])array, index);
        }

        public int Count
        {
            get { return _items.Count; }
        }

        object ICollection.SyncRoot
        {
            get { return ((IList)_items).SyncRoot; }
        }

        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        bool IList.IsFixedSize
        {
            get { return true; }
        }

        public int IndexOf(T item)
        {
            return _items.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            throw new NotSupportedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        object IList.this[int index]
        {
            get { return _items[index]; }
            set { throw new NotSupportedException(); }
        }

        public T this[int index]
        {
            get { return _items[index]; }
            set { throw new NotSupportedException(); }
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add {  }
            remove {  }
        }

        public IObservableCollection<TResult> Cast<TResult>()
        {
            return new CastingObservatorCollection<TResult>(this);
        }

        public IObservableCollection<TResult> OfType<TResult>()
        {
            return new TypeFilteringObservatorCollection<TResult>(this);
        }

        public event PropertyChangedEventHandler PropertyChanged
        {
            add {  }
            remove {  }
        }
    }
}
