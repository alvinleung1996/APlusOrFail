using System;
using System.Linq;
using System.Collections.Generic;
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

    public enum SceneStateObserverPhase
    {
        Initialized,

        PreLoad,
        Load,
        PostLoad,

        PreMakeVisible,
        MakeVisible,
        PostMakeVisible,

        PreFocus,
        Focus,
        PostFocus
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

        Task OnPreLoad(SceneStateManager manager, object arg);
        Task OnLoad(SceneStateManager manager, object arg);
        Task OnPostLoad(SceneStateManager manager, object arg);

        Task OnPreMakeVisible(ISceneState unloadedSceneState, object result);
        Task OnMakeVisible(ISceneState unloadedSceneState, object result);
        Task OnPostMakeVisible(ISceneState unloadedSceneState, object result);

        Task OnPreFocus(ISceneState unloadedSceneState, object result);
        Task OnFocus(ISceneState unloadedSceneState, object result);
        Task OnPostFocus(ISceneState unloadedSceneState, object result);

        Task OnPreBlur();
        Task OnBlur();
        Task OnPostBlur();

        Task OnPreMakeInvisible();
        Task OnMakeInvisible();
        Task OnPostMakeInvisible();

        Task OnPreUnload();
        Task OnUnload();
        Task OnPostUnload();
    }

    public interface ISceneState<TArg, TResult> : ISceneState
    {
        Task OnPreLoad(SceneStateManager manager, TArg arg);
        Task OnLoad(SceneStateManager manager, TArg arg);
        Task OnPostLoad(SceneStateManager manager, TArg arg);
    }

    public interface ILifecycleObservableSceneState<T> where T : class, ILifecycleObservableSceneState<T>
    {
        SceneStatePhase phase { get; }
        void ObserveSceneState(ISceneStateLifecycleObserver<T> observer);
        void UnobserveSeneState(ISceneStateLifecycleObserver<T> observer);
    }

    public interface ISceneStateLifecycleObserver<in T> where T : class, ILifecycleObservableSceneState<T>
    {
        Task OnSceneStatePreLoad(T sceneState);
        Task OnSceneStateLoad(T sceneState);
        Task OnSceneStatePostLoad(T sceneState);

        Task OnSceneStatePreMakeVisible(T sceneState);
        Task OnSceneStateMakeVisible(T sceneState);
        Task OnSceneStatePostMakeVisible(T sceneState);

        Task OnSceneStatePreFocus(T sceneState);
        Task OnSceneStateFocus(T sceneState);
        Task OnSceneStatePostFocus(T sceneState);

        Task OnSceneStatePreBlur(T sceneState);
        Task OnSceneStateBlur(T sceneState);
        Task OnSceneStatePostBlur(T sceneState);

        Task OnSceneStatePreMakeInvisible(T sceneState);
        Task OnSceneStateMakeInvisible(T sceneState);
        Task OnSceneStatePostMakeInvisible(T sceneState);

        Task OnSceneStatePreUnload(T sceneState);
        Task OnSceneStateUnload(T sceneState);
        Task OnSceneStatePostUnload(T sceneState);
    }


    public abstract class SceneStateBehavior<TArg, TResult> : MonoBehaviour, ISceneState<TArg, TResult>
    {
        public Type argType => typeof(TArg);
        public Type resultType => typeof(TResult);
        
        public SceneStatePhase phase { get; private set; } = SceneStatePhase.Initialized;
        protected SceneStateManager manager { get; private set; }
        protected TArg arg { get; private set; }


        private int lastState = 0;
        private Task EnsureLifecycle(int state)
        {
            if (state != (lastState - 1) && state != (lastState + 1))
            {
                Debug.LogErrorFormat($"Unexpected state {state}, last state is {lastState}");
            }
            lastState = state;
            return Task.CompletedTask;
        }

        Task ISceneState.OnPreLoad(SceneStateManager manager, object arg) => OnPreLoad(manager, (TArg)arg);
        public virtual Task OnPreLoad(SceneStateManager manager, TArg arg)
        {
            EnsureLifecycle(1);
            phase = SceneStatePhase.Loaded;
            this.arg = arg;
            this.manager = manager;
            return Task.CompletedTask;
        }
        
        Task ISceneState.OnLoad(SceneStateManager manager, object arg) => OnLoad(manager, (TArg)arg);
        public virtual Task OnLoad(SceneStateManager manager, TArg arg) => EnsureLifecycle(2);

        Task ISceneState.OnPostLoad(SceneStateManager manager, object arg) => OnPostLoad(manager, (TArg)arg);
        public virtual Task OnPostLoad(SceneStateManager manager, TArg arg) => EnsureLifecycle(3);


        public virtual Task OnPreMakeVisible(ISceneState unloadedSceneState, object result)
        {
            EnsureLifecycle(4);
            phase = SceneStatePhase.Visible;
            return Task.CompletedTask;
        }

        public virtual Task OnMakeVisible(ISceneState unloadedSceneState, object result) => EnsureLifecycle(5);

        public virtual Task OnPostMakeVisible(ISceneState unloadedSceneState, object result) => EnsureLifecycle(6);


        public virtual Task OnPreFocus(ISceneState unloadedSceneState, object result)
        {
            EnsureLifecycle(7);
            phase = SceneStatePhase.Focused;
            return Task.CompletedTask;
        }

        public virtual Task OnFocus(ISceneState unloadedSceneState, object result) => EnsureLifecycle(8);

        public virtual Task OnPostFocus(ISceneState unloadedSceneState, object result) => EnsureLifecycle(9);


        public virtual Task OnPreBlur() => EnsureLifecycle(8);

        public virtual Task OnBlur() => EnsureLifecycle(7);

        public virtual Task OnPostBlur()
        {
            EnsureLifecycle(6);
            phase = SceneStatePhase.Visible;
            return Task.CompletedTask;
        }

        
        public virtual Task OnPreMakeInvisible() => EnsureLifecycle(5);

        public virtual Task OnMakeInvisible() => EnsureLifecycle(4);

        public virtual Task OnPostMakeInvisible()
        {
            EnsureLifecycle(3);
            phase = SceneStatePhase.Loaded;
            return Task.CompletedTask;
        }


        public virtual Task OnPreUnload() => EnsureLifecycle(2);

        public virtual Task OnUnload() => EnsureLifecycle(1);

        public virtual Task OnPostUnload()
        {
            EnsureLifecycle(0);
            arg = default(TArg);
            manager = null;
            phase = SceneStatePhase.Initialized;
            return Task.CompletedTask;
        }


        protected void PushSceneState<TA, TR>(ISceneState<TA, TR> sceneState, TA arg) => manager.Push(sceneState, arg);

        protected void ReplaceSceneState<TA, TR>(TResult result, ISceneState<TA, TR> sceneState, TA arg) => manager.Replace(this, result, sceneState, arg);

        protected void PopSceneState(TResult result) => manager.Pop(this, result);
    }

    public abstract class ObservableSceneStateBehavior<TArg, TResult, TObserve> :
        SceneStateBehavior<TArg, TResult>, ILifecycleObservableSceneState<TObserve> 
        where TObserve : class, ILifecycleObservableSceneState<TObserve>
    {
        private readonly Dictionary<ISceneStateLifecycleObserver<TObserve>, SceneStateObserverPhase> observers
            = new Dictionary<ISceneStateLifecycleObserver<TObserve>, SceneStateObserverPhase>();

        protected abstract TObserve observable { get; }
        private SceneStateObserverPhase targetObserverPhase;
        private SceneStatePhase targetSceneStatePhase;

        protected virtual void Awake()
        {
            if (!ObservableSceneState<TObserve>.Register(observable))
            {
                Destroy(this);
                Debug.LogError($"Found another {typeof(TObserve)}");
            }
        }

        protected virtual void OnDestroy()
        {
            ObservableSceneState<TObserve>.Unregister(observable);
        }

        public override async Task OnPreLoad(SceneStateManager manager, TArg arg)
        {
            await base.OnPreLoad(manager, arg);
            await DispatchEvent(SceneStateObserverPhase.PreLoad, o => o.OnSceneStatePreLoad(observable));
        }

        public override async Task OnLoad(SceneStateManager manager, TArg arg)
        {
            await base.OnLoad(manager, arg);
            await DispatchEvent(SceneStateObserverPhase.Load, o => o.OnSceneStateLoad(observable));
        }

        public override async Task OnPostLoad(SceneStateManager manager, TArg arg)
        {
            await base.OnPostLoad(manager, arg);
            await DispatchEvent(SceneStateObserverPhase.PostLoad, o => o.OnSceneStatePostLoad(observable));
        }
        

        public override async Task OnPreMakeVisible(ISceneState unloadedSceneState, object result)
        {
            await base.OnPreMakeVisible(unloadedSceneState, result);
            await DispatchEvent(SceneStateObserverPhase.PreMakeVisible, o => o.OnSceneStatePreMakeVisible(observable));
        }

        public override async Task OnMakeVisible(ISceneState unloadedSceneState, object result)
        {
            await base.OnMakeVisible(unloadedSceneState, result);
            await DispatchEvent(SceneStateObserverPhase.MakeVisible, o => o.OnSceneStateMakeVisible(observable));
        }

        public override async Task OnPostMakeVisible(ISceneState unloadedSceneState, object result)
        {
            await base.OnPostMakeVisible(unloadedSceneState, result);
            await DispatchEvent(SceneStateObserverPhase.PostMakeVisible, o => o.OnSceneStatePostMakeVisible(observable));
        }


        public override async Task OnPreFocus(ISceneState unloadedSceneState, object result)
        {
            await base.OnPreFocus(unloadedSceneState, result);
            await DispatchEvent(SceneStateObserverPhase.PreFocus, o => o.OnSceneStatePreFocus(observable));
        }

        public override async Task OnFocus(ISceneState unloadedSceneState, object result)
        {
            await base.OnFocus(unloadedSceneState, result);
            await DispatchEvent(SceneStateObserverPhase.Focus, o => o.OnSceneStateFocus(observable));
        }

        public override async Task OnPostFocus(ISceneState unloadedSceneState, object result)
        {
            await base.OnPostFocus(unloadedSceneState, result);
            await DispatchEvent(SceneStateObserverPhase.PostFocus, o => o.OnSceneStatePostFocus(observable));
        }


        public override async Task OnPreBlur()
        {
            await base.OnPreBlur();
            await DispatchEvent(SceneStateObserverPhase.Focus, o => o.OnSceneStatePreBlur(observable));
        }

        public override async Task OnBlur()
        {
            await base.OnBlur();
            await DispatchEvent(SceneStateObserverPhase.PreFocus, o => o.OnSceneStateBlur(observable));
        }

        public override async Task OnPostBlur()
        {
            await base.OnPostBlur();
            await DispatchEvent(SceneStateObserverPhase.PostMakeVisible, o => o.OnSceneStatePostBlur(observable));
        }


        public override async Task OnPreMakeInvisible()
        {
            await base.OnPreMakeInvisible();
            await DispatchEvent(SceneStateObserverPhase.MakeVisible, o => o.OnSceneStatePreMakeInvisible(observable));
        }

        public override async Task OnMakeInvisible()
        {
            await base.OnMakeInvisible();
            await DispatchEvent(SceneStateObserverPhase.PreMakeVisible, o => o.OnSceneStateMakeInvisible(observable));
        }

        public override async Task OnPostMakeInvisible()
        {
            await base.OnPostMakeInvisible();
            await DispatchEvent(SceneStateObserverPhase.PostLoad, o => o.OnSceneStatePostMakeInvisible(observable));
        }


        public override async Task OnPreUnload()
        {
            await base.OnPreUnload();
            await DispatchEvent(SceneStateObserverPhase.Load, o => o.OnSceneStatePreUnload(observable));
        }

        public override async Task OnUnload()
        {
            await base.OnUnload();
            await DispatchEvent(SceneStateObserverPhase.PreLoad, o => o.OnSceneStateUnload(observable));
        }

        public override async Task OnPostUnload()
        {
            await base.OnPostUnload();
            await DispatchEvent(SceneStateObserverPhase.Initialized, o => o.OnSceneStatePostUnload(observable));
        }


        public void ObserveSceneState(ISceneStateLifecycleObserver<TObserve> observer)
        {
            if (!observers.ContainsKey(observer))
            {
                Digest(true, async () =>
                {
                    SceneStateObserverPhase phase = SceneStateObserverPhase.Initialized;

                    if (targetObserverPhase >= SceneStateObserverPhase.PreLoad && targetSceneStatePhase >= SceneStatePhase.Loaded)
                    {
                        await observer.OnSceneStatePreLoad(observable);
                        phase = SceneStateObserverPhase.PreLoad;

                        if (targetObserverPhase >= SceneStateObserverPhase.Load)
                        {
                            await observer.OnSceneStateLoad(observable);
                            phase = SceneStateObserverPhase.Load;
                        }

                        if (targetObserverPhase >= SceneStateObserverPhase.PostLoad)
                        {
                            await observer.OnSceneStatePostLoad(observable);
                            phase = SceneStateObserverPhase.PostLoad;
                        }
                    }

                    if (targetObserverPhase >= SceneStateObserverPhase.PreMakeVisible && targetSceneStatePhase >= SceneStatePhase.Visible)
                    {
                        await observer.OnSceneStatePreMakeVisible(observable);
                        phase = SceneStateObserverPhase.PreMakeVisible;

                        if (targetObserverPhase >= SceneStateObserverPhase.MakeVisible)
                        {
                            await observer.OnSceneStateMakeVisible(observable);
                            phase = SceneStateObserverPhase.MakeVisible;
                        }

                        if (targetObserverPhase >= SceneStateObserverPhase.PostMakeVisible)
                        {
                            await observer.OnSceneStatePostMakeVisible(observable);
                            phase = SceneStateObserverPhase.PostMakeVisible;
                        }
                    }

                    if (targetObserverPhase >= SceneStateObserverPhase.PreFocus && targetSceneStatePhase >= SceneStatePhase.Focused)
                    {
                        await observer.OnSceneStatePreFocus(observable);
                        phase = SceneStateObserverPhase.PreFocus;

                        if (targetObserverPhase >= SceneStateObserverPhase.Focus)
                        {
                            await observer.OnSceneStateFocus(observable);
                            phase = SceneStateObserverPhase.Focus;
                        }

                        if (targetObserverPhase >= SceneStateObserverPhase.PostFocus)
                        {
                            await observer.OnSceneStatePostFocus(observable);
                            phase = SceneStateObserverPhase.PostFocus;
                        }
                    }

                    observers[observer] = phase;
                });
            }
        }

        public void UnobserveSeneState(ISceneStateLifecycleObserver<TObserve> observer)
        {
            if (observers.ContainsKey(observer))
            {
                Digest(false, () =>
                {
                    observers.Remove(observer);
                    return Task.CompletedTask;
                });
            } 
        }

        private async Task DispatchEvent(SceneStateObserverPhase targetObserverPhase, Func<ISceneStateLifecycleObserver<TObserve>, Task> fn)
        {
            if (this.targetObserverPhase != targetObserverPhase)
            {
                await Digest(false, async () =>
                {
                    await Task.Yield();

                    if (targetObserverPhase > this.targetObserverPhase)
                    {
                        if (targetObserverPhase >= SceneStateObserverPhase.PreLoad) targetSceneStatePhase = SceneStatePhase.Loaded;
                        if (targetObserverPhase >= SceneStateObserverPhase.PreMakeVisible) targetSceneStatePhase = SceneStatePhase.Visible;
                        if (targetObserverPhase >= SceneStateObserverPhase.PreFocus) targetSceneStatePhase = SceneStatePhase.Focused;
                        tempObservers.AddRange(from p in observers where p.Value < targetObserverPhase select p.Key);
                    }
                    else
                    {
                        if (targetObserverPhase < SceneStateObserverPhase.PostFocus) targetSceneStatePhase = SceneStatePhase.Visible;
                        if (targetObserverPhase < SceneStateObserverPhase.PostMakeVisible) targetSceneStatePhase = SceneStatePhase.Loaded;
                        if (targetObserverPhase < SceneStateObserverPhase.PostLoad) targetSceneStatePhase = SceneStatePhase.Initialized;
                        tempObservers.AddRange(from p in observers where p.Value > targetObserverPhase select p.Key);
                    }
                    this.targetObserverPhase = targetObserverPhase;
                    
                    //print($"{GetType().Name} target is {Enum.GetName(typeof(SceneStateObserverPhase), targetObserverPhase)}");
                    //print($"{GetType().Name} observers has {observers.Count} and temp has {tempObservers.Count}");

                    List<Task> tasks = ListPool<Task>.Get();
                    tasks.AddRange(tempObservers.Select(async o =>
                    {
                        await fn(o);
                        observers[o] = targetObserverPhase;
                    }));

                    await Task.WhenAll(tasks.ToArray());
                    //print($"{GetType().Name}'s wait finished");

                    ListPool<Task>.Release(tasks);
                    tempObservers.Clear();
                });
            }
        }


        private struct DigestMessage
        {
            public bool parallel;
            public Func<Task> fn;
        }

        private static readonly List<ISceneStateLifecycleObserver<TObserve>> tempObservers = new List<ISceneStateLifecycleObserver<TObserve>>();
        private readonly Action<Task> taskErrorHandler = t => Debug.LogException(t.Exception);
        private Task digestMethodTask = Task.CompletedTask;
        private Task digestingTask = Task.CompletedTask;
        private readonly Queue<DigestMessage> digestMessages = new Queue<DigestMessage>();
        private bool lastParallel = true;

        private Task Digest(bool parallel, Func<Task> fn)
        {
            digestMessages.Enqueue(new DigestMessage { parallel = parallel, fn = fn });
            if (digestMethodTask.IsCompleted)
            {
                digestMethodTask = new Func<Task>(async () =>
                {
                    try
                    {
                        await digestMethodTask;
                        await AsyncDigest();
                    } catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                })();
                
            }
            return digestMethodTask;
        }

        private async Task AsyncDigest()
        {
            while (digestMessages.Count > 0)
            {
                DigestMessage message = digestMessages.Dequeue();

                if (!message.parallel || !lastParallel)
                {
                    await digestingTask;
                }

                if (message.parallel)
                {
                    List<Task> tasks = ListPool<Task>.Get();
                    tasks.Add(digestingTask);
                    tasks.Add(message.fn());
                    digestingTask = new Func<Task>(async () =>
                    {
                        await Task.WhenAll(tasks);
                        ListPool<Task>.Release(tasks);
                    })();
                    lastParallel = true;
                }
                else
                {
                    digestingTask = new Func<Task>(async () =>
                    {
                        await message.fn();
                    })();
                    lastParallel = false;
                }
            }
        }
    }

    public abstract class SceneStateLifecycleObserver<T> : ISceneStateLifecycleObserver<T> where T : class, ILifecycleObservableSceneState<T>
    {
        public virtual Task OnSceneStatePreLoad(T sceneState) => Task.CompletedTask;
        public virtual Task OnSceneStateLoad(T sceneState) => Task.CompletedTask;
        public virtual Task OnSceneStatePostLoad(T sceneState) => Task.CompletedTask;

        public virtual Task OnSceneStatePreMakeVisible(T sceneState) => Task.CompletedTask;
        public virtual Task OnSceneStateMakeVisible(T sceneState) => Task.CompletedTask;
        public virtual Task OnSceneStatePostMakeVisible(T sceneState) => Task.CompletedTask;

        public virtual Task OnSceneStatePreFocus(T sceneState) => Task.CompletedTask;
        public virtual Task OnSceneStateFocus(T sceneState) => Task.CompletedTask;
        public virtual Task OnSceneStatePostFocus(T sceneState) => Task.CompletedTask;

        public virtual Task OnSceneStatePreBlur(T sceneState) => Task.CompletedTask;
        public virtual Task OnSceneStateBlur(T sceneState) => Task.CompletedTask;
        public virtual Task OnSceneStatePostBlur(T sceneState) => Task.CompletedTask;

        public virtual Task OnSceneStatePreMakeInvisible(T sceneState) => Task.CompletedTask;
        public virtual Task OnSceneStateMakeInvisible(T sceneState) => Task.CompletedTask;
        public virtual Task OnSceneStatePostMakeInvisible(T sceneState) => Task.CompletedTask;

        public virtual Task OnSceneStatePostUnload(T sceneState) => Task.CompletedTask;
        public virtual Task OnSceneStateUnload(T sceneState) => Task.CompletedTask;
        public virtual Task OnSceneStatePreUnload(T sceneState) => Task.CompletedTask;
    }


    public static class ObservableSceneState<T> where T : class, ILifecycleObservableSceneState<T>
    {
        public static T instance { get; private set; }

        public static void Observe(ISceneStateLifecycleObserver<T> observer)
        {
            instance.ObserveSceneState(observer);
        }

        public static void Unobserve(ISceneStateLifecycleObserver<T> observer)
        {
            instance?.UnobserveSeneState(observer);
        }

        public static bool Register(T instance)
        {
            if (ReferenceEquals(ObservableSceneState<T>.instance, null))
            {
                ObservableSceneState<T>.instance = instance;
                return true;
            }
            return false;
        }

        public static bool Unregister(T instance)
        {
            if (ReferenceEquals(ObservableSceneState<T>.instance, instance))
            {
                ObservableSceneState<T>.instance = null;
                return true;
            }
            return false;
        }
    }
}
