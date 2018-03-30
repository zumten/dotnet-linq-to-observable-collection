using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace ZumtenSoft.WpfUtils.Collections
{
    public class SplitEventsObservatorCollection<T> : BaseObservatorCollection<T>
    {
        public IObservableCollection<T> Source { get; private set; }
        private ExtendedList<T> _items;

        public SplitEventsObservatorCollection(IObservableCollection<T> source)
        {
            Source = source;
        }

        private void TryInitialize()
        {
            if (_items == null)
            {
                _items = new ExtendedList<T>(Source);
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
            switch(e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        int newIndex = e.NewStartingIndex;
                        foreach (T newItem in e.NewItems)
                        {
                            _items.Insert(newIndex, newItem);
                            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItem, newIndex));
                            newIndex++;
                        }
                        OnPropertyChanged("Count");
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    {
                        int oldIndex = e.OldStartingIndex;
                        foreach (T oldItem in e.OldItems)
                        {
                            _items.RemoveAt(oldIndex);
                            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItem, oldIndex));
                        }
                        OnPropertyChanged("Count");
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    {
                        int newIndex = e.NewStartingIndex;
                        foreach (T newItem in e.NewItems)
                        {
                            T oldItem = _items[newIndex];
                            _items[newIndex] = newItem;
                            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, newItem, oldItem, newIndex));
                            newIndex++;
                        }
                        OnPropertyChanged("Count");
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    {
                        int newIndex = e.NewStartingIndex;
                        int oldIndex = e.OldStartingIndex;
                        foreach (T item in e.NewItems)
                        {
                            _items.Move(oldIndex, newIndex);
                            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, newIndex, oldIndex));

                            newIndex++;
                            if (newIndex <= oldIndex)
                                oldIndex++;
                        }
                        OnPropertyChanged("Count");
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    OnCollectionChanged(e);
                    OnPropertyChanged("Count");
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
            get
            {
                TryInitialize(); 
                return _items.Count;
            }
        }

        public override T this[int index]
        {
            get { TryInitialize(); return _items[index]; }
            set { throw new NotImplementedException(); }
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
