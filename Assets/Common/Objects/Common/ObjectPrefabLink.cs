using UnityEngine;

namespace APlusOrFail.Objects
{
    public class ObjectPrefabLink : MonoBehaviour
    {
        public ObjectPrefabInfo prefab;

        private void Start()
        {
            Debug.LogErrorFormat("Prefab link is not designed for initantiate!");
        }
    }
}
