using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace ZumtenSoft.WpfUtils.Collections
{
    /// <summary>
    /// Allows to join two IObservableCollection in the same way Enumerable.Concat works, but with the
    /// ability to observe it.
    /// </summary>
    /// <typeparam name="T">Type of the collection items</typeparam>
    public class ConcatenationObservatorCollection<T> : BaseObservatorCollection<T>
    {
        private IObservableCollection<T> _first;
        public IObservableCollection<T> First
        {
            get { return _first; }
            set
            {
                value = value ?? ReadOnlyObservableCollection<T>.Empty;
                if (_first != value)
                {
                    if (_isListeningCollection)
                    {
                        IObservableCollection<T> oldValue = _first;
                        _first = ReadOnlyObservableCollection<T>.Empty;
                        if (oldValue.Count > 0)
                            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldValue.ToList(), 0));
                        _first.CollectionChanged -= FirstCollectionChanged;
                        _first = value;
                        if (_first.Count > 0)
                            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, _first.ToList(), 0));
                        _first.CollectionChanged += FirstCollectionChanged;
                    }
                    else
                    {
                        _first = value;
                    }

                    OnPropertyChanged("Count");
                }
            }
        }

        private IObservableCollection<T> _second;
        public IObservableCollection<T> Second
        {
            get { return _second; }
            set
            {
                value = value ?? ReadOnlyObservableCollection<T>.Empty;
                if (_second != value)
                {
                    if (_isListeningCollection)
                    {
                        IObservableCollection<T> oldValue = _second;
                        _second = ReadOnlyObservableCollection<T>.Empty;
                        if (oldValue.Count > 0)
                            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldValue.ToList(), _first.Count));
                        _second.CollectionChanged -= SecondCollectionChanged;
                        _second = value;
                        if (_second.Count > 0)
                            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, _second.ToList(), _first.Count));
                        _second.CollectionChanged += SecondCollectionChanged;
                    }
                    else
                    {
                        _second = value;
                    }

                    OnPropertyChanged("Count");
                }
            }
        }
        private bool _isListeningCollection;
        private bool _isListeningProperty;

        public ConcatenationObservatorCollection(IObservableCollection<T> first, IObservableCollection<T> second)
        {
            _first = first ?? ReadOnlyObservableCollection<T>.Empty;
            _second = second ?? ReadOnlyObservableCollection<T>.Empty;
        }

        private void FirstCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, e.NewItems, e.NewStartingIndex));
                    break;
                case NotifyCollectionChangedAction.Remove:
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, e.OldItems, e.OldStartingIndex));
                    break;
                case NotifyCollectionChangedAction.Replace:
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, e.NewItems, e.OldItems, e.OldStartingIndex));
                    break;
                case NotifyCollectionChangedAction.Move:
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, e.NewItems, e.NewStartingIndex, e.OldStartingIndex));
                    break;
                case NotifyCollectionChangedAction.Reset:
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void SecondCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch(e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, e.NewItems, _first.Count + e.NewStartingIndex));
                    break;
                case NotifyCollectionChangedAction.Remove:
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, e.OldItems, _first.Count + e.OldStartingIndex));
                    break;
                case NotifyCollectionChangedAction.Replace:
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, e.NewItems, e.OldItems, _first.Count + e.OldStartingIndex));
                    break;
                case NotifyCollectionChangedAction.Move:
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, e.NewItems, _first.Count + e.NewStartingIndex, _first.Count + e.OldStartingIndex));
                    break;
                case NotifyCollectionChangedAction.Reset:
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void SourcePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
        }

        #region ObservatorCollection<T> Implementation

        public override IEnumerator<T> GetEnumerator()
        {
            return ((ICollection<T>)_first).Concat(_second).GetEnumerator();
        }

        protected override void OnObservatorsChanged(bool hasCollectionChanged, bool hasPropertyChanged)
        {
            // Add or remove hooks to OnCollectionChanged, depending if there are listeners.
            if (hasCollectionChanged)
            {
                if (!_isListeningCollection)
                {
                    _first.CollectionChanged += FirstCollectionChanged;
                    _second.CollectionChanged += SecondCollectionChanged;
                    _isListeningCollection = true;
                }
            }
            else
            {
                if (_isListeningCollection)
                {
                    _first.CollectionChanged -= FirstCollectionChanged;
                    _second.CollectionChanged -= SecondCollectionChanged;
                    _isListeningCollection = false;
                }
            }

            // Add or remove hooks to OnPropertyChanged, depending if there are listeners.
            if (hasPropertyChanged)
            {
                if (!_isListeningProperty)
                {
                    _first.PropertyChanged += SourcePropertyChanged;
                    _second.PropertyChanged += SourcePropertyChanged;
                    _isListeningProperty = true;
                }
            }
            else
            {
                if (_isListeningProperty)
                {
                    _first.PropertyChanged += SourcePropertyChanged;
                    _second.PropertyChanged += SourcePropertyChanged;
                    _isListeningProperty = false;
                }
            }
        }

        public override bool Contains(T item)
        {
            return _first.Contains(item) || _second.Contains(item);
        }

        public override void CopyTo(T[] array, int arrayIndex)
        {
            _first.CopyTo(array, arrayIndex);
            _second.CopyTo(array, arrayIndex + _first.Count);
        }

        public override int Count
        {
            get { return _first.Count + _second.Count; }
        }
        
        public override int IndexOf(T item)
        {
            int index = _first.IndexOf(item);
            if (index >= 0)
                return index;

            index = _second.IndexOf(item);
            return index < 0 ? index : index + _first.Count;
        }

        public override T this[int index]
        {
            get { return index < _first.Count ? _first[index] : _second[index - _first.Count]; }
            set { throw new NotSupportedException(); }
        }

        #endregion ObservatorCollection<T> Implementation
    }
}
