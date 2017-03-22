using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PermutationCombination
{
    public static class PermutationCombinationExtension
    {
        public static IEnumerable<IEnumerable<T>> Combination<T>(this IEnumerable<T> a, int choose)
        {
            int count = a.Count();
            if (choose >= count)
            {
                yield return a;
            }
            else if (choose <= 1)
            {
                foreach (var n in (from m in a select m))
                {
                    yield return Enumerable.Repeat(n, 1);
                }
            }
            else
            {
                for (int i = 0; i + choose <= count; ++i)
                {
                    foreach (var m in a.Skip(i + 1).Combination(choose - 1))
                    {
                        yield return a.Skip(i).Take(1).Union(m);
                    }
                }
            }
        }

        public static IEnumerable<T> Combination<T>(this IEnumerable<T> a, int choose, Func<T, T, T> aggregate)
        {
            return (from e in a.Combination(choose)
                    select e.Aggregate(aggregate));
        }

        private static IEnumerable<IEnumerable<T>> FullPermutation<T>(this IEnumerable<T> a)
        {
            int count = a.Count();
            if (count <= 1)
            {
                yield return a;
            }
            else
            {
                for (int i = 0; i < count; ++i)
                {
                    var m = a.Skip(i).Take(1);
                    foreach (var n in a.Except(m).FullPermutation())
                    {
                        yield return m.Union(n);
                    }
                }
            }
        }

        public static IEnumerable<IEnumerable<T>> Permutation<T>(this IEnumerable<T> a, int choose)
        {
            if (choose >= a.Count()) return a.FullPermutation();
            return (from e in a.Combination(choose) select e.FullPermutation()).Aggregate((e1, e2) => e1.Union(e2));
        }

        public static IEnumerable<T> Permutation<T>(this IEnumerable<T> a, int choose, Func<T, T, T> aggregate)
        {
            return (from e in a.Permutation(choose)
                    select e.Aggregate(aggregate));
        }
    }

}