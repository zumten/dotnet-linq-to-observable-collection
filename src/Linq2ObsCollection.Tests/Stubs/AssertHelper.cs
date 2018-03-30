using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZumtenSoft.Linq2ObsCollection.Collections;

namespace ZumtenSoft.Linq2ObsCollection.Tests.Stubs
{
    public class CollectionListener : List<NotifyCollectionChangedEventArgs>
    {
        public static CollectionListener Create<T>(IObservableCollection<T> observedCollection)
        {
            CollectionListener listener = new CollectionListener();
            observedCollection.CollectionChanged += (sender, args) => listener.Add(args);
            return listener;
        }

        public void Expect(params NotifyCollectionChangedEventArgs[] expected)
        {
            Assert.AreEqual(expected.Length, Count, "Number of events does not match");
            for (int i = 0; i < expected.Length; i++)
                Compare(expected[i], this[i]);
        }

        private static void Compare(NotifyCollectionChangedEventArgs expected, NotifyCollectionChangedEventArgs actual)
        {
            if (expected == null)
                Assert.IsNull(actual);
            else
            {
                Assert.IsNotNull(actual);
                Assert.AreEqual(expected.Action, actual.Action);
                switch (expected.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        CollectionAssert.AreEqual(expected.NewItems, actual.NewItems);
                        Assert.AreEqual(expected.NewStartingIndex, actual.NewStartingIndex);
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        CollectionAssert.AreEqual(expected.OldItems, actual.OldItems);
                        Assert.AreEqual(expected.OldStartingIndex, actual.OldStartingIndex);
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        CollectionAssert.AreEqual(expected.OldItems, actual.OldItems);
                        CollectionAssert.AreEqual(expected.NewItems, actual.NewItems);
                        Assert.AreEqual(expected.OldStartingIndex, actual.OldStartingIndex);
                        break;
                    case NotifyCollectionChangedAction.Move:
                        CollectionAssert.AreEqual(expected.NewItems, actual.NewItems);
                        Assert.AreEqual(expected.OldStartingIndex, actual.OldStartingIndex);
                        Assert.AreEqual(expected.NewStartingIndex, actual.NewStartingIndex);
                        break;
                }
            }
        }
    }

    public static class AssertHelper
    {
        public static void AreEqual(int[][] expected, GroupingObservatorCollection<int, int> actual)
        {
            Assert.AreEqual(expected.Length, actual.Count);
            for (int i = 0; i < expected.Length; i++)
                CollectionAssert.AreEqual(expected[i], actual[i]);
        }

        public static void AreEqual(int[][] expected, GroupingObservatorCollection<int, ObservableItem> actual)
        {
            List<IGrouping<int, ObservableItem>> groups = ((IList)actual).Cast<IGrouping<int, ObservableItem>>().ToList();
            Assert.AreEqual(expected.Length, actual.Count);
            for (int i = 0; i < expected.Length; i++)
                CollectionAssert.AreEqual(expected[i], groups[i].Select(x => x.Value).ToList());
        }
        
    }
}
