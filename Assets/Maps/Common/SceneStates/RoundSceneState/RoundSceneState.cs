﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace APlusOrFail.Maps.SceneStates.RoundSceneState
{
    using Character;

    public class RoundSceneState : SceneStateBehavior<Void, ReadOnlyCollection<RoundSceneState.PlayerStatistics>>
    {
        private class CharacterInfo
        {
            private readonly RoundSceneState enclosing;
            private GameObject character;

            public CharacterInfo(RoundSceneState enclosing, Player player)
            {
                this.enclosing = enclosing;
                enclosing.characterInfos.Add(this);

                character = Instantiate(enclosing.characterPrefab, enclosing.characterPrefab.transform.position, Quaternion.identity);
                character.GetComponent<CharacterPlayer>().player = player;
                character.GetComponent<CharacterHealth>().onHealthChanged += OnCharacterHealthChanged;
            }

            public void Update()
            {

            }

            private void OnCharacterHealthChanged(CharacterHealth charHealth, CharacterHealth.ChangeDatum health)
            {
                if (enclosing.phase.IsAtLeast(SceneStatePhase.Activated))
                {
                    if (charHealth.health <= 0)
                    {
                        Remove();
                    }
                }
            }

            public void Remove()
            {
                CharacterPlayer charPlayer = character.GetComponent<CharacterPlayer>();
                CharacterHealth charHealth = character.GetComponent<CharacterHealth>();

                enclosing.result.Add(new PlayerStatistics(charPlayer.player, charHealth.changeData));

                charPlayer.player = null;
                charHealth.onHealthChanged -= OnCharacterHealthChanged;

                Destroy(character);

                enclosing.characterInfos.Remove(this);
                if (enclosing.characterInfos.Count == 0)
                {
                    enclosing.OnAllCharacterDied();
                }
            }
        }


        public class PlayerStatistics
        {
            public readonly Player player;
            public readonly ReadOnlyCollection<CharacterHealth.ChangeDatum> healthChangeData;

            public PlayerStatistics(Player player, IEnumerable<CharacterHealth.ChangeDatum> healthChangeData)
            {
                this.player = player;
                this.healthChangeData = new ReadOnlyCollection<CharacterHealth.ChangeDatum>(healthChangeData.ToList());
            }
        }


        public GameObject characterPrefab;
        public Transform spawnPoint;

        
        private readonly List<CharacterInfo> characterInfos = new List<CharacterInfo>();
        private readonly List<PlayerStatistics> result = new List<PlayerStatistics>();

        protected override void OnLoad(Void arg)
        {
            base.OnLoad(arg);
        }

        protected override void OnActivate(ISceneState unloadedSceneState, object result)
        {
            base.OnActivate(unloadedSceneState, result);
            if (unloadedSceneState == null)
            {
                foreach (Player player in Player.players)
                {
                    new CharacterInfo(this, player);
                }
            }
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate();
            foreach (CharacterInfo info in characterInfos)
            {
                info.Remove();
            }
        }

        protected override ReadOnlyCollection<PlayerStatistics> OnUnload()
        {
            base.OnUnload();
            var result = new ReadOnlyCollection<PlayerStatistics>(this.result.ToList());
            this.result.Clear();
            return result;
        }

        private void OnAllCharacterDied()
        {
            SceneStateManager.instance.Pop(this);
        }
    }
}
