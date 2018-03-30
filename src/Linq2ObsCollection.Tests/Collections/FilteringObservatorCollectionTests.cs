using System.Collections.Specialized;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZumtenSoft.Linq2ObsCollection.Collections;
using ZumtenSoft.Linq2ObsCollection.Tests.Stubs;

namespace ZumtenSoft.Linq2ObsCollection.Tests.Collections
{
    [TestClass]
    public class FilteringObservatorCollectionTests
    {
        [TestMethod]
        public void Ctor_WhenCreatingNewCollectionWithPairComparison_OnlyPairNumbersShouldBeVisible()
        {
            ObservableCollection<int> list = new ObservableCollection<int> { 4, 7, 9 };
            FilteringObservatorCollection<int> actual = new FilteringObservatorCollection<int>(list, x => x % 2 == 0);
            CollectionListener events = CollectionListener.Create(actual);
            
            int[] expected = new[] { 4 };

            CollectionAssert.AreEqual(expected, actual);
            events.Expect();
        }

        [TestMethod]
        public void Add_WhenAddingPairNumber_EventShouldBeFired()
        {
            ObservableCollection<int> list = new ObservableCollection<int> { 4, 7, 6, 9 };
            FilteringObservatorCollection<int> actual = new FilteringObservatorCollection<int>(list, x => x % 2 == 0);
            CollectionListener events = CollectionListener.Create(actual);
            list.Add(10);

            CollectionAssert.AreEqual(new[] { 4, 6, 10 }, actual);
            events.Expect(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, 10, 2));
        }

        [TestMethod]
        public void Add_WhenAddingImpairNumber_EventShouldNotBeFired()
        {
            ObservableCollection<int> list = new ObservableCollection<int> { 4, 7, 6, 9 };
            FilteringObservatorCollection<int> actual = new FilteringObservatorCollection<int>(list, x => x % 2 == 0);
            CollectionListener events = CollectionListener.Create(actual);
            list.Add(11);
            
            CollectionAssert.AreEqual(new[] { 4, 6 }, actual);
            events.Expect();
        }

        [TestMethod]
        public void Remove_WhenRemovingPairNumber_EventShouldBeFired()
        {
            ObservableCollection<int> list = new ObservableCollection<int> { 4, 7, 9, 10 };
            FilteringObservatorCollection<int> actual = new FilteringObservatorCollection<int>(list, x => x % 2 == 0);
            CollectionListener events = CollectionListener.Create(actual);
            list.Remove(10);

            CollectionAssert.AreEqual(new[] { 4 }, actual);
            events.Expect(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, 10, 1));
        }

        [TestMethod]
        public void Remove_WhenRemovingImpairNumber_EventShouldNotBeFired()
        {
            ObservableCollection<int> list = new ObservableCollection<int> { 4, 6, 7, 9, 10 };
            FilteringObservatorCollection<int> actual = new FilteringObservatorCollection<int>(list, x => x % 2 == 0);
            list.RemoveAt(1);
            CollectionListener events = CollectionListener.Create(actual);
            list.Remove(7);

            CollectionAssert.AreEqual(new[] { 4, 10 }, actual);
            events.Expect();
        }

        [TestMethod]
        public void Move_WhenMovingPairNumber_EventShouldBeFired()
        {
            ObservableCollection<int> list = new ObservableCollection<int> { 4, 6, 7, 9, 10 };
            FilteringObservatorCollection<int> actual = new FilteringObservatorCollection<int>(list, x => x % 2 == 0);
            CollectionListener events = CollectionListener.Create(actual);
            list.Move(0, 3);

            CollectionAssert.AreEqual(new[] { 6, 4, 10 }, actual);
            events.Expect(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, 4, 1, 0));
        }

        [TestMethod]
        public void Move_WhenMovingImpairNumber_EventShouldNotBeFired()
        {
            ObservableCollection<int> list = new ObservableCollection<int> { 4, 6, 7, 9, 10 };
            FilteringObservatorCollection<int> actual = new FilteringObservatorCollection<int>(list, x => x % 2 == 0);
            CollectionListener events = CollectionListener.Create(actual);
            list.Move(2, 0);

            int[] expected = new[] { 4, 6, 10 };

            CollectionAssert.AreEqual(expected, actual);
            events.Expect();
        }

        [TestMethod]
        public void PropertyChanged_WhenItemBecomesVisible_EventShouldBeFired()
        {
            ObservableCollection<ObservableItem> list = new ObservableCollection<ObservableItem> { 4, 6, 7, 9, 10 };
            FilteringObservatorCollection<ObservableItem> actual = new FilteringObservatorCollection<ObservableItem>(list, x => x.Value % 2 == 0);
            CollectionListener events = CollectionListener.Create(actual);
            list[2].Value = 8;

            CollectionAssert.AreEqual(new ObservableItem[] { 4, 6, 8, 10 }, actual);
            events.Expect(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, (ObservableItem)8, 2));
        }

        [TestMethod]
        public void PropertyChanged_WhenItemBecomesInvisible_EventShouldBeFired()
        {
            ObservableCollection<ObservableItem> list = new ObservableCollection<ObservableItem> { 4, 6, 7, 9, 10 };
            FilteringObservatorCollection<ObservableItem> actual = new FilteringObservatorCollection<ObservableItem>(list, x => x.Value % 2 == 0);
            CollectionListener events = CollectionListener.Create(actual);
            list[1].Value = 5;

            CollectionAssert.AreEqual(new ObservableItem[] { 4, 10 }, actual);
            events.Expect(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, (ObservableItem)5, 1));
        }


        [TestMethod]
        public void Source_WhenUpdatingSource_CollectionShouldBeResetWithNewList()
        {
            ObservableCollection<int> list1 = new ObservableCollection<int> { 4, 6, 7, 9, 10 };
            ObservableCollection<int> list2 = new ObservableCollection<int> { 10, 11, 12 };
            FilteringObservatorCollection<int> actual = new FilteringObservatorCollection<int>(list1, x => x % 2 == 0);
            CollectionListener events = CollectionListener.Create(actual);
            actual.Source = list2;

            CollectionAssert.AreEqual(new[] { 10, 12 }, actual);
            events.Expect(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        [TestMethod]
        public void Predicate_WhenUpdatingPredicate_CollectionShouldBeResetWithNewList()
        {
            ObservableCollection<int> list1 = new ObservableCollection<int> { 4, 6, 7, 9, 10 };
            FilteringObservatorCollection<int> actual = new FilteringObservatorCollection<int>(list1, x => x % 2 == 0);
            CollectionListener events = CollectionListener.Create(actual);
            actual.Predicate = x => x % 2 == 1;

            CollectionAssert.AreEqual(new[] { 7, 9 }, actual);
            events.Expect(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

    }
}
