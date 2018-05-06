using System.Collections;
using UnityEngine;

namespace APlusOrFail.Decroations
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator))]
    public class BlinkingEye : MonoBehaviour
    {
        private static int animatorOpenHash = Animator.StringToHash("open");


        private Animator animator;
        private Coroutine runningCoroutine;


        private void Awake()
        {
            animator = GetComponent<Animator>();    
        }

        private void OnEnable()
        {
            runningCoroutine = StartCoroutine(ToggleEye());
        }

        private void OnDisable()
        {
            if (runningCoroutine != null)
            {
                StopCoroutine(runningCoroutine);
                runningCoroutine = null;
            }    
        }

        private IEnumerator ToggleEye()
        {
            while (true)
            {
                animator.SetBool(animatorOpenHash, Mathf.RoundToInt(Random.value) > 0);
                yield return new WaitForSeconds(Random.Range(0, 3));
            }
        }
    }
}
