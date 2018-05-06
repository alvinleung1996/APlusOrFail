using System;
using UnityEngine;
using UnityEngine.UI;

namespace APlusOrFail.UI
{
    public class FractionLayoutController : LayoutGroup
    {
        [SerializeField]
        private bool _vertical = false;
        public bool vertical { get { return _vertical; } set { SetProperty(ref _vertical, value); } }

        [SerializeField]
        private float _spacing = 0;
        public float spacing { get { return _spacing; } set { SetProperty(ref _spacing, value); } }

        [SerializeField]
        private bool _childForceExpandWidth = true;
        public bool childForceExpandWidth { get { return _childForceExpandWidth; } set { SetProperty(ref _childForceExpandWidth, value); } }

        [SerializeField]
        private bool _childForceExpandHeight = true;
        public bool childForceExpandHeight { get { return _childForceExpandHeight; } set { SetProperty(ref _childForceExpandHeight, value); } }

        [SerializeField]
        private bool _childControlWidth = true;
        public bool childControlWidth { get { return _childControlWidth; } set { SetProperty(ref _childControlWidth, value); } }

        [SerializeField]
        private bool _childControlHeight = true;
        public bool childControlHeight { get { return _childControlHeight; } set { SetProperty(ref _childControlHeight, value); } }

        [SerializeField]
        private float _denominatorWidth = 1;
        public float denominatorWidth { get { return _denominatorWidth; } set { SetProperty(ref _denominatorWidth, value); } }

        [SerializeField]
        private float _denominatorHeight = 1;
        public float denominatorHeight { get { return _denominatorHeight; } set { SetProperty(ref _denominatorHeight, value); } }


        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();
            CalcAlongAxis(0);
        }

        public override void CalculateLayoutInputVertical()
        {
            CalcAlongAxis(1);
        }

        protected void CalcAlongAxis(int axis)
        {
            bool alongOtherAxis = (axis == 1) ^ vertical;

            float combinedPadding = axis == 0 ? padding.horizontal : padding.vertical;
            bool controlSize = axis == 0 ? childControlWidth : childControlHeight;
            bool childForceExpandSize = axis == 0 ? childForceExpandWidth : childForceExpandHeight;
            float denominator = axis == 0 ? denominatorWidth : denominatorHeight;

            float totalMin = combinedPadding;
            float totalPreferred = combinedPadding;
            float totalFlexible = 0;
            float totalPercentageMin = combinedPadding;
            float totalPercentagePreferred = combinedPadding;

            for (int i = 0; i < rectChildren.Count; ++i)
            {
                RectTransform child = rectChildren[i];
                float min, preferred, flexible, numerator;
                GetChildSizes(child, axis, controlSize, childForceExpandSize, out min, out preferred, out flexible, out numerator);
                float percentage = (denominator > 0 && numerator > 0) ? (numerator / denominator) : -1;
                
                if (alongOtherAxis)
                {
                    if (percentage > 0)
                    {
                        totalMin = Mathf.Max(min / percentage + combinedPadding, totalMin);
                        totalPreferred = Mathf.Max(preferred / percentage + combinedPadding, totalPreferred);
                    }
                    else
                    {
                        totalMin = Mathf.Max(min + combinedPadding, totalMin);
                        totalPreferred = Mathf.Max(preferred + combinedPadding, totalPreferred);
                    }
                    totalFlexible = Mathf.Max(flexible, totalFlexible);
                }
                else
                {
                    if (percentage > 0)
                    {
                        totalPercentageMin = Mathf.Max(min / percentage + combinedPadding, totalPercentageMin);
                        totalPercentagePreferred = Mathf.Max(preferred / percentage + combinedPadding, totalPercentagePreferred);
                    }
                    totalMin += min + (i == (rectChildren.Count - 1) ? spacing : 0);
                    totalPreferred += preferred + (i == (rectChildren.Count - 1) ? spacing : 0);
                    totalFlexible += flexible;
                }
            }
            totalMin = Mathf.Max(totalMin, totalPercentageMin);
            totalPreferred = Mathf.Max(totalPreferred, totalPercentagePreferred);

            totalPreferred = Mathf.Max(totalMin, totalPreferred);
            SetLayoutInputForAxis(totalMin, totalPreferred, totalFlexible, axis);
        }

        public override void SetLayoutHorizontal()
        {
            SetChildrenAlongAxis(0);
        }

        public override void SetLayoutVertical()
        {
            SetChildrenAlongAxis(1);
        }

        protected void SetChildrenAlongAxis(int axis)
        {
            float innerSize = rectTransform.rect.size[axis] - (axis == 0 ? padding.horizontal : padding.vertical);
            bool controlSize = axis == 0 ? childControlWidth : childControlHeight;
            bool childForceExpandSize = axis == 0 ? childForceExpandWidth : childForceExpandHeight;
            float denominator = axis == 0 ? denominatorWidth : denominatorHeight;
            float alignmentOnAxis = GetAlignmentOnAxis(axis);

            bool alongOtherAxis = (axis == 1) ^ vertical;
            if (alongOtherAxis)
            {
                for (int i = 0; i < rectChildren.Count; ++i)
                {
                    RectTransform child = rectChildren[i];
                    float min, preferred, flexible, numerator;
                    GetChildSizes(child, axis, controlSize, childForceExpandSize, out min, out preferred, out flexible, out numerator);
                    float percentage = (denominator > 0 && numerator > 0) ? (numerator / denominator) : -1;

                    float requiredSpace;
                    if (flexible > 0)
                    {
                        requiredSpace = Mathf.Max(innerSize, min);
                    }
                    else if (percentage > 0)
                    {
                        requiredSpace = Mathf.Max(innerSize * percentage, min);
                    }
                    else
                    {
                        requiredSpace = Mathf.Clamp(innerSize, min, preferred);
                    }

                    float startOffset = GetStartOffset(axis, requiredSpace);

                    if (controlSize)
                    {
                        SetChildAlongAxis(child, axis, startOffset, requiredSpace);
                    }
                    else
                    {
                        float offsetInCell = (requiredSpace - child.sizeDelta[axis]) * alignmentOnAxis;
                        SetChildAlongAxis(child, axis, startOffset + offsetInCell);
                    }
                }
            }
            else
            {
                float pos = axis == 0 ? padding.left : padding.top;

                float percentageSize = 0;
                float totalNonpercentageMin = 0;
                float totalNonPercentagePreferred = 0;
                for (int i = 0; i < rectChildren.Count; ++i)
                {
                    RectTransform child = rectChildren[i];
                    float min, preferred, flexible, numerator;
                    GetChildSizes(child, axis, controlSize, childForceExpandSize, out min, out preferred, out flexible, out numerator);
                    float percentage = (denominator > 0 && numerator > 0) ? (numerator / denominator) : -1;

                    if (percentage > 0)
                    {
                        percentageSize += Mathf.Max(innerSize * percentage, min);
                    }
                    else
                    {
                        totalNonpercentageMin += min;
                        totalNonPercentagePreferred += preferred;
                    }
                }
                float totalInnerMin = percentageSize + totalNonpercentageMin;
                float totalInnerPreferred = percentageSize + totalNonPercentagePreferred;
                float totalFlexible = GetTotalFlexibleSize(axis);

                if (totalFlexible == 0 && totalInnerPreferred < innerSize)
                {
                    pos = GetStartOffset(axis, totalInnerPreferred);
                }

                float minMaxLerp = 0;
                if (totalInnerMin != totalInnerPreferred)
                {
                    minMaxLerp = Mathf.Clamp01((innerSize - totalInnerMin) / (totalInnerPreferred - totalInnerPreferred));
                }

                float itemFlexibleMultiplier = 0;
                if (innerSize > totalInnerPreferred && totalFlexible > 0)
                {
                    itemFlexibleMultiplier = (innerSize - totalInnerPreferred) / totalFlexible;
                }

                for (int i = 0; i < rectChildren.Count; ++i)
                {
                    RectTransform child = rectChildren[i];
                    float min, preferred, flexible, numerator;
                    GetChildSizes(child, axis, controlSize, childForceExpandSize, out min, out preferred, out flexible, out numerator);
                    float percentage = (denominator > 0 && numerator > 0) ? (numerator / denominator) : -1;

                    float childSize;
                    if (percentage > 0)
                    {
                        childSize = Mathf.Max(innerSize * percentage, min);
                    }
                    else
                    {
                        childSize = Mathf.Lerp(min, preferred, minMaxLerp);
                    }
                    childSize += flexible * itemFlexibleMultiplier;

                    if (controlSize)
                    {
                        SetChildAlongAxis(child, axis, pos, childSize);
                    }
                    else
                    {
                        float offsetInCell = (childSize - child.sizeDelta[axis]) * alignmentOnAxis;
                        SetChildAlongAxis(child, axis, pos + offsetInCell);
                    }

                    pos += childSize + spacing;
                }
            }
        }


        private static void GetChildSizes(RectTransform child, int axis, bool controlSize, bool childForceExpand,
            out float min, out float preferred, out float flexible, out float numerator)
        {
            if (!controlSize)
            {
                min = child.sizeDelta[axis];
                preferred = min;
                flexible = 0;
                numerator = 0;
            }
            else
            {
                min = LayoutUtility.GetMinSize(child, axis);
                preferred = LayoutUtility.GetPreferredSize(child, axis);
                flexible = LayoutUtility.GetFlexibleSize(child, axis);
                numerator = GetNumerator(child, axis);
            }

            if (childForceExpand)
                flexible = Mathf.Max(flexible, 1);
        }


        public static float GetNumerator(RectTransform rect, int axis)
        {
            return GetIFractionProperty(rect, p => axis == 0 ? p.numeratorWidth : p.numeratorHeight, 0);
        }

        public static float GetIFractionProperty(RectTransform rect, Func<IFractionLayoutElement, float> property, float defaultValue)
        {
            return LayoutUtility.GetLayoutProperty(rect, e =>
            {
                IFractionLayoutElement p = e as IFractionLayoutElement;
                return p != null ? property(p) : -1;
            }, defaultValue);
        }
    }

    public interface IFractionLayoutElement : ILayoutElement
    {
        float numeratorWidth { get; }
        float numeratorHeight { get; }
    }
}
