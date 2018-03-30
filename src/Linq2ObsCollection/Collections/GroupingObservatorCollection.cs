using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using ZumtenSoft.WpfUtils.Comparison;

namespace ZumtenSoft.WpfUtils.Collections
{
    /// <summary>
    /// Allows to group items by key in the same way Enumerable.GroupBy works, but with the
    /// ability to observe it.
    /// </summary>
    /// <remarks>Does not support duplicates.</remarks>
    /// <typeparam name="TKey">Type of the key used to group items</typeparam>
    /// <typeparam name="T">Type of the source collection items</typeparam>
    public class GroupingObservableCollection<TKey, T> : BaseObservatorCollection<GroupingObservableCollection<TKey, T>.GroupCollection>, ILookup<TKey, T>
    {
        #region Nested classes

        public class GroupCollection : BaseObservatorCollection<T>, IGrouping<TKey, T>
        {
            internal readonly ExtendedList<T> Items;

            public GroupCollection(IGrouping<TKey, T> group)
            {
                Key = group.Key;
                Items = new ExtendedList<T>(group);
            }

            public GroupCollection(TKey key, IEnumerable<T> items)
            {
                Key = key;
                Items = new ExtendedList<T>(items);
            }

            public TKey Key { get; private set; }

            public override T this[int index]
            {
                get { return Items[index]; }
                set { throw new NotSupportedException(); }
            }

            public override IEnumerator<T> GetEnumerator()
            {
                return Items.GetEnumerator();
            }

            protected override void OnObservatorsChanged(bool hasCollectionChanged, bool hasPropertyChanged)
            {
                
            }

            public override bool Contains(T item)
            {
                return Items.Contains(item);
            }

            public override void CopyTo(T[] array, int arrayIndex)
            {
                Items.CopyTo(array, arrayIndex);
            }

            public override int Count
            {
                get { return Items.Count; }
            }

            public override int IndexOf(T item)
            {
                return Items.IndexOf(item);
            }
        }

        class MetaData
        {
            public TKey Key;
            public T Value;
            public uint Order;
        }

        #endregion Nested classes

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

        private IEqualityComparer<TKey> _keyComparer;
        public IEqualityComparer<TKey> KeyComparer
        {
            get { return _keyComparer; }
            set
            {
                if (_keyComparer != value)
                {
                    _keyComparer = value;
                    _items = ((ICollection<T>)_source).GroupBy(_keySelector, _keyComparer).Select(x => new GroupCollection(x)).ToList();
                    _itemsByKey = _items.ToDictionary(x => x.Key, _keyComparer);

                    ResetMetadatas();

                    _source.CollectionChanged += SourceOnCollectionChanged;
                }
            }
        }

        private Func<T, TKey> _keySelector;
        public Func<T, TKey> KeySelector
        {
            get { return _keySelector; }
            set
            {
                if (_keySelector != value)
                {
                    _keySelector = value;
                    _items = ((ICollection<T>)_source).GroupBy(_keySelector, _keyComparer).Select(x => new GroupCollection(x)).ToList();
                    _itemsByKey = _items.ToDictionary(x => x.Key, _keyComparer);

                    ResetMetadatas();

                    _source.CollectionChanged += SourceOnCollectionChanged;
                }
            }
        }
        private List<GroupCollection> _items;
        private Dictionary<TKey, GroupCollection> _itemsByKey;

        /// <summary>
        /// Table of mapping between each item and it's metadatas. This is what makes this collection
        /// non-tolerant to duplicate items.
        /// </summary>
        private Dictionary<T, MetaData> _metaDatas;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source">Source collection to observe.</param>
        /// <param name="keySelector">Function used to extract the key from each item.</param>
        /// <param name="comparer">Comparer used to distinguish the keys from each-other.</param>
        public GroupingObservableCollection(IObservableCollection<T> source, Func<T, TKey> keySelector, IEqualityComparer<TKey> comparer = null)
        {
            _source = source ?? ReadOnlyObservableCollection<T>.Empty;
            _keySelector = keySelector;
            _keyComparer = comparer ?? EqualityComparer<TKey>.Default;
        }

        /// <summary>
        /// Initializes the collection if it not already initialized (in a lazy-loading fashion)
        /// </summary>
        internal void TryInitialize()
        {
            if (_items == null)
            {
                _items = ((ICollection<T>)_source).GroupBy(_keySelector, _keyComparer).Select(x => new GroupCollection(x)).ToList();
                _itemsByKey = _items.ToDictionary(x => x.Key, _keyComparer);
                IEqualityComparer<T> comparer = ReferenceComparer<T>.EqualityComparer;
                _metaDatas = new Dictionary<T, MetaData>(_source.Count * 2, comparer);

                ResetMetadatas();

                foreach (INotifyPropertyChanged item in _source.OfType<INotifyPropertyChanged>())
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
                foreach (INotifyPropertyChanged item in _metaDatas.Keys.OfType<INotifyPropertyChanged>())
                    item.PropertyChanged -= ItemPropertyChanged;

                _items = null;
                _itemsByKey = null;
                _metaDatas = null;
                _source.CollectionChanged -= SourceOnCollectionChanged;
            }
        }

        private void Reset()
        {
            foreach (INotifyPropertyChanged item in _items.SelectMany(x => x).OfType<INotifyPropertyChanged>())
                item.PropertyChanged -= ItemPropertyChanged;

            _items = ((ICollection<T>)_source).GroupBy(_keySelector, _keyComparer).Select(x => new GroupCollection(x)).ToList();
            _itemsByKey = _items.ToDictionary(x => x.Key, _keyComparer);

            ResetMetadatas();

            foreach (INotifyPropertyChanged item in _source.OfType<INotifyPropertyChanged>())
                item.PropertyChanged += ItemPropertyChanged;
        }

        /// <summary>
        /// Initializes or resets all the indexes, making sure their ordering numbers are all spaced out from each-others.
        /// </summary>
        private void ResetMetadatas()
        {
            _metaDatas.Clear();
            uint partsLength = uint.MaxValue / ((uint)_source.Count + 1);
            for (int index = 0; index < _source.Count; index++)
            {
                T value = _source[index];
                _metaDatas.Add(value, new MetaData{ Key = _keySelector(value), Value = value, Order = (1 + (uint)index) * partsLength });
            }
        }

        /// <summary>
        /// Update this collection every time the source collection is updated.
        /// </summary>
        private void SourceOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddItems(e.NewItems, e.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    RemoveItems(e.OldItems);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    RemoveItems(e.OldItems);
                    AddItems(e.NewItems, e.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Move:
                    MoveItems(e.NewItems, e.NewStartingIndex);
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
            AppendToMetadatas(items, startingIndex);
            foreach (INotifyPropertyChanged item in items.OfType<INotifyPropertyChanged>())
                item.PropertyChanged += ItemPropertyChanged;

            foreach (IGrouping<TKey, T> newGroup in items.Cast<T>().GroupBy(_keySelector))
            {
                List<T> newItems = newGroup.ToList();
                // Extracting the group if it already exists.
                GroupCollection group;
                if (_itemsByKey.TryGetValue(newGroup.Key, out group))
                {
                    // Find the index of the first item we need to add
                    uint order = _metaDatas[newItems[0]].Order;
                    int newIndex = ~BinarySearch(group.Items, order);
                    group.Items.InsertRange(newIndex, newItems);
                    group.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItems, newIndex));
                    group.OnPropertyChanged("Count");
                }
                else
                {
                    // The group does not exist, we have to create it.
                    group = new GroupCollection(newGroup);
                    _items.Add(group);
                    _itemsByKey.Add(group.Key, group);
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, (object) group, _items.Count - 1));
                    OnPropertyChanged("Count");
                }
            }
        }

        private void RemoveItems(IList items)
        {
            foreach (INotifyPropertyChanged item in items.OfType<INotifyPropertyChanged>())
                item.PropertyChanged -= ItemPropertyChanged;

            foreach (T item in items)
            {
                TKey key = _keySelector(item);
                GroupCollection group = _itemsByKey[key];
                MetaData meta = _metaDatas[item];
                int index = BinarySearch(group.Items, meta.Order);
                _metaDatas.Remove(item);
                group.Items.RemoveAt(index);
                group.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
                group.OnPropertyChanged("Count");

                // If the group has no item anymore, then we can remove it.
                if (group.Count == 0)
                {
                    int indexGroup = _items.IndexOf(group);
                    _items.RemoveAt(indexGroup);
                    _itemsByKey.Remove(key);
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, (object) group, indexGroup));
                    OnPropertyChanged("Count");
                }
            }
        }

        private void MoveItems(IList items, int newSourceIndex)
        {
            // We take a snapshot of how the items were positioned before redoing the metadatas
            var oldGroups = items
                .Cast<T>()
                .Select(item => _metaDatas[item])
                .GroupBy(meta => meta.Key, _keyComparer)
                .Select(grp =>
                {
                    var groupCollection = _itemsByKey[grp.Key];
                    var metas = grp.ToList();
                    return new
                    {
                        GroupCollection = groupCollection,
                        Metas = metas,
                        OldIndex = BinarySearch(groupCollection, metas[0].Order)
                    };
                })
                .ToList();

            // Reset the metadatas because the ordering is not valid anymore
            foreach (T item in items.Cast<T>())
                _metaDatas.Remove(item);

            AppendToMetadatas(items, newSourceIndex);

            // Readd all the items and fire the move event
            foreach (var oldGroup in oldGroups)
            {
                List<T> itemsToNotify = new List<T>();
                int newIndex = -1;
                foreach (MetaData oldMeta in oldGroup.Metas)
                {
                    MetaData newMetaData = _metaDatas[oldMeta.Value];
                    newMetaData.Key = oldMeta.Key; // The key might have changed because of another PropertyChanged listener, but we dont want to know about it for now
                    itemsToNotify.Add(newMetaData.Value);
                    
                    if (newIndex < 0)
                        newIndex =  ~BinarySearch(oldGroup.GroupCollection, newMetaData.Order, oldGroup.OldIndex, oldGroup.Metas.Count);
                }

                if (oldGroup.OldIndex != newIndex)
                {
                    oldGroup.GroupCollection.Items.MoveRange(oldGroup.OldIndex, newIndex, itemsToNotify.Count);
                    oldGroup.GroupCollection.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, itemsToNotify, newIndex, oldGroup.OldIndex));
                }
            }
        }

        private void ItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            T item = (T)sender;
            TKey newKey = _keySelector(item);
            MetaData meta = _metaDatas[item];

            // If the key changed, we will have to change the item's group
            if (!_keyComparer.Equals(newKey, meta.Key))
            {
                // Remove the item from the old group
                GroupCollection oldGroup = _itemsByKey[meta.Key];
                int oldIndex = BinarySearch(oldGroup.Items, meta.Order);
                oldGroup.Items.RemoveAt(oldIndex);
                oldGroup.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, oldIndex));
                oldGroup.OnPropertyChanged("Count");

                // If the group has no item anymore, then we can remove it.
                if (oldGroup.Count == 0)
                {
                    int indexGroup = _items.IndexOf(oldGroup);
                    _items.RemoveAt(indexGroup);
                    _itemsByKey.Remove(oldGroup.Key);
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, (object)oldGroup, indexGroup));
                    OnPropertyChanged("Count");
                }

                meta.Key = newKey;
                GroupCollection newGroup;
                if (_itemsByKey.TryGetValue(newKey, out newGroup))
                {
                    // Find the index of the item in the new group
                    int newIndex = ~BinarySearch(newGroup.Items, meta.Order);
                    newGroup.Items.Insert(newIndex, item);
                    newGroup.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, newIndex));
                    newGroup.OnPropertyChanged("Count");
                }
                else
                {
                    // The group does not exist, we have to create it.
                    newGroup = new GroupCollection(newKey, new[] { item });
                    int newIndex = _items.Count;
                    _items.Add(newGroup);
                    _itemsByKey.Add(newGroup.Key, newGroup);
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, (object) newGroup, newIndex));
                    OnPropertyChanged("Count");
                }
            }
        }

        private void AppendToMetadatas(IList items, int startingIndex)
        {
            int nbNewItems = items.Count;

            // Find the previous item and the next one to make sure the new ordering is between those two.
            int previousIndex = startingIndex - 1;
            int nextIndex = startingIndex + nbNewItems;
            uint minCustomIndex = (previousIndex < 0 ? 0 : _metaDatas[_source[previousIndex]].Order) + 1;
            uint maxCustomIndex = (nextIndex >= _source.Count ? uint.MaxValue : _metaDatas[_source[nextIndex]].Order) - 1;

            // If there is enough space for adding the new items, then we do so
            if (1 + maxCustomIndex - minCustomIndex >= nbNewItems)
            {
                uint partsLength = (maxCustomIndex - minCustomIndex + 1) / ((uint)nbNewItems + 1);
                for (int i = 0; i < nbNewItems; i++)
                {
                    T value = (T)items[i];
                    MetaData metaData = new MetaData { Key = _keySelector(value), Value = value, Order = minCustomIndex + (1 + (uint)i) * partsLength };
                    _metaDatas.Add(value, metaData);
                }
            }
            else
            {
                // Otherwise, we have to rebuild the entire table
                ResetMetadatas();
            }
        }

        private int BinarySearch(IList<T> group, uint customIndex, int indexToExclude = Int32.MaxValue, int nbItemsToExclude = 0)
        {
            Comparer<uint> comparer = Comparer<uint>.Default;
            int low = 0, hi = group.Count - 1 - nbItemsToExclude;
            while (low <= hi)
            {
                int median = low + (hi - low >> 1);
                int num = comparer.Compare(_metaDatas[group[(median >= indexToExclude ? median + nbItemsToExclude : median)]].Order, customIndex);

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

        IEnumerator<IGrouping<TKey, T>> IEnumerable<IGrouping<TKey, T>>.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override IEnumerator<GroupCollection> GetEnumerator()
        {
            TryInitialize();
            return _items.GetEnumerator();
        }

        // If all observers have been removed, we can think it is safe to dispose the collection.
        // If this was not the case, the collection will be reinitialized, which is fine.
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

        public override bool Contains(GroupCollection item)
        {
            TryInitialize();
            return _items.Contains(item);
        }

        public override void CopyTo(GroupCollection[] array, int arrayIndex)
        {
            TryInitialize();
            _items.CopyTo(array, arrayIndex);
        }

        public bool Contains(TKey key)
        {
            return _itemsByKey.ContainsKey(key);
        }

        public override int Count
        {
            get
            {
                TryInitialize();
                return _items.Count;
            }
        }

        IEnumerable<T> ILookup<TKey, T>.this[TKey key]
        {
            get { return _itemsByKey[key]; }
        }

        public override int IndexOf(GroupCollection item)
        {
            TryInitialize();
            return _items.IndexOf(item);
        }

        public override GroupCollection this[int index]
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
