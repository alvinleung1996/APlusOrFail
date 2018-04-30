using System;
using UnityEngine;

namespace APlusOrFail.Character
{
    using NameTagCanvas;

    [RequireComponent(typeof(CharacterPlayer))]
    [Obsolete]
    public class CharacterNameTag : MonoBehaviour
    {
        
        private NameTagCanvas0.NameTagInfo nameTagInfo;
        
        private void Start()
        {
            nameTagInfo = NameTagCanvas0.instance.AddNameTag(transform);
            nameTagInfo.worldOffset = new Vector2(0, 1);

            CharacterPlayer charPlayer = GetComponentInParent<CharacterPlayer>();
            //SetPlayer(charPlayer.player);
            //charPlayer.onPlayerChanged += OnPlayerChanged;
        }

        private void OnDestroy()
        {
            NameTagCanvas0.instance?.RemoveNameTag(transform);
        }

        private void OnPlayerChanged(CharacterPlayer charPlayer, PlayerSetting newPlayer)
        {
            SetPlayer(newPlayer);
        }

        private PlayerSetting player;
        private void SetPlayer(PlayerSetting player)
        {
            //if (this.player != null)
            //{
            //    this.player.onNameChanged -= OnPlayerNameChanged;
            //    this.player.onColorChanged -= OnPlayerColorChanged;
            //}
            //this.player = player;

            //SetName(player?.name);
            //SetColor(player?.color);
            //if (player != null)
            //{
            //    player.onNameChanged += OnPlayerNameChanged;
            //    player.onColorChanged += OnPlayerColorChanged;
            //}
        }

        private void OnPlayerNameChanged(PlayerSetting player, string name)
        {
            SetName(name);
        }

        private void SetName(string name)
        {
            nameTagInfo.name = name;
        }

        private void OnPlayerColorChanged(PlayerSetting player, Color color)
        {
            SetColor(color);
        }

        private void SetColor(Color? color)
        {
            nameTagInfo.color = color ?? Color.white;
        }
    }
}
