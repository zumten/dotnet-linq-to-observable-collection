using System.Collections.Generic;

namespace ZumtenSoft.Linq2ObsCollection.Samples.FolderSize.Models
{
    public class SortingOption<T>
    {
        public SortingOption(string name, IComparer<T> comparer)
        {
            Name = name;
            Comparer = comparer;
        }

        public string Name { get; private set; }
        public IComparer<T> Comparer { get; private set; }
    }
}
