using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace ZumtenSoft.WpfUtils.Collections
{
    public interface IObservableCollection : INotifyPropertyChanged, INotifyCollectionChanged
    {
        /// <summary>
        /// Casts every item of a collection into the type TResult.
        /// </summary>
        IObservableCollection<TResult> Cast<TResult>();

        /// <summary>
        /// Casts every item of a collection into the type TResult.
        /// </summary>
        IObservableCollection<TResult> OfType<TResult>();
    }

    /// <summary>
    /// Common interface for all the observable collections.
    /// </summary>
    /// <typeparam name="T">Type of the collection items</typeparam>
    public interface IObservableCollection<T> : IList<T>, IObservableCollection
    {
        
    }
}