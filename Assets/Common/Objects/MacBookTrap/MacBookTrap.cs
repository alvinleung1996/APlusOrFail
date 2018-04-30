using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace APlusOrFail.Objects
{
    using Character;
    using Maps;

    public class MacBookTrap : MonoBehaviour, IObjectPlayerSource
    {
        private static int animatorCloseHash = Animator.StringToHash("close");


        public BoxCollider2D screenTrigger;
        private Animator animator;
        public SpriteRenderer screenRenderer;
        public Color damgerousColor = Color.red;
        public float waitingTime = 2;

        private Coroutine runningCoroutine;
        private bool closingAnimationFinished = true;

        IReadOnlyPlayerSetting IObjectPlayerSource.player { get; set; }


        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        private void OnEnable()
        {
            runningCoroutine = StartCoroutine(RandomColorAndClose());
        }

        private void OnDisable()
        {
            if (runningCoroutine != null)
            {
                StopCoroutine(runningCoroutine);
                runningCoroutine = null;
            }
        }

        private Collider2D[] colliders = new Collider2D[4];
        private IEnumerator RandomColorAndClose()
        {
            while (true)
            {
                if (Random.value < 0.25f)
                {
                    screenRenderer.color = damgerousColor;

                    yield return new WaitForSeconds(waitingTime);

                    closingAnimationFinished = false;
                    animator.SetBool(animatorCloseHash, true);

                    yield return new WaitUntil(() => closingAnimationFinished);

                    int colliderCount;
                    while ((colliderCount = Physics2D.OverlapCollider(screenTrigger, new ContactFilter2D
                    {
                        useLayerMask = true,
                        layerMask = 1 << LayerId.Characters
                    }, colliders)) == colliders.Length)
                    {
                        colliders = new Collider2D[colliders.Length * 2];
                    }

                    IRoundPlayerStat roundPlayerStat = ((IObjectPlayerSource)this).player != null ?
                        MapManager.mapStat.GetRoundPlayerStat(MapManager.mapStat.currentRound, ((IObjectPlayerSource)this).player) :
                        null;
                    for (int i = 0; i < colliderCount; ++i)
                    {
                        CharacterControl charControl = colliders[i].gameObject.GetComponentInParent<CharacterControl>();
                        if (!charControl.ended)
                        {
                            int healthDelta = charControl.ChangeHealth(new ReadOnlyPlayerHealthChange(PlayerHealthChangeReason.ByTrap, -charControl.health, gameObject));
                            if (healthDelta > 0 && roundPlayerStat != null)
                            {
                                roundPlayerStat.scoreChanges.Add(MapManager.mapStat.roundSettings[MapManager.mapStat.currentRound]
                                    .CreatePointsChange(PlayerPointsChangeReason.KillOtherByTrap, gameObject));
                            }
                        }
                    }

                    yield return new WaitForSeconds(Random.Range(1, 5));
                }

                screenRenderer.color = Random.ColorHSV();
                animator.SetBool(animatorCloseHash, false);

                yield return new WaitForSeconds(Random.Range(0, 3));
            }
        }

        private void OnClosingAnimationFinished()
        {
            closingAnimationFinished = true;
        }
    }
}
