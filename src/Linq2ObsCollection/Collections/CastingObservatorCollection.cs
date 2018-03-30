using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace ZumtenSoft.Linq2ObsCollection.Collections
{
    /// <summary>
    /// Casts every item of the source IObservableCollection in the same way Enumerable.Cast works,
    /// but with the ability to observe it.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CastingObservatorCollection<T> : BaseObservatorCollection<T>
    {
        public IObservableCollection Source { get; private set; }
        private readonly IList _sourceList;
        private bool _isListeningCollection;
        private bool _isListeningProperties;

        public CastingObservatorCollection(IObservableCollection source)
        {
            Source = source;
            _sourceList = (IList) source;
        }

        public override IEnumerator<T> GetEnumerator()
        {
            return _sourceList.Cast<T>().GetEnumerator();
        }

        public override bool Contains(T item)
        {
            return _sourceList.Contains(item);
        }

        public override void CopyTo(T[] array, int arrayIndex)
        {
            _sourceList.CopyTo(array, arrayIndex);
        }

        public override int IndexOf(T item)
        {
            return _sourceList.IndexOf(item);
        }

        public override int Count
        {
            get { return _sourceList.Count; }
        }

        public override T this[int index]
        {
            get { return (T)_sourceList[index]; }
            set { throw new NotSupportedException(); }
        }

        protected override void OnObservatorsChanged(bool hasCollectionChanged, bool hasPropertyChanged)
        {
            if (hasCollectionChanged && !_isListeningCollection)
            {
                Source.CollectionChanged += SourceOnCollectionChanged;
                _isListeningCollection = true;
            }
            else if (!hasCollectionChanged && _isListeningCollection)
            {
                Source.CollectionChanged -= SourceOnCollectionChanged;
                _isListeningCollection = false;
            }

            if (hasPropertyChanged && !_isListeningProperties)
            {
                Source.PropertyChanged += SourceOnPropertyChanged;
                _isListeningProperties = true;
            }
            else if (!hasPropertyChanged && _isListeningProperties)
            {
                Source.PropertyChanged -= SourceOnPropertyChanged;
                _isListeningProperties = false;
            }
        }

        private void SourceOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
        }

        private void SourceOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnCollectionChanged(e);
        }
    }
}
