using System;
using System.Collections.Generic;
using UnityEngine;

namespace APlusOrFail
{
    [Obsolete]
    public static class PlayerInputRegistry
    {
        private static readonly Dictionary<KeyCode, IReadOnlySharedPlayerSetting> registry = new Dictionary<KeyCode, IReadOnlySharedPlayerSetting>();

        public static bool HasRegistered(KeyCode key)
        {
            return registry.ContainsKey(key);
        }

        public static bool HasRegisteredByOther(KeyCode key, IReadOnlySharedPlayerSetting exceptPlayer)
        {
            IReadOnlySharedPlayerSetting associatedPlayer = GetAssociatedPlayer(key);
            return associatedPlayer != null && associatedPlayer != exceptPlayer;
        }

        public static IReadOnlySharedPlayerSetting GetAssociatedPlayer(KeyCode key)
        {
            IReadOnlySharedPlayerSetting player;
            return registry.TryGetValue(key, out player) ? player : null;
        }

        public static void RegisterKey(KeyCode key, IReadOnlySharedPlayerSetting player)
        {
            IReadOnlySharedPlayerSetting associatedPlayer = GetAssociatedPlayer(key);
            if (associatedPlayer == null)
            {
                registry[key] = player;
            }
            else if (associatedPlayer != player)
            {
                throw new ArgumentException($"Key \"{Enum.GetName(typeof(KeyCode), key)}\" has already been associated with Player {associatedPlayer.id}");
            }
        }

        public static void UnregisterKey(KeyCode key, IReadOnlySharedPlayerSetting player)
        {
            if (GetAssociatedPlayer(key) == player)
            {
                registry.Remove(key);
            }
        }
    }
}
