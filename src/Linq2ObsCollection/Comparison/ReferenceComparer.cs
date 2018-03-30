using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ZumtenSoft.WpfUtils.Comparison
{
    /// <summary>
    /// Compares objects by their reference, allowing to add any type of object to a Dictionary.
    /// For ValueTypes, the default comparer is used instead.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class ReferenceComparer<T> : IEqualityComparer<T>, IComparer<T>
    {
        private static IEqualityComparer<T> _equalityComparer;
        public static IEqualityComparer<T> EqualityComparer
        {
            get
            {
                return _equalityComparer ?? (_equalityComparer = typeof(T).IsValueType ? (IEqualityComparer<T>)EqualityComparer<T>.Default : new ReferenceComparer<T>());
            }
        }

        private static IComparer<T> _comparer;
        public static IComparer<T> Comparer
        {
            get
            {
                return _comparer ?? (_comparer = typeof(T).IsValueType ? (IComparer<T>)Comparer<T>.Default : new ReferenceComparer<T>());
            }
        }

        public bool Equals(T x, T y)
        {
            return ReferenceEquals(x, y);
        }

        public int GetHashCode(T obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }

        public int Compare(T x, T y)
        {
            return RuntimeHelpers.GetHashCode(x).CompareTo(RuntimeHelpers.GetHashCode(y));
        }
    }
}
