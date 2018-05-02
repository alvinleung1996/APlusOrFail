using System;
using UnityEngine;

namespace APlusOrFail
{
    public abstract class SingletonBehavior<T> : MonoBehaviour where T : SingletonBehavior<T>
    {
        public static T instance { get; private set; }

        protected virtual void Awake()
        {
            if (ReferenceEquals(instance, null))
            {
                instance = (T)this;
            }
            else
            {
                Destroy(this);
                throw new InvalidOperationException($"Found another {typeof(T).Name}");
            }
        }

        protected virtual void OnDestroy()
        {
            if (ReferenceEquals(instance, this))
            {
                instance = null;
            }
        }
    }
}
