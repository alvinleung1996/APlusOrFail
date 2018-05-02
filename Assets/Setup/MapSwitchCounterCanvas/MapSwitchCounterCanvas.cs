using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace APlusOrFail.MapSwitchCounter
{
    [System.Obsolete]
    public class MapSwitchCounterCanvas : MonoBehaviour
    {
        public class PendingSwitching
        {
            public float startTime;
            public int sceneBuildIndex;

            private int _priority;
            public int priority
            {
                get
                {
                    return _priority;
                }
                set
                {
                    if (_priority != value)
                    {
                        _priority = value;
                        mapSwitchCounterCanvas.UpdatePendingSwitching();
                    }
                }
            }

            private readonly MapSwitchCounterCanvas mapSwitchCounterCanvas;

            public PendingSwitching(MapSwitchCounterCanvas mapSwitchCounterCanvas, int sceneBuildIndex)
            {
                this.mapSwitchCounterCanvas = mapSwitchCounterCanvas;
                this.sceneBuildIndex = sceneBuildIndex;
            }

            public void Cancel()
            {
                mapSwitchCounterCanvas.UnscheduleSwitch(this);
            }
        }

        public static MapSwitchCounterCanvas instance { get; private set; }

        public RectTransform counterRectTransform;
        public Text counterText;
        public float waitTime = 5;

        private readonly List<PendingSwitching> pendingSwitchings = new List<PendingSwitching>();

        private PendingSwitching _pendingSwitch;
        private PendingSwitching pendingSwitching
        {
            get
            {
                return _pendingSwitch;
            }
            set
            {
                if (_pendingSwitch != value)
                {
                    _pendingSwitch = value;
                    counterRectTransform.gameObject.SetActive(value != null);
                    if (value != null) value.startTime = Time.time;
                }
                
            }
        }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
                Debug.LogError("Found another map switch counter canvas!");
            }
        }

        private void Start()
        {
            counterRectTransform.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        private void Update()
        {
            if (pendingSwitching != null)
            {
                float remainingTime = waitTime - (Time.time - pendingSwitching.startTime);
                if (remainingTime > 0)
                {
                    counterText.text = $"{Mathf.Ceil(remainingTime)}";
                }
                else
                {
                    SceneManager.LoadSceneAsync(pendingSwitching.sceneBuildIndex);
                    pendingSwitching = null;
                }
            }
        }

        public void UpdatePendingSwitching()
        {
            int? maxPriority = null;
            PendingSwitching selected = null;
            foreach (PendingSwitching switching in pendingSwitchings)
            {
                if (maxPriority == null || switching.priority > maxPriority)
                {
                    maxPriority = switching.priority;
                    selected = switching;
                }
                else if (switching.priority == maxPriority)
                {
                    selected = null;
                    break;
                }
            }
            pendingSwitching = selected;
        }

        public PendingSwitching ScheduleSwitch(int sceneBuildIndex)
        {
            PendingSwitching switching = new PendingSwitching(this, sceneBuildIndex);
            pendingSwitchings.Add(switching);
            UpdatePendingSwitching();
            return switching;
        }

        public void UnscheduleSwitch(PendingSwitching pendingSwitch)
        {
            if (!pendingSwitchings.Remove(pendingSwitch))
            {
                Debug.LogErrorFormat("Cannot find pending switch!");
            }
            UpdatePendingSwitching();
        }
    }
}
