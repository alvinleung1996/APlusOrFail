using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace APlusOrFail.Objects
{
    using Character;

    public class GoalArea : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D collision)
        {
            CharacterControl charControl;
            if (collision.gameObject.layer == LayerId.Characters && (charControl = collision.gameObject.GetComponentInParent<CharacterControl>()) != null)
            {
                charControl.Win(gameObject);
            }
        }
    }
}
