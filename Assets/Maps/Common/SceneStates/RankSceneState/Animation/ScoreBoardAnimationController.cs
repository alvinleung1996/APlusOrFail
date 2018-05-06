using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace APlusOrFail.Maps.SceneStates
{
    public class ScoreBoardAnimationController : MonoBehaviour
    {
        private static int openHash = Animator.StringToHash("open");

        private enum State
        {
            Closed,
            Opening,
            Opened,
            Closing
        }


        private Animator animator;

        private State state = State.Closed;
        private TaskCompletionSource<Void> openTaskSource;
        private TaskCompletionSource<Void> closeTaskSource;
        

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        public async Task Open()
        {
            if (state == State.Opened)
            {
                return;
            }
            else
            {
                if (openTaskSource == null)
                {
                    openTaskSource = new TaskCompletionSource<Void>();
                    if (closeTaskSource != null) await closeTaskSource.Task;
                    state = State.Opening;
                    animator.SetBool(openHash, true);
                }
                await openTaskSource.Task;
            }
        }

        public async Task Close()
        {
            if (state == State.Closed)
            {
                return;
            }
            else
            {
                if (closeTaskSource == null)
                {
                    closeTaskSource = new TaskCompletionSource<Void>();
                    if (openTaskSource != null) await openTaskSource.Task;
                    state = State.Closing;
                    animator.SetBool(openHash, false);
                   
                }
                await closeTaskSource.Task;
            }
        }

        private void OnOpeningAnimationFinished()
        {
            openTaskSource.SetResult(null);
            openTaskSource = null;
            state = State.Opened;
        }

        private void OnClosingAnimationFinished()
        {
            closeTaskSource.SetResult(null);
            closeTaskSource = null;
            state = State.Closed;
        }
    }
}
