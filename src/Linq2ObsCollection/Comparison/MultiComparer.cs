using System.Collections.Generic;

namespace ZumtenSoft.WpfUtils.Comparison
{
    /// <summary>
    /// Joins multiple IComparer together executing each of the Compare as a fallback
    /// for the previous Comparer.
    /// </summary>
    /// <typeparam name="T">The type of item to be compared</typeparam>
    internal class MultiComparer<T> : IComparer<T>
    {
        internal List<IComparer<T>> Comparers { get; private set; }

        public MultiComparer(params IComparer<T>[] comparers)
        {
            Comparers = new List<IComparer<T>>();
            foreach (IComparer<T> comparer in comparers)
                AddComparer(comparer);
        }

        private void AddComparer(IComparer<T> comparer)
        {
            MultiComparer<T> multi = comparer as MultiComparer<T>;
            if (multi != null)
            {
                foreach (IComparer<T> subComparer in multi.Comparers)
                    AddComparer(subComparer);
            }
            else
            {
                Comparers.Add(comparer);
            }
        }

        public int Compare(T x, T y)
        {
            int result = 0;
            for (int i = 0; result == 0 && i < Comparers.Count; i++)
                result = Comparers[i].Compare(x, y);
            return result;
        }
    }
}