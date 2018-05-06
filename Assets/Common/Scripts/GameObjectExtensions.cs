using UnityEngine;

namespace APlusOrFail
{
    public static class GameObjectExtensions
    {
        public static void SetLayerRecursively(this GameObject gameObject, int layer)
        {
            gameObject.layer = layer;
            foreach (Transform childTransform in gameObject.transform)
            {
                SetLayerRecursively(childTransform.gameObject, layer);
            }
        }
    }
}
