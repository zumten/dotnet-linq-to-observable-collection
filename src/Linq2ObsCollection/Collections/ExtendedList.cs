using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ZumtenSoft.Linq2ObsCollection.Collections
{
    /// <summary>
    /// List with the ability to move elements in an optimized way.
    /// </summary>
    internal class ExtendedList<T> : List<T>
    {
        public ExtendedList()
        {
            
        }

        public ExtendedList(IEnumerable<T> collection) : base(collection)
        {
            
        }

        private static Func<TClass, TField> FieldGetter<TClass, TField>(string name)
        {
            ParameterExpression param = Expression.Parameter(typeof(TClass), "arg");
            MemberExpression member = Expression.Field(param, name);
            LambdaExpression lambda = Expression.Lambda(typeof(Func<TClass, TField>), member, param);
            Func<TClass, TField> compiled = (Func<TClass, TField>)lambda.Compile();
            return compiled;
        }

        private static readonly Func<List<T>, T[]> ItemsGetter = FieldGetter<List<T>, T[]>("_items");

        public void Move(int oldIndex, int newIndex)
        {
            if (oldIndex < 0 || oldIndex >= Count || newIndex < 0 || newIndex >= Count)
                throw new IndexOutOfRangeException();

            if (oldIndex != newIndex)
            {
                // If the elements are right next to each-other, just swap them
                if (oldIndex == newIndex + 1 || oldIndex + 1 == newIndex)
                {
                    T item = this[oldIndex];
                    this[oldIndex] = this[newIndex];
                    this[newIndex] = item;
                }

                else if (newIndex > oldIndex)
                {
                    T[] items = ItemsGetter(this);
                    T item = items[oldIndex];

                    // If newIndex is greater, we have to move elements to the left
                    Array.Copy(items, oldIndex + 1, items, oldIndex, newIndex - oldIndex);
                    items[newIndex] = item;
                }
                else
                {
                    T[] items = ItemsGetter(this);
                    T item = items[oldIndex];

                    // If newIndex is lower, we have to move elements to the right
                    Array.Copy(items, newIndex, items, newIndex + 1, oldIndex - newIndex);
                    items[newIndex] = item;
                }
            }
        }

        public void MoveRange(int oldIndex, int newIndex, int count)
        {
            if (count == 1)
            {
                // If we have a single element to move, let's just use Move instead of MoveRange
                Move(oldIndex, newIndex);
            }
            else if (count > 0 && oldIndex != newIndex)
            {
                if (oldIndex < 0 || oldIndex > Count - count || newIndex < 0 || newIndex > Count - count)
                    throw new IndexOutOfRangeException();

                T[] items = ItemsGetter(this);

                // If we are moving by only 1 item, we can handle with with a single Array.Copy
                if (oldIndex - 1 == newIndex) // oldIndex > newIndex
                {
                    T temp = this[newIndex];
                    Array.Copy(items, oldIndex, items, newIndex, count);
                    this[newIndex + count] = temp;
                }
                else if (oldIndex + 1 == newIndex) // oldIndex < newIndex
                {
                    T temp = this[oldIndex + count];
                    Array.Copy(items, oldIndex, items, newIndex, count);
                    this[oldIndex] = temp;
                }
                else if (oldIndex > newIndex)
                {
                    T[] temp = new T[oldIndex - newIndex];
                    Array.Copy(items, newIndex, temp, 0, temp.Length);
                    Array.Copy(items, oldIndex, items, newIndex, count);
                    Array.Copy(temp, 0, items, newIndex + count, temp.Length);
                }
                else
                {
                    T[] temp = new T[newIndex - oldIndex];
                    Array.Copy(items, oldIndex + count, temp, 0, temp.Length);
                    Array.Copy(items, oldIndex, items, newIndex, count);
                    Array.Copy(temp, 0, items, oldIndex, temp.Length);
                }
            }
        }
    }
}
