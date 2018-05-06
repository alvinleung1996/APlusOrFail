using UnityEngine;

namespace APlusOrFail.UI
{
    public class FractionLayoutElement : BaseLayoutElement, IFractionLayoutElement
    {
        [SerializeField] private float _numeratorWidth = -1;
        public float numeratorWidth { get { return _numeratorWidth; } set { SetProperty(ref _numeratorWidth, value); } }

        [SerializeField] private float _numeratorHeight = -1;
        public float numeratorHeight { get { return _numeratorHeight; } set { SetProperty(ref _numeratorHeight, value); } }
    }
}
