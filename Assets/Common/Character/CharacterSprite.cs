using UnityEngine;

namespace APlusOrFail.Character
{
    [RequireComponent(typeof(CharacterPlayer))]
    public class CharacterSprite : MonoBehaviour
    {
        public GameObject attachedSprite { get; private set; }
        
        private void Start()
        {
            int id = GetComponent<CharacterSpriteId>().spriteId;
            GameObject prefab;
            if (SharedData.CharacterSpriteIdMap.TryGetValue(id, out prefab))
            {
                attachedSprite = Instantiate(prefab, transform);
            }
        }
    }
}
