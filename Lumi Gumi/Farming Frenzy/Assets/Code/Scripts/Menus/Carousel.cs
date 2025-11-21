using UnityEngine;

namespace Code.Scripts.Menus
{
    public class Carousel : MonoBehaviour
    {
        #region Editor Fields
        [SerializeField] private GameObject[] _helpPages;
        #endregion

        #region Properties
        private int _activePage = 0;
        #endregion

        #region Methods
        public void ToggleNext()
        {
            _activePage = (_activePage+1)%_helpPages.Length;
            ToggleActivePage();
        }

        public void TogglePrev()
        {
            _activePage = (_activePage - 1 + _helpPages.Length) % _helpPages.Length;
            ToggleActivePage();
        }

        private void ToggleActivePage()
        {
            foreach (var page in _helpPages)
            {
                page.SetActive(false);
            }
            _helpPages[_activePage].SetActive(true);

        }
        #endregion
    }
}
