using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace APlusOrFail.Setup.SceneStates
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Image))]
    public class ColorButton : MonoBehaviour, IPointerClickHandler
    {
        public Color color;
        public string text;
        public event EventHandler<ColorButton> onSelected;

        private void Start()
        {
            ApplyProperties();
        }

        private void OnValidate()
        {
            ApplyProperties();
        }

        private void ApplyProperties()
        {
            GetComponent<Image>().color = color;
            GetComponentInChildren<Text>().text = text;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            onSelected?.Invoke(this);
        }
    }
}
