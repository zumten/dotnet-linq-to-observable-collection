using System.Collections.Generic;
using System.Windows.Threading;
using ZumtenSoft.Linq2ObsCollection.Samples.FolderSize.Threading;
using ZumtenSoft.Linq2ObsCollection.Threading;

namespace ZumtenSoft.Linq2ObsCollection.Samples.FolderSize.Models
{
    public class ProcessingContext
    {
        public ProcessingContext(Dispatcher dispatcher, int processingNbThreads, IComparer<FolderViewModel> folderComparer)
        {
            FolderComparer = folderComparer;
            UpdateSizeDispatcher = new DispatcherQueue(dispatcher, DispatcherPriority.Background);
            UpdateIconDispatcher = new DispatcherQueue(dispatcher);
            SizeProcessingQueue = new TasksBag(processingNbThreads);
            IconProcessingQueue = new TasksQueue(1);
        }

        public bool IsRunning { get; set; }
        public IComparer<FolderViewModel> FolderComparer { get; set; }
        public TasksBag SizeProcessingQueue { get; private set; }
        public TasksQueue IconProcessingQueue { get; private set; }
        public DispatcherQueue UpdateSizeDispatcher { get; private set; }
        public DispatcherQueue UpdateIconDispatcher { get; private set; }
    }
}