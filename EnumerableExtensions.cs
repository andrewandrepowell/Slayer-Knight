using System;
using System.Collections.Generic;
using System.Linq;

namespace Utility
{
    // https://stackoverflow.com/questions/7476174/foreach-loop-determine-which-is-the-last-iteration-of-the-loop
    // See Daniel Wolf and Fabricio Godoy.
    public static class EnumerableExtensions
    {
        public static IEnumerable<IterationElement<T>> Detailed<T>(this IEnumerable<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            using (var enumerator = source.GetEnumerator())
            {
                bool isFirst = true;
                bool hasNext = enumerator.MoveNext();
                int index = 0;
                while (hasNext)
                {
                    T current = enumerator.Current;
                    hasNext = enumerator.MoveNext();
                    yield return new IterationElement<T>(index, current, isFirst, !hasNext);
                    isFirst = false;
                    index++;
                }
            }
        }

        public struct IterationElement<T>
        {
            public int Index { get; }
            public bool IsFirst { get; }
            public bool IsLast { get; }
            public T Value { get; }

            public IterationElement(int index, T value, bool isFirst, bool isLast)
            {
                Index = index;
                IsFirst = isFirst;
                IsLast = isLast;
                Value = value;
            }
        }

        // https://stackoverflow.com/questions/5132758/words-combinations-without-repetition
        // See jrbeverly's response.
        public static IEnumerable<IEnumerable<T>> GetPermutations<T>(this IEnumerable<T> items, int count)
        {
            int i = 0;
            foreach (var item in items)
            {
                if (count == 1)
                    yield return new T[] { item };
                else
                {
                    foreach (var result in GetPermutations(items.Skip(i + 1), count - 1))
                        yield return new T[] { item }.Concat(result);
                }

                ++i;
            }
        }
    }
}
