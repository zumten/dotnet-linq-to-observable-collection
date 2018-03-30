using System.Threading;
using System.Windows.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZumtenSoft.Linq2ObsCollection.Collections;
using ZumtenSoft.Linq2ObsCollection.Threading;

namespace ZumtenSoft.Linq2ObsCollection.Tests.Collections
{
    [TestClass]
    public class DispatchingObservatorCollectionTests
    {
        [TestMethod]
        public void Add_WhenInsertingItems_EventsShouldBeFiredOnDispatcherThread()
        {
            Dispatcher expected = Dispatcher.CurrentDispatcher;
            Dispatcher actual = null;

            DispatcherQueue dispatchQueue = new DispatcherQueue(expected, DispatcherPriority.Normal);
            DispatcherFrame frame = new DispatcherFrame();
            ObservableCollection<int> list = new ObservableCollection<int> { 4, 7, 9 };
            DispatchingObservatorCollection<int> dispatchList = new DispatchingObservatorCollection<int>(dispatchQueue, list);
            dispatchList.CollectionChanged += (sender, args) =>
            {
                actual = Dispatcher.CurrentDispatcher;
                frame.Continue = false;
            };

            Thread thread = new Thread(() => list.Add(5));

            thread.Start();
            thread.Join();
            Dispatcher.PushFrame(frame);

            Assert.AreEqual(expected, actual);
        }

    }
}
