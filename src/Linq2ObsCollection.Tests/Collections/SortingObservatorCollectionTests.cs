using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZumtenSoft.Linq2ObsCollection.Collections;
using ZumtenSoft.Linq2ObsCollection.Tests.Stubs;

namespace ZumtenSoft.Linq2ObsCollection.Tests.Collections
{
    [TestClass]
    public class SortingObservatorCollectionTests
    {
        [TestMethod]
        public void Ctor_InitializingMultipleOrderedItems_ResultShouldNotChange()
        {
            ObservableCollection<int> list = new ObservableCollection<int> { 1, 2, 3, 6, 8, 9 };
            SortingObservatorCollection<int> actual = new SortingObservatorCollection<int>(list);
            CollectionListener events = CollectionListener.Create(actual);

            int[] expected = new[] { 1, 2, 3, 6, 8, 9 };
            CollectionAssert.AreEqual(expected, actual);
            events.Expect();
        }

        [TestMethod]
        public void Ctor_InitializingMultipleUnorderedItems_OrderShouldBeUpdated()
        {
            ObservableCollection<int> list = new ObservableCollection<int> { 1, 8, 3, 2, 6, 9 };
            SortingObservatorCollection<int> actual = new SortingObservatorCollection<int>(list);
            CollectionListener events = CollectionListener.Create(actual);

            int[] expected = new[] { 1, 2, 3, 6, 8, 9 };
            CollectionAssert.AreEqual(expected, actual);
            events.Expect();
        }

        [TestMethod]
        public void Add_WhenAddingOrderedItem_ResultShouldNotChange()
        {
            ObservableCollection<int> list = new ObservableCollection<int> { 1, 6, 10 };
            SortingObservatorCollection<int> actual = new SortingObservatorCollection<int>(list);
            CollectionListener events = CollectionListener.Create(actual);
            list.Insert(2, 8);

            CollectionAssert.AreEqual(new[] { 1, 6, 8, 10 }, actual);
            events.Expect(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, 8, 2));
        }

        [TestMethod]
        public void Add_WhenAddingUnorderedItem_OrderShouldBeUpdated()
        {
            ObservableCollection<int> list = new ObservableCollection<int> { 1, 6, 10 };
            SortingObservatorCollection<int> actual = new SortingObservatorCollection<int>(list);
            CollectionListener events = CollectionListener.Create(actual);
            list.Insert(1, 100);

            CollectionAssert.AreEqual(new[] { 1, 6, 10, 100 }, actual);
            events.Expect(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, 100, 3));
        }

        [TestMethod]
        public void Add_WhenAddingMultipleItems_OrderShouldBeUpdated()
        {
            ObservableCollection<int> list = new ObservableCollection<int> { 6, 10, 11, 1, 13 };
            SortingObservatorCollection<int> actual = new SortingObservatorCollection<int>(list);
            list.Add(25);
            list.Add(24);
            list.Insert(0, 12);
            list.Insert(3, 50);

            int[] expected = new[] { 1, 6, 10, 11, 12, 13, 24, 25, 50 };

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Remove_WhenRemovingItems_OrderShouldBeMaintained()
        {
            ObservableCollection<int> list = new ObservableCollection<int> { 1, 6, 10, 11, 13, 18 };
            SortingObservatorCollection<int> actual = new SortingObservatorCollection<int>(list);
            list.Remove(11);
            list.RemoveAt(1);

            int[] expected = new[] { 1, 10, 13, 18 };

            CollectionAssert.AreEqual(expected, actual);
        }
        
        [TestMethod]
        public void Move_WhenMovingItems_ResultShouldNotChange()
        {
            ObservableCollection<int> list = new ObservableCollection<int> { 1, 6, 7, 10, 11 };
            SortingObservatorCollection<int> actual = new SortingObservatorCollection<int>(list);
            CollectionListener events = CollectionListener.Create(actual);
            list.Move(2, 4);

            int[] expected = { 1, 6, 7, 10, 11 };

            CollectionAssert.AreEqual(expected, actual);
            events.Expect();
        }

        [TestMethod]
        public void Replace_WhenReplacingItems_OrderShouldBeMaintained()
        {
            ObservableCollection<int> list = new ObservableCollection<int> { 1, 6, 7, 10, 11 };
            SortingObservatorCollection<int> actual = new SortingObservatorCollection<int>(list);
            CollectionListener events = CollectionListener.Create(actual);
            list[0] = 25;

            CollectionAssert.AreEqual(new[] { 6, 7, 10, 11, 25 }, actual);
            events.Expect(
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, 1, 0),
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, 25, 4));
        }

        [TestMethod]
        public void Add_WhenAddingMultipleItemsRandomly_OrderShouldBePreserved()
        {
            ObservableCollection<int> list = new ObservableCollection<int>();
            SortingObservatorCollection<int> actual = new SortingObservatorCollection<int>(list);
            actual.TryInitialize(); // Force initialization
            Random rnd = new Random();
            for (int i = 0; i < 1000; i++)
                list.Insert(rnd.Next(0, i), i);

            int[] expected = ((ICollection<int>)list).OrderBy(x => x).ToArray();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Add_WhenAddingMultipleObservableItemsRandomly_OrderShouldBePreserved()
        {
            ObservableCollection<ObservableItem> list = new ObservableCollection<ObservableItem>();
            SortingObservatorCollection<ObservableItem> actual = list.OrderBy(x => x.Value);
            actual.TryInitialize(); // Force initialization
            Random rnd = new Random();
            for (int i = 0; i < 1000; i++)
                list.Insert(rnd.Next(0, i), new ObservableItem(rnd.Next()) );
            foreach (ObservableItem item in list)
                item.Value = rnd.Next();

            ObservableItem[] expected = ((ICollection<ObservableItem>)list).OrderBy(x => x.Value).ToArray();
            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
