using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZumtenSoft.WpfUtils.Collections;
using ZumtenSoft.WpfUtils.Tests.Stubs;

namespace ZumtenSoft.WpfUtils.Tests.Collections
{
    [TestClass]
    public class TypeFilteringObservatorCollectionTests
    {
        public interface IBaseInterface
        {
            int Value { get; }
        }

        public class FirstImplementation : IBaseInterface
        {
            public FirstImplementation(int value)
            {
                Value = value;
            }

            public int Value { get; private set; }
        }

        public class SecondImplementation : IBaseInterface
        {
            public SecondImplementation(int value)
            {
                Value = value;
            }

            public int Value { get; private set; }
        }

        [TestMethod]
        public void Ctor_WhenInstanciatingList_OnlyFirstImplementationItemsShouldPassThrough()
        {
            ObservableCollection<IBaseInterface> list = new ObservableCollection<IBaseInterface>
            {
                new FirstImplementation(5),
                new SecondImplementation(10),
                new SecondImplementation(15)
            };
            TypeFilteringObservatorCollection<FirstImplementation> actual = new TypeFilteringObservatorCollection<FirstImplementation>(list);
            CollectionListener events = CollectionListener.Create(actual);

            FirstImplementation[] expected = ((IList)list).OfType<FirstImplementation>().ToArray();
            CollectionAssert.AreEqual(expected, actual);
            events.Expect();
        }

        [TestMethod]
        public void Add_WhenInsertingValidTypeItem_EventShouldBeFired()
        {
            ObservableCollection<IBaseInterface> list = new ObservableCollection<IBaseInterface>
            {
                new FirstImplementation(5),
                new SecondImplementation(10),
                new SecondImplementation(15)
            };
            TypeFilteringObservatorCollection<FirstImplementation> actual = new TypeFilteringObservatorCollection<FirstImplementation>(list);
            CollectionListener events = CollectionListener.Create(actual);
            list.Add(new FirstImplementation(20));

            FirstImplementation[] expected = ((IList)list).OfType<FirstImplementation>().ToArray();
            CollectionAssert.AreEqual(expected, actual);
            events.Expect(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, list[3], 1));
        }


        [TestMethod]
        public void Add_WhenInsertingInvalidTypeItem_EventShouldNotBeFired()
        {
            ObservableCollection<IBaseInterface> list = new ObservableCollection<IBaseInterface>
            {
                new FirstImplementation(5),
                new SecondImplementation(10),
                new SecondImplementation(15)
            };
            TypeFilteringObservatorCollection<FirstImplementation> actual = new TypeFilteringObservatorCollection<FirstImplementation>(list);
            CollectionListener events = CollectionListener.Create(actual);
            list.Add(new SecondImplementation(20));

            FirstImplementation[] expected = ((IList)list).OfType<FirstImplementation>().ToArray();
            CollectionAssert.AreEqual(expected, actual);
            events.Expect();
        }

        [TestMethod]
        public void Remove_WhenRemovingValidItem_EventShouldBeFired()
        {
            ObservableCollection<IBaseInterface> list = new ObservableCollection<IBaseInterface>
            {
                new FirstImplementation(5),
                new SecondImplementation(10),
                new SecondImplementation(15),
                new FirstImplementation(20)
            };
            TypeFilteringObservatorCollection<FirstImplementation> actual = new TypeFilteringObservatorCollection<FirstImplementation>(list);
            CollectionListener events = CollectionListener.Create(actual);
            IBaseInterface itemRemoved = list[3];
            list.RemoveAt(3);

            FirstImplementation[] expected = ((IList)list).OfType<FirstImplementation>().ToArray();
            CollectionAssert.AreEqual(expected, actual);
            events.Expect(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, itemRemoved, 1));
        }


        [TestMethod]
        public void Remove_WhenRemovingInvalidItem_EventShouldNotBeFired()
        {
            ObservableCollection<IBaseInterface> list = new ObservableCollection<IBaseInterface>
            {
                new FirstImplementation(5),
                new SecondImplementation(10),
                new SecondImplementation(15),
                new FirstImplementation(20)
            };
            TypeFilteringObservatorCollection<FirstImplementation> actual = new TypeFilteringObservatorCollection<FirstImplementation>(list);
            CollectionListener events = CollectionListener.Create(actual);
            list.RemoveAt(2);

            FirstImplementation[] expected = ((IList)list).OfType<FirstImplementation>().ToArray();
            CollectionAssert.AreEqual(expected, actual);
            events.Expect();
        }

        [TestMethod]
        public void Move_WhenMovingValidItem_EventShouldBeFired()
        {
            ObservableCollection<IBaseInterface> list = new ObservableCollection<IBaseInterface>
            {
                new FirstImplementation(5),
                new SecondImplementation(10),
                new SecondImplementation(15),
                new FirstImplementation(20)
            };
            TypeFilteringObservatorCollection<FirstImplementation> actual = new TypeFilteringObservatorCollection<FirstImplementation>(list);
            CollectionListener events = CollectionListener.Create(actual);
            IBaseInterface movedItem = list[0];
            list.Move(0, 3);

            FirstImplementation[] expected = ((IList)list).OfType<FirstImplementation>().ToArray();
            CollectionAssert.AreEqual(expected, actual);
            events.Expect(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, movedItem, 1, 0));
        }


        [TestMethod]
        public void Move_WhenMovingInvalidItem_EventShouldNotBeFired()
        {
            ObservableCollection<IBaseInterface> list = new ObservableCollection<IBaseInterface>
            {
                new FirstImplementation(5),
                new SecondImplementation(10),
                new SecondImplementation(15),
                new FirstImplementation(20)
            };
            TypeFilteringObservatorCollection<FirstImplementation> actual = new TypeFilteringObservatorCollection<FirstImplementation>(list);
            CollectionListener events = CollectionListener.Create(actual);
            list.Move(1, 3);

            FirstImplementation[] expected = ((IList)list).OfType<FirstImplementation>().ToArray();
            CollectionAssert.AreEqual(expected, actual);
            events.Expect();
        }

        [TestMethod]
        public void Replace_WhenReplacingValidItemWithInvalidItem_ItemShouldBeRemoved()
        {
            ObservableCollection<IBaseInterface> list = new ObservableCollection<IBaseInterface>
            {
                new FirstImplementation(5),
                new SecondImplementation(10),
                new SecondImplementation(15),
                new FirstImplementation(20)
            };
            TypeFilteringObservatorCollection<FirstImplementation> actual = new TypeFilteringObservatorCollection<FirstImplementation>(list);
            CollectionListener events = CollectionListener.Create(actual);
            IBaseInterface itemRemoved = list[0];
            list[0] = new SecondImplementation(5);

            FirstImplementation[] expected = ((IList)list).OfType<FirstImplementation>().ToArray();
            CollectionAssert.AreEqual(expected, actual);
            events.Expect(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, itemRemoved, 0));
        }

        [TestMethod]
        public void Replace_WhenReplacingInvalidItemWithValidItem_ItemShouldBeAdded()
        {
            ObservableCollection<IBaseInterface> list = new ObservableCollection<IBaseInterface>
            {
                new FirstImplementation(5),
                new SecondImplementation(10),
                new SecondImplementation(15),
                new FirstImplementation(20)
            };
            TypeFilteringObservatorCollection<FirstImplementation> actual = new TypeFilteringObservatorCollection<FirstImplementation>(list);
            CollectionListener events = CollectionListener.Create(actual);
            list[1] = new FirstImplementation(10);

            FirstImplementation[] expected = ((IList)list).OfType<FirstImplementation>().ToArray();
            CollectionAssert.AreEqual(expected, actual);
            events.Expect(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, list[1], 1));
        }
    }
}
