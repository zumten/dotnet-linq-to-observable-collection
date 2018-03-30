using System.ComponentModel;

namespace ZumtenSoft.WpfUtils.Samples.FolderSize.Models
{
    public abstract class NotifyObject : INotifyPropertyChanged
    {
        protected void Notify(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void Notify(PropertyChangedEventArgs args)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, args);
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
