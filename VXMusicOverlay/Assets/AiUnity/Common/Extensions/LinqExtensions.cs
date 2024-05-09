// ***********************************************************************
// Assembly         : Assembly-CSharp
// Author           : AiDesigner
// Created          : 06-20-2016
// Modified         : 07-23-2018
// ***********************************************************************
#if AIUNITY_CODE

using System;
using System.Collections.Generic;
using System.Linq;

namespace AiUnity.Common.Extensions
{
    /// <summary>
    /// Linq Extensions.
    /// </summary>
    public static class LinqExtensions
    {
        #region Methods
        /// <summary>
        /// Appends the specified value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values">The values.</param>
        /// <param name="value">The value.</param>
        /// <returns>IEnumerable&lt;T&gt;.</returns>
        public static IEnumerable<T> MyAppend<T>(this IEnumerable<T> list, T item)
        {
            foreach (var element in list)
            {
                yield return element;
            }
            yield return item;
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
        {
            return enumerable == null || !enumerable.Any();
        }

        /// <summary>
        /// Prepends the specified value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values">The values.</param>
        /// <param name="value">The value.</param>
        /// <returns>IEnumerable&lt;T&gt;.</returns>
        public static IEnumerable<T> MyPrepend<T>(this IEnumerable<T> values, T value)
        {
            yield return value;
            foreach (T item in values)
            {
                yield return item;
            }
        }

        /// <summary>
        /// Skips the last.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="count">The count.</param>
        /// <returns>IEnumerable&lt;T&gt;.</returns>
        public static IEnumerable<T> SkipLast<T>(this IEnumerable<T> source, int count = 1)
        {
            var enumerator = source.GetEnumerator();
            var queue = new Queue<T>(count + 1);
            while (true)
            {
                if (!enumerator.MoveNext()) break;
                queue.Enqueue(enumerator.Current);
                if (queue.Count > count) yield return queue.Dequeue();
            }
        }

        /// <summary>
        /// Skips items from the input sequence until the given predicate returns true
        /// when applied to the current source item; that item will be the last skipped.
        /// </summary>
        /// <typeparam name="TSource">Type of the source sequence</typeparam>
        /// <param name="source">Source sequence</param>
        /// <param name="predicate">Predicate used to determine when to stop yielding results from the source.</param>
        /// <returns>Items from the source sequence after the predicate first returns true when applied to the item.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// source
        /// or
        /// predicate
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="source" /> or <paramref name="predicate" /> is null</exception>
        public static IEnumerable<TSource> SkipUntil<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (predicate == null) throw new ArgumentNullException("predicate");

            using (var iterator = source.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    if (predicate(iterator.Current))
                    {
                        break;
                    }
                }
                while (iterator.MoveNext())
                {
                    yield return iterator.Current;
                }
            }
        }

        /// <summary>
        /// Wraps this object instance into an IEnumerable&lt;T&gt;
        /// consisting of a single item.
        /// </summary>
        /// <typeparam name="T">Type of the wrapped object.</typeparam>
        /// <param name="item">The object to wrap.</param>
        /// <returns>An IEnumerable&lt;T&gt; consisting of a single item.</returns>
        public static IEnumerable<T> Yield<T>(this T item)
        {
            if (item == null) yield break;
            yield return item;
        }

        /// <summary>
        /// Inserts items into a list using the reference list to determine order.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="reference">The reference.</param>
        /// <param name="items">The items.</param>
        public static void InsertRelative<T>(this IList<T> source, IList<T> reference, params T[] items)
        {
            int insertIndex;

            foreach (T item in items)
            {
                int tagPriority = reference.IndexOf(item);

                for (insertIndex = 0; insertIndex < source.Count(); insertIndex++)
                {
                    // Lower index represents higher priority
                    if (tagPriority >= 0 && (tagPriority < reference.IndexOf(source[insertIndex])))
                    {
                        break;
                    }
                }
                source.Insert(insertIndex, item);
            }
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default(TValue))
        {
            TValue value;
            return dictionary.TryGetValue(key, out value) ? value : defaultValue;
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> defaultValueProvider)
        {
            TValue value;
            return dictionary.TryGetValue(key, out value) ? value
                 : defaultValueProvider();
        }

        public static bool ScrambledEquals<T>(this IEnumerable<T> list1, IEnumerable<T> list2)
        {
            if (list2 == null || (list1.Count() != list2.Count()))
            {
                return false;
            }

            var cnt = new Dictionary<T, int>();

            foreach (T s in list1)
            {
                if (cnt.ContainsKey(s))
                {
                    cnt[s]++;
                }
                else
                {
                    cnt.Add(s, 1);
                }
            }
            foreach (T s in list2)
            {
                if (cnt.ContainsKey(s))
                {
                    cnt[s]--;
                }
                else
                {
                    return false;
                }
            }
            return cnt.Values.All(c => c == 0);
        }

        public static int IndexOf<T>(this IEnumerable<T> list, T item)
        {
            return list.Select((x, index) => EqualityComparer<T>.Default.Equals(item, x) ? index : -1)
                .FirstOrDefault(x => x != -1, -1);
        }

        public static T FirstOrDefault<T>(this IEnumerable<T> source, Func<T, bool> predicate, T alternate)
        {
            return source.Where(predicate).FirstOrDefault(alternate);
        }

        public static T FirstOrDefault<T>(this IEnumerable<T> source, T alternate)
        {
            return source.DefaultIfEmpty(alternate).First();
        }

        #endregion
    }
}

#endif