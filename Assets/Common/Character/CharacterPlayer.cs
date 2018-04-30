using UnityEngine;

namespace APlusOrFail.Character
{
    public class CharacterPlayer : MonoBehaviour
    {
        private IReadOnlyPlayerSetting _player;
        public IReadOnlyPlayerSetting player
        {
            get
            {
                return _player;
            }
            set
            {
                _player = value;
                //onPlayerChanged?.Invoke(this, value);
            }
        }
        //public event EventHandler<CharacterPlayer, PlayerSetting> onPlayerChanged;
    }
}
