using UnityEngine;

namespace APlusOrFail.Character
{
    [RequireComponent(typeof(CharacterPlayer))]
    public class CharacterSprite : MonoBehaviour
    {
        public GameObject overrideCharacterSprite;

        public GameObject attachedSprite { get; private set; }
        
        private void Start()
        {
            CharacterPlayer charPlayer = GetComponent<CharacterPlayer>();
            GameObject prefab = overrideCharacterSprite ?? charPlayer.player.characterSprite;
            if (prefab != null)
            {
                attachedSprite = Instantiate(prefab, transform);
            }
        }
    }
}
