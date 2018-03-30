using System;
using System.Collections.Generic;
using ZumtenSoft.WpfUtils.Comparison;
using ZumtenSoft.WpfUtils.Threading;

namespace ZumtenSoft.WpfUtils.Collections
{
    public static class ObservatorCollectionExtensions
    {
        /// <summary>
        /// Wraps the current collection with a new one sending all the CollectionChanged events on the dispatcher thread.
        /// </summary>
        public static DispatchingObservatorCollection<T> Dispatch<T>(this IObservableCollection<T> source, DispatcherQueue dispatcher)
        {
            return new DispatchingObservatorCollection<T>(dispatcher, source);
        }

        /// <summary>
        /// Splits all the bulk events (AddRange, InsertRange, RemoveRange, MoveRange) in multiple events to make the collection
        /// compatible with ListCollectionView. If you see the error "range actions are not supported", you might need to use
        /// this wrapper.
        /// </summary>
        public static SplitEventsObservatorCollection<T> SplitEvents<T>(this IObservableCollection<T> source)
        {
            return new SplitEventsObservatorCollection<T>(source);
        }

        /// <summary>
        /// Wraps the source collection with a new one converting each item using the selector.
        /// Works the same way Enumerable.Select works.
        /// </summary>
        public static SelectionObservatorCollection<TSource, T> Select<TSource, T>(this IObservableCollection<TSource> source, Func<TSource, T> selector)
        {
            return new SelectionObservatorCollection<TSource, T>(source, selector);
        }

        /// <summary>
        /// Wraps the source collection with a new one filtering the the items using the predicate.
        /// </summary>
        public static FilteringObservatorCollection<T> Where<T>(this IObservableCollection<T> source, Func<T, bool> predicate)
        {
            return new FilteringObservatorCollection<T>(source, predicate);
        }

        /// <summary>
        /// Combines two collection together.
        /// </summary>
        public static ConcatenationObservatorCollection<T> Concat<T>(this IObservableCollection<T> first, IObservableCollection<T> second)
        {
            return new ConcatenationObservatorCollection<T>(first, second);
        }

        /// <summary>
        /// Splits all the elements using a key, in the same way Enumerable.GroupBy works.
        /// </summary>
        public static GroupingObservableCollection<TKey, T> GroupBy<TKey, T>(this IObservableCollection<T> source, Func<T, TKey> keySelector, IEqualityComparer<TKey> keyComparer = null)
        {
            return new GroupingObservableCollection<TKey, T>(source, keySelector, keyComparer ?? EqualityComparer<TKey>.Default);
        }

        /// <summary>
        /// Builds a new observable collection sorting the source collection elements using a comparer.
        /// </summary>
        public static SortingObservatorCollection<T> Sort<T>(this IObservableCollection<T> source, IComparer<T> comparer)
        {
            return new SortingObservatorCollection<T>(source, comparer);
        }

        /// <summary>
        /// Builds a new observable collection sorting the source collection elements using a the keySelector.
        /// </summary>
        public static SortingObservatorCollection<T> OrderBy<T, TKey>(this IObservableCollection<T> source, Func<T, TKey> keySelector, IComparer<TKey> keyComparer = null)
        {
            return new SortingObservatorCollection<T>(source, new PropertyComparer<T, TKey>(keySelector, keyComparer));
        }

        /// <summary>
        /// Builds a new observable collection sorting the source collection elements using a the keySelector, in a descending order.
        /// </summary>
        public static SortingObservatorCollection<T> OrderByDescending<T, TKey>(this IObservableCollection<T> source, Func<T, TKey> keySelector, IComparer<TKey> keyComparer = null)
        {
            return new SortingObservatorCollection<T>(source, new PropertyComparer<T, TKey>(keySelector, keyComparer, true));
        }

        /// <summary>
        /// Adds a new criteria to the SortingObservatorCollection, in an ascending order
        /// </summary>
        public static SortingObservatorCollection<T> ThenBy<T, TKey>(this SortingObservatorCollection<T> source, Func<T, TKey> keySelector, IComparer<TKey> keyComparer = null)
        {
            source.Comparer = new MultiComparer<T>(source.Comparer, new PropertyComparer<T, TKey>(keySelector, keyComparer));
            return source;
        }

        /// <summary>
        /// Adds a new criteria to the SortingObservatorCollection, in an descending order
        /// </summary>
        public static SortingObservatorCollection<T> ThenByDescending<T, TKey>(this SortingObservatorCollection<T> source, Func<T, TKey> keySelector, IComparer<TKey> keyComparer = null)
        {
            source.Comparer = new MultiComparer<T>(source.Comparer, new PropertyComparer<T, TKey>(keySelector, keyComparer, true));
            return source;
        }
    }
}
