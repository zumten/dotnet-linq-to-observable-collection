using System.Collections.Specialized;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZumtenSoft.Linq2ObsCollection.Collections;
using ZumtenSoft.Linq2ObsCollection.Tests.Stubs;

namespace ZumtenSoft.Linq2ObsCollection.Tests.Collections
{
    [TestClass]
    public class SelectionObservatorCollectionTests
    {
        [TestMethod]
        public void Ctor_WhenCreatingNewCollection_OrderShouldBePreserved()
        {
            ObservableCollection<int> list = new ObservableCollection<int> { 4, 7, 9 };
            SelectionObservatorCollection<int, string> actual = new SelectionObservatorCollection<int, string>(list, x => x.ToString(CultureInfo.InvariantCulture));
            CollectionListener events = CollectionListener.Create(actual);

            string[] expected = new[] { "4", "7", "9" };

            CollectionAssert.AreEqual(expected, actual);
            events.Expect();
        }

        [TestMethod]
        public void Add_WhenInsertingMultipleItems_OrderShouldBePreserved()
        {
            ObservableCollection<int> list = new ObservableCollection<int> { 4, 6, 7, 9 };
            SelectionObservatorCollection<int, string> actual = new SelectionObservatorCollection<int, string>(list, x => x.ToString(CultureInfo.InvariantCulture));
            CollectionListener events = CollectionListener.Create(actual);
            list.Add(10);

            CollectionAssert.AreEqual(new[] { "4", "6", "7", "9", "10" }, actual);
            events.Expect(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, "10", 4));
        }

        [TestMethod]
        public void Remove_WhenRemovingMultipleItems_OrderShouldBePreserved()
        {
            ObservableCollection<int> list = new ObservableCollection<int> { 4, 7, 9, 10 };
            SelectionObservatorCollection<int, string> actual = new SelectionObservatorCollection<int, string>(list, x => x.ToString(CultureInfo.InvariantCulture));
            CollectionListener events = CollectionListener.Create(actual);
            list.Remove(10);

            CollectionAssert.AreEqual(new[] { "4", "7", "9" }, actual);
            events.Expect(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, "10", 3));
        }

        [TestMethod]
        public void Move_WhenRemovingMultipleItems_OrderShouldBePreserved()
        {
            ObservableCollection<int> list = new ObservableCollection<int> { 4, 6, 7, 9, 10 };
            SelectionObservatorCollection<int, string> actual = new SelectionObservatorCollection<int, string>(list, x => x.ToString(CultureInfo.InvariantCulture));
            CollectionListener events = CollectionListener.Create(actual);
            list.Move(1, 4);

            string[] expected = new[] { "4", "7", "9", "10", "6" };

            CollectionAssert.AreEqual(expected, actual);
            events.Expect(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, "6", 4, 1));
        }

        [TestMethod]
        public void Source_WhenUpdatingSource_CollectionShouldBeResetWithNewList()
        {
            ObservableCollection<int> list1 = new ObservableCollection<int> { 4, 6, 7, 9, 10 };
            ObservableCollection<int> list2 = new ObservableCollection<int> { 11, 12, 13 };
            SelectionObservatorCollection<int, string> actual = new SelectionObservatorCollection<int, string>(list1, x => x.ToString(CultureInfo.InvariantCulture));
            CollectionListener events = CollectionListener.Create(actual);
            actual.Source = list2;

            CollectionAssert.AreEqual(new[] { "11", "12", "13" }, actual);
            events.Expect(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
