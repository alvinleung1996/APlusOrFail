using System;
using System.Collections.Generic;
using UnityEngine;

namespace APlusOrFail
{
    public static class PlayerInputRegistry
    {
        private static readonly Dictionary<KeyCode, IReadOnlyPlayerSetting> registry = new Dictionary<KeyCode, IReadOnlyPlayerSetting>();

        public static bool HasRegistered(KeyCode key)
        {
            return registry.ContainsKey(key);
        }

        public static bool HasRegisteredByOther(KeyCode key, IReadOnlyPlayerSetting exceptPlayer)
        {
            IReadOnlyPlayerSetting associatedPlayer = GetAssociatedPlayer(key);
            return associatedPlayer != null && associatedPlayer != exceptPlayer;
        }

        public static IReadOnlyPlayerSetting GetAssociatedPlayer(KeyCode key)
        {
            IReadOnlyPlayerSetting player;
            return registry.TryGetValue(key, out player) ? player : null;
        }

        public static void RegisterKey(KeyCode key, IReadOnlyPlayerSetting player)
        {
            IReadOnlyPlayerSetting associatedPlayer = GetAssociatedPlayer(key);
            if (associatedPlayer == null)
            {
                registry[key] = player;
            }
            else if (associatedPlayer != player)
            {
                throw new ArgumentException($"Key \"{Enum.GetName(typeof(KeyCode), key)}\" has already been associated with Player {associatedPlayer.id}");
            }
        }

        public static void UnregisterKey(KeyCode key, IReadOnlyPlayerSetting player)
        {
            if (GetAssociatedPlayer(key) == player)
            {
                registry.Remove(key);
            }
        }
    }
}
