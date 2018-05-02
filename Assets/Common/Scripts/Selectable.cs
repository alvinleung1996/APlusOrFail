using UnityEngine;

namespace APlusOrFail
{
    public class Selectable : MonoBehaviour
    {
        public event EventHandler<Selectable> onSelected;
    
        private void OnMouseDown()
        {
            onSelected?.Invoke(this);
        }
    }
}
