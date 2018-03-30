using System;
using System.Collections.Generic;

namespace ZumtenSoft.Linq2ObsCollection.Comparison
{
    /// <summary>
    /// Compares two value by comparing two children properties.
    /// </summary>
    /// <typeparam name="T">Type of the item compared</typeparam>
    /// <typeparam name="TValue">Type of the item's value compared</typeparam>
    internal class PropertyComparer<T, TValue> : IComparer<T>
    {
        private readonly Func<T, TValue> _keySelector;
        private readonly IComparer<TValue> _comparer;
        private readonly bool _isDescending;

        public PropertyComparer(Func<T, TValue> keySelector, IComparer<TValue> keyComparer = null, bool isDescending = false)
        {
            _keySelector = keySelector;
            _comparer = keyComparer ?? Comparer<TValue>.Default;
            _isDescending = isDescending;
        }

        public int Compare(T x, T y)
        {
            int result = _comparer.Compare(_keySelector(x), _keySelector(y));
            if (_isDescending)
                return result == 0 ? 0 : result < 0 ? 1 : -1;
            return result;
        }
    }
}