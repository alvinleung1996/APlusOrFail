using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace APlusOrFail.UI
{
    [RequireComponent(typeof(RectTransform))]
    [ExecuteInEditMode]
    public abstract class BaseLayoutElement : LayoutElement
    {
        //        public virtual void CalculateLayoutInputHorizontal() { }
        //        public virtual void CalculateLayoutInputVertical() { }
        //        public virtual float minWidth => 0;
        //        public virtual float minHeight => 0;
        //        public virtual float preferredWidth => 0;
        //        public virtual float preferredHeight => 0;
        //        public virtual float flexibleWidth => 0;
        //        public virtual float flexibleHeight => 0;
        //        public virtual int layoutPriority => 0;


        //        protected override void OnEnable()
        //        {
        //            base.OnEnable();
        //            SetDirty();
        //        }

        //        protected override void OnTransformParentChanged()
        //        {
        //            SetDirty();
        //        }

        //        protected override void OnDisable()
        //        {
        //            SetDirty();
        //            base.OnDisable();
        //        }

        //        protected override void OnDidApplyAnimationProperties()
        //        {
        //            SetDirty();
        //        }

        //        protected override void OnBeforeTransformParentChanged()
        //        {
        //            SetDirty();
        //        }

        //        protected void SetDirty()
        //        {
        //            if (IsActive())
        //            {
        //                LayoutRebuilder.MarkLayoutForRebuild(transform as RectTransform);
        //            }
        //        }

        protected void SetProperty<T>(ref T currentValue, T newValue)
        {
            if (!currentValue.Equals(newValue))
            {
                currentValue = newValue;
                SetDirty();
            }
        }

        //#if UNITY_EDITOR
        //        protected override void OnValidate()
        //        {
        //            SetDirty();
        //        }
        //#endif
    }
}
