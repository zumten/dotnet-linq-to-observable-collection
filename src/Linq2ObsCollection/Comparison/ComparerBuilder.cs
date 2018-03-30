using System;
using System.Collections.Generic;

namespace ZumtenSoft.WpfUtils.Comparison
{
    /// <summary>
    /// Builds a comparer using the same syntax as linq Enumerable.OrderBy;
    /// </summary>
    /// <example>
    /// var comparer = ComparerBuilder&lt;Model&gt;.OrderBy(x => x.Name).ThenBy(x => x.Description).Comparer;
    /// </example>
    /// <typeparam name="T"></typeparam>
    public class ComparerBuilder<T>
    {
        public IComparer<T> Comparer { get; private set; }

        public ComparerBuilder(IComparer<T> comparer)
        {
            Comparer = comparer;
        }

        public static ComparerBuilder<T> OrderBy<TKey>(Func<T, TKey> keySelector, IComparer<TKey> keyComparer = null) 
        {
            return new ComparerBuilder<T>(new PropertyComparer<T, TKey>(keySelector, keyComparer ?? Comparer<TKey>.Default));
        }

        public static ComparerBuilder<T> OrderByDescending<TKey>(Func<T, TKey> keySelector, IComparer<TKey> keyComparer = null)
        {
            return new ComparerBuilder<T>(new PropertyComparer<T, TKey>(keySelector, keyComparer ?? Comparer<TKey>.Default, true));
        }

        public ComparerBuilder<T> Then(IComparer<T> nextComparer)
        {
            return new ComparerBuilder<T>(new MultiComparer<T>(Comparer, nextComparer));
        }

        public ComparerBuilder<T> ThenBy<TKey>(Func<T, TKey> keySelector, IComparer<TKey> keyComparer = null)
        {
            return Then(new PropertyComparer<T, TKey>(keySelector, keyComparer ?? Comparer<TKey>.Default));
        }

        public ComparerBuilder<T> ThenByDescending<TKey>(Func<T, TKey> keySelector, IComparer<TKey> keyComparer = null)
        {
            return Then(new PropertyComparer<T, TKey>(keySelector, keyComparer ?? Comparer<TKey>.Default, true));
        }
    }
}