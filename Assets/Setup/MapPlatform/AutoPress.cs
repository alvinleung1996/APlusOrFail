using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace APlusOrFail.Setup
{
    using Character;

    public class AutoPress : MonoBehaviour
    {
        public SpringJoint2D springJoint;
        public string targetScenePath;

        private readonly Dictionary<CharacterPlayer, List<Collider2D>> enteredPlayers = new Dictionary<CharacterPlayer, List<Collider2D>>();
        private int targetSceneBuildIndex;
        
        private IMapSelectionHandler handler;


        private void Awake()
        {
            targetSceneBuildIndex = SceneUtility.GetBuildIndexByScenePath(targetScenePath);
            if (targetSceneBuildIndex < 0)
            {
                Debug.LogErrorFormat($"Cannot find scene at path: {targetScenePath}");
                Destroy(this);
            }
        }
        
        private void Start()
        {
            springJoint.autoConfigureDistance = false;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            CharacterPlayer charPlayer;
            if (collision.gameObject.layer == LayerId.Characters 
                && (charPlayer = collision.gameObject.GetComponentInParent<CharacterPlayer>()) != null)
            {
                List<Collider2D> colliders;
                if (!enteredPlayers.TryGetValue(charPlayer, out colliders))
                {
                    colliders = ListPool<Collider2D>.Get();
                    enteredPlayers.Add(charPlayer, colliders);
                }
                colliders.Add(collision.collider);
                UpdateHandler();
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            CharacterPlayer charPlayer;
            if (collision.gameObject.layer == LayerId.Characters
                && (charPlayer = collision.gameObject.GetComponentInParent<CharacterPlayer>()) != null)
            {
                List<Collider2D> colliders;
                if (enteredPlayers.TryGetValue(charPlayer, out colliders))
                {
                    colliders.Remove(collision.collider);
                    if (colliders.Count == 0)
                    {
                        enteredPlayers.Remove(charPlayer);
                        ListPool<Collider2D>.Release(colliders);
                        UpdateHandler();
                    }
                }
            }
        }

        private void UpdateHandler()
        {
            if (enteredPlayers.Count > 0)
            {
                if (handler == null)
                {
                    handler = MapSelectionRegistry.Schedule(targetSceneBuildIndex, enteredPlayers.Count);
                }
                else
                {
                    MapSelectionRegistry.UpdateSchedule(handler, targetSceneBuildIndex, enteredPlayers.Count);
                }
            }
            else
            {
                if (handler != null)
                {
                    MapSelectionRegistry.Unschedule(handler);
                    handler = null;
                }
            }
        }
    }
}
