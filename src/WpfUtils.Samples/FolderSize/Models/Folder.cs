using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using ZumtenSoft.WpfUtils.Collections;
using ZumtenSoft.WpfUtils.Samples.FolderSize.Threading;

namespace ZumtenSoft.WpfUtils.Samples.FolderSize.Models
{
    public class Folder : INotifyPropertyChanged
    {
        private readonly TasksBag _tasks;

        public Folder(Folder parent, DirectoryInfo source, TasksBag tasks)
        {
            _tasks = tasks;
            Parent = parent;
            Source = source;
            Files = new ObservableCollection<FileInfo>();
            SubFolders = new ObservableCollection<Folder>();
            _tasks.Push(Process);
        }

        public DirectoryInfo Source { get; private set; }
        public Folder Parent { get; private set; }
        public string Name { get { return Source.Name; } }

        private long _length;
        public long Length { get { return _length; } }

        public ObservableCollection<FileInfo> Files { get; private set; }
        public ObservableCollection<Folder> SubFolders { get; private set; }

        private void Process()
        {
            FileInfo[] files;
            try
            {
                files = Source.GetFiles();
            }
            catch (UnauthorizedAccessException)
            {
                files = new FileInfo[0];
            }
            catch (DirectoryNotFoundException)
            {
                files = new FileInfo[0];
            }

            DirectoryInfo[] directories;
            try
            {
                directories = Source.GetDirectories();
            }
            catch (UnauthorizedAccessException)
            {
                directories = new DirectoryInfo[0];
            }
            catch (DirectoryNotFoundException)
            {
                directories = new DirectoryInfo[0];
            }
            _length = files.Sum(x => x.Length);
            Files.AddRange(files);

            if (PropertyChanged != null)
                PropertyChanged(this, new SizePropertyChangedEventArgs("Length", _length));

            List<Folder> newFolders = new List<Folder>();
            foreach (DirectoryInfo directory in directories)
            {
                Folder folder = new Folder(this, directory, _tasks);
                folder.PropertyChanged += SubFolderOnPropertyChanged;
                newFolders.Add(folder);
            }
            SubFolders.AddRange(newFolders);
        }

        private void SubFolderOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SizePropertyChangedEventArgs sizeChanged = e as SizePropertyChangedEventArgs;
            if (sizeChanged != null)
            {
                Interlocked.Add(ref _length, sizeChanged.IncrementBy);
                if (PropertyChanged != null)
                    PropertyChanged(this, e);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
