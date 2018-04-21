﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace APlusOrFail
{
    public class SceneStateManager : MonoBehaviour
    {
        private enum Action
        {
            None,
            Push,
            Replace,
            Pop
        }

        public static SceneStateManager instance { get; private set; }

        public UnityEngine.Object initialSceneState;

        private bool asyncUpdating;
        private readonly Stack<ISceneState> sceneStateStack = new Stack<ISceneState>();
        private Action pendingAction = Action.None;
        private ISceneState pendingSceneState;
        private object pendingArg;
        private object pendingResult;

        private void Awake()
        {
            if (ReferenceEquals(instance, null))
            {
                instance = this;
            }
            else
            {
                Destroy(this);
                Debug.LogErrorFormat("Found another scene state manager!");
            }
        }

        private void OnDestroy()
        {
            if (ReferenceEquals(instance, this))
            {
                instance = null;
            }
        }

        private void Start()
        {
            if (initialSceneState != null)
            {
                if (initialSceneState.GetType().GetInterfaces()
                    .Where(i => i.IsGenericType)
                    .Any(i => i.GetGenericTypeDefinition() == typeof(ISceneState<,>)))
                {
                    ISceneState state = (ISceneState)initialSceneState;
                    Type argType = state.argType;
                    Type resultType = state.resultType;
                    new Action<ISceneState<object, object>, object>(Push).Method
                        .GetGenericMethodDefinition()
                        .MakeGenericMethod(argType, resultType)
                        .Invoke(this, new object[] { state, argType.IsValueType ? Activator.CreateInstance(argType) : null });
                }
                else
                {
                    Debug.LogErrorFormat($"initial scene state has type {initialSceneState.GetType()} which does not implement {typeof(ISceneState<,>)}");
                }
            }
        }

        private void Update()
        {
            if (pendingAction != Action.None && !asyncUpdating)
            {
                UpdateAsync().ContinueWith(t => Debug.LogException(t.Exception), TaskContinuationOptions.OnlyOnFaulted);
            }
        }

        private async Task UpdateAsync()
        {
            asyncUpdating = true;

            Action pendingAction = this.pendingAction;
            ISceneState pendingSceneState = this.pendingSceneState;
            object pendingArg = this.pendingArg;
            object pendingResult = this.pendingResult;

            this.pendingAction = Action.None;
            this.pendingSceneState = null;
            this.pendingArg = null;
            this.pendingResult = null;

            ISceneState oldSceneState, newSceneState;

            switch (pendingAction)
            {
                case Action.Push:
                    oldSceneState = sceneStateStack.Count > 0 ? sceneStateStack.Peek() : null;
                    newSceneState = pendingSceneState;
                    sceneStateStack.Push(newSceneState);

                    await WhenTwo(
                        newSceneState.Load(pendingArg, null, null),
                        NonNullTask(oldSceneState?.Blur())
                    );
                    await WhenTwo(
                        newSceneState.MakeVisible(null, null),
                        NonNullTask(oldSceneState?.MakeInvisible())
                    );
                    await newSceneState.Focus(null, null);
                    break;

                case Action.Replace:
                    oldSceneState = sceneStateStack.Pop();
                    newSceneState = pendingSceneState;
                    sceneStateStack.Push(newSceneState);

                    await WhenTwo(
                        newSceneState.Load(pendingArg, null, null),
                        oldSceneState.Blur()
                    );
                    await WhenTwo(
                        newSceneState.MakeVisible(null, null),
                        oldSceneState.MakeInvisible()
                    );
                    await WhenTwo(
                        newSceneState.Focus(null, null),
                        oldSceneState.Unload()
                    );
                    break;

                case Action.Pop:
                    oldSceneState = sceneStateStack.Pop();
                    newSceneState = sceneStateStack.Count > 0 ? sceneStateStack.Peek() : null;
                    
                    await oldSceneState.Blur();
                    await WhenTwo(
                        NonNullTask(newSceneState?.MakeVisible(oldSceneState, pendingResult)),
                        oldSceneState.MakeInvisible()
                    );
                    await WhenTwo(
                        NonNullTask(newSceneState?.Focus(oldSceneState, pendingResult)),
                        oldSceneState.Unload()
                    );
                    break;
            }

            asyncUpdating = false;
        }

        public void Push<TA, TR>(ISceneState<TA, TR> sceneState, TA arg)
        {
            pendingAction = Action.Push;
            pendingSceneState = sceneState;
            pendingArg = arg;
            pendingResult = null;
        }

        public void Replace<TA1, TR1, TA2, TR2>(ISceneState<TA1, TR1> sceneState, TR1 result, ISceneState<TA2, TR2> newSceneState, TA2 arg)
        {
            if (sceneStateStack.Count == 0 || sceneStateStack.Peek() != newSceneState)
            {
                throw new InvalidOperationException("The scene to pop is not at top!");
            }
            pendingAction = Action.Replace;
            pendingSceneState = newSceneState;
            pendingArg = arg;
            pendingResult = result;
        }

        public void Pop<TA, TR>(ISceneState<TA, TR> sceneState, TR result)
        {
            if (sceneStateStack.Count == 0 || sceneStateStack.Peek() != sceneState)
            {
                throw new InvalidOperationException("The scene to pop is not at top!");
            }
            pendingAction = Action.Pop;
            pendingSceneState = null;
            pendingArg = null;
            pendingResult = result;
        }

        public void Undo()
        {
            pendingAction = Action.None;
            pendingSceneState = null;
            pendingArg = null;
            pendingResult = null;
        }

        private Task NonNullTask(Task task) => task ?? Task.CompletedTask;

        private readonly Task[] _tasks = new Task[2];
        private Task WhenTwo(Task task1, Task task2)
        {
            _tasks[0] = task1;
            _tasks[1] = task2;
            return Task.WhenAll(_tasks);
        }
    }
}
