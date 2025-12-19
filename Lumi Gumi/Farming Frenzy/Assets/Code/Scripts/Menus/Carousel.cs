using Code.Scripts.Managers;
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

        private void awake()
        {
            //Time.timeScale = 0f;
        }
        private void OnEnable()
        {
            _activePage = 0;
            // Force reset visuals
            for (int i = 0; i < _helpPages.Length; i++)
            {
                _helpPages[i].SetActive(i == 0);
            }
        }

        private void Update()
        {
            Time.timeScale = 0f;

            // Detect "Press Anywhere" (Mouse click or screen tap) to advance
            if (Input.GetMouseButtonDown(0))
            {
                ToggleNext();
            }
        }

        #region Methods
        public void ToggleNext()
        {
            // 1. Turn off the previous (current) page
            if (_activePage >= 0 && _activePage < _helpPages.Length)
            {
                _helpPages[_activePage].SetActive(false);
            }

            // 2. Check if we reached the end
            if (_activePage >= _helpPages.Length - 1)
            {
                // We are at the last page and user clicked.
                // Since we already turned it off above, we now just resume the game.

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.ResumeGame();
                }
                else
                {
                    // Fallback if GameManager is missing
                    gameObject.SetActive(false);
                }
            }
            else
            {
                // 3. Advance to the next page
                _activePage++;
                if (_activePage < _helpPages.Length)
                {
                    _helpPages[_activePage].SetActive(true);
                }
            }
        }
        #endregion
    }
}
