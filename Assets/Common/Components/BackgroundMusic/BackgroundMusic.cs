using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace APlusOrFail.Audio
{
    public class BackgroundMusic : MonoBehaviour
    {

        public static BackgroundMusic instance { get; private set; }

        private void Awake()
        {
            if (ReferenceEquals(instance, null))
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (ReferenceEquals(instance, this))
            {
                instance = null;
            }
        }
    }
}
