using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace APlusOrFail.Objects
{
    public class Projector : MonoBehaviour
    {
        public SpriteRenderer lightConeRenderer;

        private Coroutine runningCoroutine;

        private void OnEnable()
        {
            runningCoroutine = StartCoroutine(ChangeScreenColor());
        }

        private void OnDestroy()
        {
            if (runningCoroutine != null)
            {
                StopCoroutine(runningCoroutine);
                runningCoroutine = null;
            }
        }

        private IEnumerator ChangeScreenColor()
        {
            while (true)
            {
                lightConeRenderer.color = Random.ColorHSV();
                yield return new WaitForSeconds(Random.Range(0, 3));
            }
        }
    }
}
