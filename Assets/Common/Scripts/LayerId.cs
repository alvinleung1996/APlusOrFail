using UnityEngine;

namespace APlusOrFail
{
    public static class LayerId
    {
        public static readonly int Characters = GetLayerId("Characters");
        public static readonly int Objects = GetLayerId("Objects");
        public static readonly int Decroations = GetLayerId("Decroations");
        public static readonly int SelectableObjects = GetLayerId("Selectable Objects");

        private static int GetLayerId(string layerName)
        {
            int id = LayerMask.NameToLayer(layerName);
            if (id < 0) Debug.LogErrorFormat($"Cannot find layer \"{layerName}\"");
            return id;
        }
    }
}
