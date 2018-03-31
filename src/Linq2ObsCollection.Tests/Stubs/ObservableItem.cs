using System.ComponentModel;
using System.Diagnostics;

namespace ZumtenSoft.Linq2ObsCollection.Tests.Stubs
{
    [DebuggerDisplay(@"\{ObservableItem Value={" + nameof(Value) + @"}\}")]
    public class ObservableItem : INotifyPropertyChanged
    {
        public ObservableItem(int value)
        {
            _value = value;
        }

        private int _value;
        public int Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Value"));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public override string ToString()
        {
            return "{" + Value + "}";
        }

        public static implicit operator ObservableItem(int m)
        {
            return new ObservableItem(m);
        }

        public override bool Equals(object obj)
        {
            return obj is ObservableItem obsItem && _value.Equals(obsItem._value);
        }

        public override int GetHashCode()
        {
            return _value;
        }
    }
}