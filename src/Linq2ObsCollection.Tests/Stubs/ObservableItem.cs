using System.ComponentModel;
using System.Diagnostics;

namespace ZumtenSoft.Linq2ObsCollection.Tests.Stubs
{
    [DebuggerDisplay(@"\{ObservableItem Value={Value}\}")]
    public class ObservableItem : INotifyPropertyChanged
    {
        public ObservableItem(int value)
        {
            _value = value;
        }

        private int _value;
        public int Value
        {
            get { return _value; }
            set
            {
                if (_value != value)
                {
                    _value = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("Value"));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public override string ToString()
        {
            return "{" + Value + "}";
        }

        //public static implicit operator int(ObservableItem m)
        //{
        //    return m.Value;
        //}

        public static implicit operator ObservableItem(int m)
        {
            return new ObservableItem(m);
        }

        public override bool Equals(object obj)
        {
            ObservableItem obsItem = obj as ObservableItem;
            return obsItem != null && _value.Equals(obsItem._value);
        }

        public bool Equals(ObservableItem other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other._value == _value;
        }

        public override int GetHashCode()
        {
            return _value;
        }
    }
}