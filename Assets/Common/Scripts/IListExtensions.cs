using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APlusOrFail
{
    public static class IListExtensions
    {
        public static void AddRange<T>(this IList<T> list, IEnumerable<T> collection)
        {
            List<T> concreteList = list as List<T>;
            if (concreteList != null)
            {
                concreteList.AddRange(collection);
            }
            else
            {
                foreach (T value in collection)
                {
                    list.Add(value);
                }
            }
        }

        public static int FindIndex<T>(this IReadOnlyList<T> list, Predicate<T> match)
        {
            List<T> concreteList = list as List<T>;
            if (concreteList != null)
            {
                return concreteList.FindIndex(match);
            }
            else
            {
                for (int i = 0; i < list.Count; ++i)
                {
                    if (match(list[i]))
                    {
                        return i;
                    }
                }
                return -1;
            }
        }
    }
}
