using System.Collections.Generic;

namespace APlusOrFail
{
    public static class ListPool<T>
    {
        private static readonly List<List<T>> pool = new List<List<T>>();

        public static List<T> Get()
        {
            //if (System.Threading.Thread.CurrentThread.ManagedThreadId != 1)
            //{
            //    UnityEngine.Debug.LogErrorFormat($"on another thread! {System.Threading.Thread.CurrentThread.ManagedThreadId}");
            //}
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
                list.Clear();
                pool.Add(list);
            }
        }
    }
}
