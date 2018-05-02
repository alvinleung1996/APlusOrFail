using UnityEngine;
using System.Collections.Generic;

namespace APlusOrFail
{
    public static class SharedData
    {
        public static IReadOnlyDictionary<int, GameObject> CharacterSpriteIdMap { get; set; }
        public static IReadOnlyList<IReadOnlySharedPlayerSetting> playerSettings { get; set; }
    }
}
