using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZumtenSoft.Linq2ObsCollection.Collections;
using ZumtenSoft.Linq2ObsCollection.Tests.Stubs;

namespace ZumtenSoft.Linq2ObsCollection.Tests.Collections
{
    [TestClass]
    public class CastingObservatorCollectionTests
    {
        [TestMethod]
        public void Ctor_WhenInstanciatingList_IndexShouldBePreserved()
        {
            ObservableCollection<ObservableItem> list = new ObservableCollection<ObservableItem> { 4, 7, 9 };
            CastingObservatorCollection<INotifyPropertyChanged> actual = new CastingObservatorCollection<INotifyPropertyChanged>(list);
            CollectionListener events = CollectionListener.Create(actual);

            INotifyPropertyChanged[] expected = ((IList)list).Cast<INotifyPropertyChanged>().ToArray();
            CollectionAssert.AreEqual(expected, actual);
            events.Expect();
        }

        [TestMethod]
        public void Add_WhenInsertingIntoTheSourceList_IndexShouldBePreserved()
        {
            ObservableCollection<ObservableItem> list = new ObservableCollection<ObservableItem> { 4, 7, 9 };
            CastingObservatorCollection<INotifyPropertyChanged> actual = new CastingObservatorCollection<INotifyPropertyChanged>(list);
            CollectionListener events = CollectionListener.Create(actual);
            list.Add(19);

            INotifyPropertyChanged[] expected = ((IList)list).Cast<INotifyPropertyChanged>().ToArray();
            CollectionAssert.AreEqual(expected, actual);
            events.Expect(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new ObservableItem(19), 3));
        }

        [TestMethod]
        public void Remove_WhenInsertingFromTheSourceList_IndexShouldBePreserved()
        {
            ObservableCollection<ObservableItem> list = new ObservableCollection<ObservableItem> { 4, 7, 9 };
            CastingObservatorCollection<INotifyPropertyChanged> actual = new CastingObservatorCollection<INotifyPropertyChanged>(list);
            CollectionListener events = CollectionListener.Create(actual);
            list.RemoveAt(1);

            INotifyPropertyChanged[] expected = ((IList)list).Cast<INotifyPropertyChanged>().ToArray();
            CollectionAssert.AreEqual(expected, actual);
            events.Expect(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new ObservableItem(7), 1));
        }

        [TestMethod]
        public void Move_WhenMovingFromTheSourceList_IndexShouldBePreserved()
        {
            ObservableCollection<ObservableItem> list = new ObservableCollection<ObservableItem> { 4, 7, 9 };
            CastingObservatorCollection<INotifyPropertyChanged> actual = new CastingObservatorCollection<INotifyPropertyChanged>(list);
            CollectionListener events = CollectionListener.Create(actual);
            list.Move(1, 2);

            INotifyPropertyChanged[] expected = ((IList)list).Cast<INotifyPropertyChanged>().ToArray();
            CollectionAssert.AreEqual(expected, actual);
            events.Expect(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, new ObservableItem(7), 2, 1));
        }
    }
}
