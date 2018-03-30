using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZumtenSoft.WpfUtils.Collections;

namespace ZumtenSoft.WpfUtils.Tests.Collections
{
    [TestClass]
    public class ExtendedListTests
    {
        [TestMethod]
        public void Move_MovingToGreaterPosition_ItemShouldBeMoved()
        {
            ExtendedList<int> actual = new ExtendedList<int> { 4, 7, 9, 12, 16, 19 };
            actual.Move(1, 4);

            int[] expected = new[] { 4, 9, 12, 16, 7, 19 };
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Move_MovingToLowerPosition_ItemShouldBeMoved()
        {
            ExtendedList<int> actual = new ExtendedList<int> { 4, 7, 9, 12, 16, 19 };
            actual.Move(4, 1);

            int[] expected = new[] { 4, 16, 7, 9, 12, 19 };
            CollectionAssert.AreEqual(expected, actual);
        }


        [TestMethod]
        public void MoveRange_MovingToGreaterPosition_ItemsShouldBeMoved()
        {
            ExtendedList<int> actual = new ExtendedList<int> { 4, 7, 9, 12, 16, 19 };
            actual.MoveRange(1, 4, 2);

            int[] expected = new[] { 4, 12, 16, 19, 7, 9 };
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MoveRange_MovingToLowerPosition_ItemsShouldBeMoved()
        {
            ExtendedList<int> actual = new ExtendedList<int> { 4, 7, 9, 12, 16, 19 };
            actual.MoveRange(4, 1, 2);

            int[] expected = new[] { 4, 16, 19, 7, 9, 12 };
            CollectionAssert.AreEqual(expected, actual);
        }


        [TestMethod]
        public void MoveRange_MovingFromOnePositionForward_ItemsShouldBeMoved()
        {
            ExtendedList<int> actual = new ExtendedList<int> { 4, 7, 9, 12, 16, 19 };
            actual.MoveRange(1, 2, 3);

            int[] expected = new[] { 4, 16, 7, 9, 12, 19 };
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MoveRange_MovingFromOnePositionBackward_ItemsShouldBeMoved()
        {
            ExtendedList<int> actual = new ExtendedList<int> { 4, 7, 9, 12, 16, 19 };
            actual.MoveRange(2, 1, 3);

            int[] expected = new[] { 4, 9, 12, 16, 7, 19 };
            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
