using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace ZumtenSoft.WpfUtils.Collections
{
    public class RangeObservatorCollection<T> : BaseObservatorCollection<T>
    {
        public IObservableCollection<T> Source { get; private set; }
        public int? Skip { get; private set; }
        public int? Take { get; private set; }
        private List<T> _items;

        public RangeObservatorCollection(IObservableCollection<T> source, int? skip, int? take)
        {
            Source = source;
            Skip = skip;
            Take = take;
        }

        private List<T> GetRange(IList<T> list, int? skip, int? take)
        {
            List<T> result = new List<T>();
            int minIndex = skip ?? 0;
            int maxIndex = minIndex + list.Count - minIndex;
            if (take.HasValue)
                maxIndex = Math.Min(maxIndex, minIndex + take.Value);

            for (int i = minIndex; i < maxIndex; i++)
                result.Add(list[i]);

            return result;
        }

        private void TryInitialize()
        {
            if (_items == null)
            {
                _items = GetRange(Source, Skip, Take);
                Source.CollectionChanged += SourceOnCollectionChanged;
            }
        }

        private void TryDispose()
        {
            if (_items != null)
            {
                _items = null;
                Source.CollectionChanged -= SourceOnCollectionChanged;
            }
        }

        private void SourceOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            int minSourceIndex = Skip ?? 0;
            int maxSourceIndex = Take.HasValue ? minSourceIndex + Take.Value : Int32.MaxValue;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewStartingIndex < minSourceIndex)
                    {
                        int nbItemsToAdd = Math.Min(maxSourceIndex - minSourceIndex, e.NewItems.Count);
                        int nbItemsToRemove = Math.Min(nbItemsToAdd, _items.Count);

                        if (nbItemsToRemove > 0)
                        {
                            int indexToRemove = _items.Count - nbItemsToRemove;
                            List<T> itemsToRemove = _items.GetRange(indexToRemove, nbItemsToRemove);
                            _items.RemoveRange(indexToRemove, nbItemsToRemove);
                            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, itemsToRemove, indexToRemove));
                        }

                        if (nbItemsToAdd > 0)
                        {
                            List<T> itemsToAdd = GetRange(Source, minSourceIndex, nbItemsToAdd).ToList();
                            _items.InsertRange(0, itemsToAdd);
                            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, itemsToAdd, 0));
                        }
                    }
                    else if (e.NewStartingIndex <= maxSourceIndex)
                    {
                        int nbItemsToAdd = Math.Min(maxSourceIndex - e.NewStartingIndex, e.NewItems.Count);
                        int nbItemsToRemove = Math.Min(nbItemsToAdd, _items.Count - (e.NewStartingIndex - minSourceIndex));

                        if (nbItemsToRemove > 0)
                        {
                            int indexToRemove = _items.Count - nbItemsToRemove;
                            List<T> itemsToRemove = _items.GetRange(indexToRemove, nbItemsToRemove);
                            _items.RemoveRange(indexToRemove, nbItemsToRemove);
                            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, itemsToRemove, indexToRemove));
                        }

                        if (nbItemsToAdd > 0)
                        {
                            int indexToAdd = e.NewStartingIndex - minSourceIndex;
                            List<T> itemsToAdd = e.NewItems.Cast<T>().Take(nbItemsToAdd).ToList();
                            _items.InsertRange(indexToAdd, itemsToAdd);
                            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, itemsToAdd, indexToAdd));
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    break;
                case NotifyCollectionChangedAction.Replace:
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
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

        public override int IndexOf(T item)
        {
            TryInitialize();
            return _items.IndexOf(item);
        }

        public override int Count
        {
            get { TryInitialize(); return _items.Count; }
        }

        public override T this[int index]
        {
            get
            {
                TryInitialize(); return _items[index];
            }
            set { throw new NotSupportedException(); }
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
    }
}
