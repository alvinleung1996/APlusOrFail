using UnityEngine;

namespace APlusOrFail
{
    public static class SortingLayerId
    {
        public static readonly int UI = GetSortingLayerId("UI");

        private static int GetSortingLayerId(string name)
        {
            int id = SortingLayer.NameToID(name);
            if (id == 0)
            {
                Debug.LogErrorFormat($"Cannot find sorting layer \"{name}\"!");
            }
            return id;
        }
    }
}
