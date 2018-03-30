using System.Collections.Specialized;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZumtenSoft.WpfUtils.Collections;
using ZumtenSoft.WpfUtils.Tests.Stubs;

namespace ZumtenSoft.WpfUtils.Tests.Collections
{
    [TestClass]
    public class RangeObservatorCollectionTests
    {
        [TestMethod]
        public void Ctor_WhenCreatingNewRangeCollectionWithLargerNumbers_ItemsShouldBeTruncatedOnBothSides()
        {
            ObservableCollection<int> list = new ObservableCollection<int> { 1, 2, 3, 4, 5, 6, 7, 8 };
            RangeObservatorCollection<int> actual = new RangeObservatorCollection<int>(list, 3, 4);
            CollectionListener events = CollectionListener.Create(actual);

            int[] expected = new[] { 4, 5, 6, 7 };

            CollectionAssert.AreEqual(expected, actual);
            events.Expect();
        }

        [TestMethod]
        public void Ctor_WhenCreatingNewRangeCollectionWithoutFilter_ItemsShouldRemainTheSame()
        {
            ObservableCollection<int> list = new ObservableCollection<int> { 1, 2, 3, 4 };
            RangeObservatorCollection<int> actual = new RangeObservatorCollection<int>(list, null, null);
            CollectionListener events = CollectionListener.Create(actual);

            int[] expected = new[] { 1, 2, 3, 4 };

            CollectionAssert.AreEqual(expected, actual);
            events.Expect();
        }

        [TestMethod]
        public void Ctor_WhenCreatingNewRangeCollectionWithOnlySkipFilter_ItemsShouldBeTruncatedAtTheBeginning()
        {
            ObservableCollection<int> list = new ObservableCollection<int> { 1, 2, 3, 4, 5, 6 };
            RangeObservatorCollection<int> actual = new RangeObservatorCollection<int>(list, 2, null);
            CollectionListener events = CollectionListener.Create(actual);

            int[] expected = new[] { 3, 4, 5, 6 };

            CollectionAssert.AreEqual(expected, actual);
            events.Expect();
        }

        [TestMethod]
        public void Ctor_WhenCreatingNewRangeCollectionWithOnlyTakeFilter_ItemsShouldBeTruncatedAtTheEnd()
        {
            ObservableCollection<int> list = new ObservableCollection<int> { 1, 2, 3, 4, 5, 6 };
            RangeObservatorCollection<int> actual = new RangeObservatorCollection<int>(list, null, 4);
            CollectionListener events = CollectionListener.Create(actual);

            int[] expected = new[] { 1, 2, 3, 4 };

            CollectionAssert.AreEqual(expected, actual);
            events.Expect();
        }

        [TestMethod]
        public void Add_WhenAddingMultipleItemAtTheBeginning_BothRemoveAndAddEventShouldBeFired()
        {
            ObservableCollection<int> list = new ObservableCollection<int> { 1, 2, 3, 4, 5, 6, 7, 8 };
            RangeObservatorCollection<int> actual = new RangeObservatorCollection<int>(list, 2, 4);
            CollectionListener events = CollectionListener.Create(actual);
            list.InsertRange(1, new int[] { 20, 21, 22 });

            int[] expected = new[] { 21, 22, 2, 3 };

            CollectionAssert.AreEqual(expected, actual);
            events.Expect(
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new int[] { 4, 5, 6 }, 1),
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new int[] { 21, 22, 2 }, 0));
        }

        [TestMethod]
        public void Add_WhenAddingMultipleItemInTheMiddle_BothRemoveAndAddEventShouldBeFired()
        {
            ObservableCollection<int> list = new ObservableCollection<int> { 1, 2, 3, 4, 5, 6, 7, 8 };
            RangeObservatorCollection<int> actual = new RangeObservatorCollection<int>(list, 2, 4);
            CollectionListener events = CollectionListener.Create(actual);
            list.InsertRange(4, new int[] {20, 21, 22, 23, 24, 25});

            int[] expected = new[] { 3, 4, 20, 21 };

            CollectionAssert.AreEqual(expected, actual);
            events.Expect(
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new int[] { 5, 6 }, 2),
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new int[] { 20, 21 }, 2));
        }

        [TestMethod]
        public void Add_WhenAddingItemInTheMiddle_BothRemoveAndAddEventShouldBeFired()
        {
            ObservableCollection<int> list = new ObservableCollection<int> { 1, 2, 3, 4, 5, 6, 7, 8 };
            RangeObservatorCollection<int> actual = new RangeObservatorCollection<int>(list, 2, 4);
            CollectionListener events = CollectionListener.Create(actual);
            list.Insert(4, 20);

            int[] expected = new[] { 3, 4, 20, 5 };

            CollectionAssert.AreEqual(expected, actual);
            events.Expect(
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, 6, 3),
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, 20, 2));
        }

        [TestMethod]
        public void Add_WhenAddingItemAtTheEnd_NoEventShouldBeFired()
        {
            ObservableCollection<int> list = new ObservableCollection<int> { 1, 2, 3, 4, 5, 6, 7, 8 };
            RangeObservatorCollection<int> actual = new RangeObservatorCollection<int>(list, 2, 4);
            CollectionListener events = CollectionListener.Create(actual);
            list.Add(9);

            int[] expected = new[] { 3, 4, 5, 6 };

            CollectionAssert.AreEqual(expected, actual);
            events.Expect();
        }
    }
}
