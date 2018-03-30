using System.ComponentModel;
using System.Windows.Media.Imaging;
using ZumtenSoft.Linq2ObsCollection.Collections;
using ZumtenSoft.Linq2ObsCollection.Samples.FolderSize.Icons;

namespace ZumtenSoft.Linq2ObsCollection.Samples.FolderSize.Models
{
    public class FolderViewModel : INotifyPropertyChanged
    {
        private readonly ProcessingContext _context;
        public Folder Source { get; private set; }
        public FolderViewModel Parent { get; private set; }

        public FolderViewModel(FolderViewModel parent, Folder source, ProcessingContext context)
        {
            _context = context;
            Parent = parent;
            Source = source;
            Source.PropertyChanged += Source_PropertyChanged;
        }

        public string Name
        {
            get { return Source.Name; }
        }

        public long Length
        {
            get { return Source.Length; }
        }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set { if (_isExpanded != value) { _isExpanded = value; Notify("IsExpanded"); Notify("Image"); } }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { if (_isSelected != value) { _isSelected = value; Notify("IsSelected"); } }
        }

        public BitmapImage Image
        {
            get { return _isExpanded ? IconsCollection.FolderOpened : IconsCollection.FolderClosed; }
        }

        private SortingObservatorCollection<FolderViewModel> _subFolders;
        public SortingObservatorCollection<FolderViewModel> SubFolders
        {
            get
            {
                if (_subFolders == null)
                {
                    _subFolders = Source.SubFolders
                        .Dispatch(_context.UpdateSizeDispatcher)
                        .Select(x => new FolderViewModel(this, x, _context))
                        .Sort(_context.FolderComparer);
                    //.SplitEvents(); Dont need to use it, because the sorting already splits up all the events
                }
                return _subFolders;
            }
        }

        private IObservableCollection<FileViewModel> _files;
        public IObservableCollection<FileViewModel> Files
        {
            get
            {
                if (_files == null)
                {
                    _files = Source.Files
                        .Dispatch(_context.UpdateSizeDispatcher)
                        .Select(x => new FileViewModel(x, _context));
                }
                return _files;
            }
        }

        public void UpdateSorting()
        {
            if (_subFolders != null)
            {
                _subFolders.Comparer = _context.FolderComparer;
                foreach (FolderViewModel subFolder in _subFolders)
                    subFolder.UpdateSorting();
            }
        }

        private void Source_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _context.UpdateSizeDispatcher.Push(() => Notify(e.PropertyName));
        }

        private void Notify(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}