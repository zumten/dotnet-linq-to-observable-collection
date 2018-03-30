using System.ComponentModel;

namespace ZumtenSoft.Linq2ObsCollection.Samples.FolderSize.Models
{
    public class SizePropertyChangedEventArgs : PropertyChangedEventArgs
    {
        public long IncrementBy { get; private set; }

        public SizePropertyChangedEventArgs(string propertyName, long incrementBy) : base(propertyName)
        {
            IncrementBy = incrementBy;
        }
    }
}