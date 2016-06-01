using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqExtensions
{
    public static class LinqExtensions
    {
        /// <summary>
        /// Merges two sequenses.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sequense1"></param>
        /// <param name="sequense2"></param>
        /// <param name="compare"></param>
        /// <returns></returns>
        public static IEnumerable<T> Merge<T>(this IEnumerable<T> sequense1, IEnumerable<T> sequense2, Func<T, T, bool> compare)
        {
            using (var en1 = sequense1.GetEnumerator())
            using (var en2 = sequense2.GetEnumerator())
            {
                bool en1exists = en1.MoveNext();
                bool en2exists = en2.MoveNext();
                while (en1exists || en2exists)
                {
                    if (en1exists)
                    {
                        if (en2exists)
                        {
                            if (compare(en1.Current, en2.Current))
                            {
                                yield return en1.Current;
                                en1exists = en1.MoveNext();
                            }
                            else
                            {
                                yield return en2.Current;
                                en2exists = en2.MoveNext();
                            }
                        }
                        else
                        {
                            yield return en1.Current;
                            en1exists = en1.MoveNext();
                        }
                    }
                    else
                    {
                        if (en2exists)
                        {
                            yield return en2.Current;
                            en2exists = en2.MoveNext();
                        }
                    }
                }
            }
        }

        public static IEnumerable<T> MergeMin<T>(this IEnumerable<T> list1, IEnumerable<T> list2) where T : IComparable
        {
            return Merge(list1, list2, (a, b) => a.CompareTo(b) < 0);
        }

        public static IEnumerable<T> MergeMax<T>(this IEnumerable<T> list1, IEnumerable<T> list2) where T : IComparable
        {
            return Merge(list1, list2, (a, b) => a.CompareTo(b) > 0);
        }

        public static IEnumerable<T> Diff<T>(this IEnumerable<T> source, Func<T, T, T> diff)
        {
            return Enumerable.Repeat(source.First(), 1).Concat(source.Zip(source.Skip(1), (a, b) => diff(a, b)));
        }

        // median - от 0.0 до 1.0
        public static T[][] FilterByMedians<T>(this IEnumerable<T> source, params double[] medians)
        {
            var counts = new Dictionary<T, long>();
            long count = 0;
            foreach (var item in source)
            {
                if (!counts.ContainsKey(item))
                    counts.Add(item, 0);
                counts[item]++;
                count++;
            }
            long cnt = 0;
            var percents = new List<KeyValuePair<T, double>>();
            foreach (var kv in counts.OrderByDescending(c => c.Value))
            {
                cnt += kv.Value;
                percents.Add(new KeyValuePair<T, double>(kv.Key, (double)cnt / count));
            }
            var result = new List<T[]>();
            double prevM = 0.0;
            foreach (double m in medians)
            {
                result.Add(percents.Where(c => c.Value > prevM && c.Value <= m).Select(c => c.Key).ToArray());
                prevM = m;
            }
            return result.ToArray();
        }

        // median - от 0.0 до 1.0
        public static IEnumerable<T> FilterByMedian<T>(this IEnumerable<T> source, double median)
        {
            return source.FilterByMedians(median)[0];
        }

    }
}
