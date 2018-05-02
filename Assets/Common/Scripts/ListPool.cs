using System.Collections.Generic;

namespace APlusOrFail
{
    public static class ListPool<T>
    {
        private static readonly List<List<T>> pool = new List<List<T>>();

        public static List<T> Get()
        {
            if (pool.Count > 0)
            {
                List<T> list = pool[pool.Count - 1];
                pool.RemoveAt(pool.Count - 1);
                return list;
            }
            else
            {
                return new List<T>();
            }
        }

        public static void Release(List<T> list)
        {
            if (pool.Count < 10)
            {
                pool.Add(list);
                list.Clear();
            }
        }
    }
}
