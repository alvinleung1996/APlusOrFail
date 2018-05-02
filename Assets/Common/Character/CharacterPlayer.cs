using UnityEngine;

namespace APlusOrFail.Character
{
    public class CharacterPlayer : MonoBehaviour
    {
        private IReadOnlySharedPlayerSetting _playerSetting;
        public IReadOnlySharedPlayerSetting playerSetting
        {
            get
            {
                return _playerSetting;
            }
            set
            {
                _playerSetting = value;
                //onPlayerChanged?.Invoke(this, value);
            }
        }
        //public event EventHandler<CharacterPlayer, PlayerSetting> onPlayerChanged;
    }
}
