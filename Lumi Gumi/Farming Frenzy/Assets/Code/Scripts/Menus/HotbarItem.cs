using Code.Scripts.Player;
using UnityEngine;

namespace Code.Scripts.Menus
{
    public class HotbarItem : MonoBehaviour
    {
        #region Editor Fields
        [SerializeField] private PlayerController.CursorState _cursor;
        #endregion

        public void OnClick()
        {
            PlayerController.Instance.SetPickedCursor(_cursor, null, null);
        }
    }
}
