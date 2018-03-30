using System.ComponentModel;

namespace ZumtenSoft.WpfUtils.Samples.FolderSize.Models
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