using System.Collections.Specialized;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZumtenSoft.WpfUtils.Collections;
using ZumtenSoft.WpfUtils.Tests.Stubs;

namespace ZumtenSoft.WpfUtils.Tests.Collections
{
    [TestClass]
    public class ConcatenationObservatorCollectionTests
    {
        [TestMethod]
        public void Add_WhenInsertingMultipleItems_IndexShouldBePreserved()
        {
            ObservableCollection<int> list1 = new ObservableCollection<int> { 4, 7, 9 };
            ObservableCollection<int> list2 = new ObservableCollection<int> { 12, 16, 19 };
            ConcatenationObservatorCollection<int> actual = new ConcatenationObservatorCollection<int>(list1, list2);
            CollectionListener events = CollectionListener.Create(actual);
            list1.Add(10);
            list2.Insert(0, 11);

            int[] expected = new[] { 4, 7, 9, 10, 11, 12, 16, 19 };
            CollectionAssert.AreEqual(expected, actual);
            events.Expect(
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, 10, 3),
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, 11, 4));
        }

        [TestMethod]
        public void Remove_WhenRemovingMultipleItems_IndexShouldBePreserved()
        {
            ObservableCollection<int> list1 = new ObservableCollection<int> { 4, 7, 9 };
            ObservableCollection<int> list2 = new ObservableCollection<int> { 12, 16, 19 };
            ConcatenationObservatorCollection<int> actual = new ConcatenationObservatorCollection<int>(list1, list2);
            CollectionListener events = CollectionListener.Create(actual);
            list1.Remove(7);
            list2.RemoveAt(2);

            int[] expected = new[] { 4, 9, 12, 16 };
            CollectionAssert.AreEqual(expected, actual);
            events.Expect(
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, 7, 1),
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, 19, 4));
        }

        [TestMethod]
        public void Move_WhenRemovingMultipleItems_IndexShouldBePreserved()
        {
            ObservableCollection<int> list1 = new ObservableCollection<int> { 4, 7, 9 };
            ObservableCollection<int> list2 = new ObservableCollection<int> { 12, 16, 19 };
            ConcatenationObservatorCollection<int> actual = new ConcatenationObservatorCollection<int>(list1, list2);
            CollectionListener events = CollectionListener.Create(actual);
            list1.Move(0, 1);
            list2.Move(2, 0);

            int[] expected = new[] { 7, 4, 9, 19, 12, 16 };
            CollectionAssert.AreEqual(expected, actual);
            events.Expect(
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, 4, 1, 0),
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, 19, 3, 5));
        }

        [TestMethod]
        public void First_WhenUpdatingFirstCollection_FirstItemsShouldBeReplacedByNewCollectionItems()
        {
            ObservableCollection<int> list1 = new ObservableCollection<int> { 4, 7, 9 };
            ObservableCollection<int> list2 = new ObservableCollection<int> { 12, 16, 19 };
            ObservableCollection<int> newList1 = new ObservableCollection<int> { 20, 25, 29 };
            ConcatenationObservatorCollection<int> actual = new ConcatenationObservatorCollection<int>(list1, list2);
            CollectionListener events = CollectionListener.Create(actual);
            actual.First = newList1;

            int[] expected = new[] { 20, 25, 29, 12, 16, 19 };
            CollectionAssert.AreEqual(expected, actual);
            events.Expect(
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, list1.ToList(), 0),
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newList1.ToList(), 0));
        }


        [TestMethod]
        public void Second_WhenUpdatingSecondCollection_LastItemsShouldBeReplacedByNewCollectionItems()
        {
            ObservableCollection<int> list1 = new ObservableCollection<int> { 4, 7, 9 };
            ObservableCollection<int> list2 = new ObservableCollection<int> { 12, 16, 19 };
            ObservableCollection<int> newList2 = new ObservableCollection<int> { 20, 25, 29 };
            ConcatenationObservatorCollection<int> actual = new ConcatenationObservatorCollection<int>(list1, list2);
            CollectionListener events = CollectionListener.Create(actual);
            actual.Second = newList2;

            int[] expected = new[] { 4, 7, 9, 20, 25, 29 };
            CollectionAssert.AreEqual(expected, actual);
            events.Expect(
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, list2.ToList(), 3),
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newList2.ToList(), 3));
        }
    }
}
