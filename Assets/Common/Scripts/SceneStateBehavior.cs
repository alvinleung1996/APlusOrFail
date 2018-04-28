using System;
using System.Threading.Tasks;
using UnityEngine;

namespace APlusOrFail
{
    public enum SceneStatePhase
    {
        Initialized,
        Loaded,
        Visible,
        Focused
    }

    public static class SceneStateExtensions
    {
        public static bool IsAtLeast(this SceneStatePhase state, SceneStatePhase min)
        {
            return state >= min;
        }
    }

    public interface ISceneState
    {
        Type argType { get; }
        Type resultType { get; }

        SceneStatePhase phase { get; }

        Task Load(SceneStateManager manager, object arg);

        Task MakeVisible(ISceneState unloadedSceneState, object result);

        Task Focus(ISceneState unloadedSceneState, object result);

        Task Blur();

        Task MakeInvisible();

        Task Unload();
    }

    public interface ISceneState<TArg, TResult> : ISceneState
    {
        Task Load(SceneStateManager manager, TArg arg);
    }

    public class SceneStateBehavior<TArg, TResult> : MonoBehaviour, ISceneState<TArg, TResult>
    {
        public Type argType => typeof(TArg);
        public Type resultType => typeof(TResult);
        
        public SceneStatePhase phase { get; private set; } = SceneStatePhase.Initialized;
        protected SceneStateManager manager { get; private set; }
        protected TArg arg { get; private set; }

        Task ISceneState.Load(SceneStateManager manager, object arg) => Load(manager, (TArg)arg);
        public Task Load(SceneStateManager manager, TArg arg)
        {
            phase = SceneStatePhase.Loaded;
            this.arg = arg;
            this.manager = manager;
            return OnLoad();
        }

        public Task MakeVisible(ISceneState unloadedSceneState, object result)
        {
            phase = SceneStatePhase.Visible;
            return OnMakeVisible(unloadedSceneState, result);
        }

        public Task Focus(ISceneState unloadedSceneState, object result)
        {
            phase = SceneStatePhase.Focused;
            return OnFocus(unloadedSceneState, result);
        }

        public async Task Blur()
        {
            await OnBlur();
            phase = SceneStatePhase.Visible;
        }

        public async Task MakeInvisible()
        {
            await OnMakeInvisible();
            phase = SceneStatePhase.Loaded;
        }
        
        public async Task Unload()
        {
            await OnUnload();
            arg = default(TArg);
            manager = null;
            phase = SceneStatePhase.Initialized;
        }


        protected virtual Task OnLoad() => Task.CompletedTask;

        protected virtual Task OnMakeVisible(ISceneState unloadedSceneState, object result) => Task.CompletedTask;

        protected virtual Task OnFocus(ISceneState unloadedSceneState, object result) => Task.CompletedTask;

        protected virtual Task OnBlur() => Task.CompletedTask;

        protected virtual Task OnMakeInvisible() => Task.CompletedTask;

        protected virtual Task OnUnload() => Task.CompletedTask;


        protected void PushSceneState<TA, TR>(ISceneState<TA, TR> sceneState, TA arg) => manager.Push(sceneState, arg);

        protected void ReplaceSceneState<TA, TR>(TResult result, ISceneState<TA, TR> sceneState, TA arg) => manager.Replace(this, result, sceneState, arg);

        protected void PopSceneState(TResult result) => manager.Pop(this, result);
    }
}
