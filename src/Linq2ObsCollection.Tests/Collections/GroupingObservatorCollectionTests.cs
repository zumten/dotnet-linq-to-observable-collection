using System.Collections.Specialized;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZumtenSoft.Linq2ObsCollection.Collections;
using ZumtenSoft.Linq2ObsCollection.Tests.Stubs;

namespace ZumtenSoft.Linq2ObsCollection.Tests.Collections
{
    [TestClass]
    public class GroupingObservatorCollectionTests
    {
        [TestMethod]
        public void Add_WhenInsertingAnItemWithoutGroup_NewGroupShouldBeCreated()
        {
            ObservableCollection<int> list = new ObservableCollection<int> { 4, 7, 9, 12, 16, 19 };
            GroupingObservatorCollection<int, int> actual = new GroupingObservatorCollection<int, int>(list, x => x%3);
            CollectionListener events = CollectionListener.Create(actual);
            list.Add(5);

            int[][] expected = new[] { new[] { 4, 7, 16, 19 }, new[] { 9, 12 }, new[] { 5 } };
            AssertHelper.AreEqual(expected, actual);
            events.Expect(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, (object)actual[2], 2));
        }

        [TestMethod]
        public void Add_WhenInsertingAnItemWithGroup_ShouldBeAddedAtTheEnd()
        {
            ObservableCollection<int> list = new ObservableCollection<int> { 4, 7, 9, 12, 16, 19 };
            GroupingObservatorCollection<int, int> actual = new GroupingObservatorCollection<int, int>(list, x => x % 3);
            CollectionListener events = CollectionListener.Create(actual);
            CollectionListener eventsChild = CollectionListener.Create(actual[1]);
            list.Add(6);

            int[][] expected = new[] { new[] { 4, 7, 16, 19 }, new[] { 9, 12, 6 } };
            AssertHelper.AreEqual(expected, actual);
            events.Expect();
            eventsChild.Expect(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, 6, 2));
        }

        [TestMethod]
        public void Remove_WhenAnItemAloneInItsGroup_GroupShouldBeRemoved()
        {
            ObservableCollection<int> list = new ObservableCollection<int> { 4, 5, 7, 9, 12, 16, 19 };
            GroupingObservatorCollection<int, int> actual = new GroupingObservatorCollection<int, int>(list, x => x % 3);
            CollectionListener events = CollectionListener.Create(actual);
            var groupRemoved = actual[1];
            list.Remove(5);

            int[][] expected = new[] { new[] { 4, 7, 16, 19 }, new[] { 9, 12 } };
            AssertHelper.AreEqual(expected, actual);
            events.Expect(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, (object) groupRemoved, 1));
        }

        [TestMethod]
        public void Remove_WhenAnItemIsRemovedFromBiggerGroup_GroupShouldRemain()
        {
            ObservableCollection<int> list = new ObservableCollection<int> { 4, 5, 7, 9, 12, 16, 19 };
            GroupingObservatorCollection<int, int> actual = new GroupingObservatorCollection<int, int>(list, x => x % 3);
            CollectionListener events = CollectionListener.Create(actual);
            CollectionListener eventsChild = CollectionListener.Create(actual[0]);
            list.Remove(16);

            int[][] expected = new[] { new[] { 4, 7, 19 }, new[] { 5 }, new[] { 9, 12 } };
            AssertHelper.AreEqual(expected, actual);
            events.Expect();
            eventsChild.Expect(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, 16, 2));
        }

        [TestMethod]
        public void Move_WhenItemIsMovedWithoutChangingGroupOrder_OrderShouldStay()
        {
            ObservableCollection<int> list = new ObservableCollection<int> { 4, 5, 7, 9, 12, 16, 19 };
            GroupingObservatorCollection<int, int> actual = new GroupingObservatorCollection<int, int>(list, x => x % 3);
            CollectionListener events = CollectionListener.Create(actual);
            list.Move(2, 4);

            int[][] expected = new[] { new[] { 4, 7, 16, 19 }, new[] { 5 }, new[] { 9, 12 } };
            AssertHelper.AreEqual(expected, actual);
            events.Expect();
        }


        [TestMethod]
        public void Move_WhenItemIsMovedWithinTheSameGroup_OrderShouldChange()
        {
            ObservableCollection<int> list = new ObservableCollection<int> { 4, 5, 7, 9, 12, 16, 19 };
            GroupingObservatorCollection<int, int> actual = new GroupingObservatorCollection<int, int>(list, x => x % 3);
            CollectionListener events = CollectionListener.Create(actual);
            CollectionListener eventsChild = CollectionListener.Create(actual[0]);
            list.Move(2, 6);

            int[][] expected = new[] { new[] { 4, 16, 19, 7 }, new[] { 5 }, new[] { 9, 12 } };
            AssertHelper.AreEqual(expected, actual);
            events.Expect();
            eventsChild.Expect(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, 7, 3, 1));
        }

        [TestMethod]
        public void MoveRange_WhenMultipleItemsMovedWithinTheSameGroup_OrderShouldChange()
        {
            ObservableCollection<int> list = new ObservableCollection<int> { 4, 5, 7, 9, 12, 16, 19 };
            GroupingObservatorCollection<int, int> actual = new GroupingObservatorCollection<int, int>(list, x => x % 3);
            CollectionListener events = CollectionListener.Create(actual);
            CollectionListener eventsChild = CollectionListener.Create(actual[0]);
            list.MoveRange(0, 3, 3);

            int[][] expected = new[] { new[] { 16, 4, 7, 19 }, new[] { 5 }, new[] { 9, 12 } };
            AssertHelper.AreEqual(expected, actual);
            events.Expect();
            eventsChild.Expect(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, new[] { 4, 7 }, 1, 0));
        }

        [TestMethod]
        public void PropertyChanged_WhenGroupingPropertyChanges_ItemShouldChangeGroup()
        {
            ObservableCollection<ObservableItem> list = new ObservableCollection<ObservableItem> { 4, 7, 9, 12, 16, 19 };
            GroupingObservatorCollection<int, ObservableItem> actual = new GroupingObservatorCollection<int, ObservableItem>(list, x => x.Value / 10);
            CollectionListener events = CollectionListener.Create(actual);
            CollectionListener eventsChild1 = CollectionListener.Create(actual[0]);
            CollectionListener eventsChild2 = CollectionListener.Create(actual[1]);
            ObservableItem movingObject = list[0];
            list[0].Value = 13;

            int[][] expected = new[] { new[] { 7, 9 }, new[] { 13, 12, 16, 19 } };
            AssertHelper.AreEqual(expected, actual);
            events.Expect();
            eventsChild1.Expect(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, movingObject, 0));
            eventsChild2.Expect(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, movingObject, 0));
        }

        [TestMethod]
        public void PropertyChanged_WhenGroupingPropertyChanges_ItemShouldChangeGroup2()
        {
            ObservableCollection<ObservableItem> list = new ObservableCollection<ObservableItem> { 4, 7, 9, 12, 16, 19 };
            GroupingObservatorCollection<int, ObservableItem> actual = new GroupingObservatorCollection<int, ObservableItem>(list, x => x.Value / 10);
            CollectionListener events = CollectionListener.Create(actual);
            list[0].Value = 23;

            int[][] expected = new[] { new[] { 7, 9 }, new[] { 12, 16, 19 }, new[] { 23 }};
            AssertHelper.AreEqual(expected, actual);
            events.Expect(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, actual.Last<IGrouping<int, ObservableItem>>(), 2));
        }


        [TestMethod]
        public void PropertyChanged_WhenGroupBecomesEmpty_GroupShouldBeRemoved()
        {
            ObservableCollection<ObservableItem> list = new ObservableCollection<ObservableItem> { 4, 7, 9, 12, 16, 19 };
            GroupingObservatorCollection<int, ObservableItem> actual = new GroupingObservatorCollection<int, ObservableItem>(list, x => x.Value / 5);
            CollectionListener events = CollectionListener.Create(actual);
            var groupRemoved = actual[0];
            list[0].Value = 13;

            int[][] expected = { new[] { 7, 9 }, new[] { 13, 12 }, new[] { 16, 19 }};
            AssertHelper.AreEqual(expected, actual);
            events.Expect(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, (object)groupRemoved, 0));
        }
    }
}
