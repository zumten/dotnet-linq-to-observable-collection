using System;
using System.Collections;
using System.ComponentModel;
using ZumtenSoft.Linq2ObsCollection.Collections;

namespace ZumtenSoft.Linq2ObsCollection.Samples.TreeSample
{
    public interface INode : IDisposable
    {
        string Name { get; }
        IEnumerable Nodes { get; }
    }

    public class Student : NotifyObject
    {
        public Student(string name, int score)
        {
            _name = name;
            _score = score;
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    Notify("Name");
                }
            }
        }

        private int _score;
        public int Score
        {
            get { return _score; }
            set
            {
                if (_score != value)
                {
                    _score = value;
                    Notify("Score");
                }
            }
        }
    }

    public class StudentGroupingNode : NotifyObject, INode
    {
        private readonly string _baseName;

        public StudentGroupingNode(int minimumScore, int maximumScore, IObservableCollection<StudentViewModel> nodes)
        {
            _baseName = "Scores from " + minimumScore + " to " + maximumScore;
            Nodes = nodes;
            Nodes.PropertyChanged += NodesOnPropertyChanged;
        }

        private void NodesOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch(e.PropertyName)
            {
                case "Count":
                    Notify("Name");
                    break;
            }
        }

        public string Name
        {
            get { return _baseName + " (" + Nodes.Count + ")"; }
        }

        IEnumerable INode.Nodes
        {
            get { return Nodes; }
        }

        public IObservableCollection<StudentViewModel> Nodes { get; private set; }

        public void Dispose()
        {
            Nodes.PropertyChanged -= NodesOnPropertyChanged;
        }
    }

    public class StudentViewModel : NotifyObject, INode
    {
        public Student Model { get; private set; }

        public StudentViewModel(Student model)
        {
            Model = model;
            Model.PropertyChanged += Source_PropertyChanged;
        }

        private void Source_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch(e.PropertyName)
            {
                case "Name":
                    Notify("Name");
                    break;

                case "Score":
                    Notify("Name");
                    Notify("Score");
                    break;
            }
        }

        public string Name
        {
            get { return Model.Name + " (Score: " + Model.Score + ")"; }
        }

        public int Score
        {
            get { return Model.Score; }
        }

        public IEnumerable Nodes
        {
            get { return null; }
        }

        public void Dispose()
        {
            Model.PropertyChanged -= Source_PropertyChanged;
        }
    }

    public class NotifyObject : INotifyPropertyChanged
    {
        protected void Notify(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
