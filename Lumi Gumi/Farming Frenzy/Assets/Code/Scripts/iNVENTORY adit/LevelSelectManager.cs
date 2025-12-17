using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic; // Added to use Dictionary/Lists

namespace Code.Scripts.Menus
{
    public class LevelSelectManager : MonoBehaviour
    {
        [System.Serializable]
        public struct LevelButton
        {
            public Button button;
            public Image buttonImage;
            public string sceneName;
            public TextMeshProUGUI levelNumberText;
        }

        [Header("Levels Configuration")]
        [SerializeField] private LevelButton[] _levels;
        [SerializeField] private Sprite _lockedSprite;
        [SerializeField] private Sprite _unlockedSprite;

        [Header("Popup References")]
        [SerializeField] private GameObject _starPopupPanel;
        [SerializeField] private GameObject[] _stars;
        [SerializeField] private Sprite _starFilled;
        [SerializeField] private Sprite _starEmpty;

        [Header("Popup Text Data")]
        [SerializeField] private TextMeshProUGUI _popupLevelName;
        [SerializeField] private TextMeshProUGUI _debtText;     // NEW
        [SerializeField] private TextMeshProUGUI _harvestText;  // NEW
        [SerializeField] private TextMeshProUGUI _timeText;     // NEW

        [Header("Buttons")]
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _backButton;

        private string _selectedLevel;

        // Store the original sizes of buttons so we can restore them when unlocked
        private Vector2[] _originalSizes;

        private void Start()
        {
            // 1. Capture the original sizes BEFORE we change anything
            _originalSizes = new Vector2[_levels.Length];
            for (int i = 0; i < _levels.Length; i++)
            {
                if (_levels[i].buttonImage != null)
                {
                    _originalSizes[i] = _levels[i].buttonImage.rectTransform.sizeDelta;
                }
            }

            UpdateLevelButtons();
            _starPopupPanel.SetActive(false);
            _backButton.onClick.AddListener(ClosePopup);
        }

        private void UpdateLevelButtons()
        {
            for (int i = 0; i < _levels.Length; i++)
            {
                string levelName = _levels[i].sceneName;
                bool isUnlocked = false;

                // Level 1 is always unlocked
                if (i == 0) isUnlocked = true;
                else
                {
                    string prevLevelName = _levels[i - 1].sceneName;
                    int prevStars = PlayerPrefs.GetInt(prevLevelName + "_Stars", 0);
                    isUnlocked = prevStars > 0;
                }

                var rectTransform = _levels[i].buttonImage.rectTransform;

                if (isUnlocked)
                {
                    // UNLOCKED STATE
                    _levels[i].buttonImage.sprite = _unlockedSprite;
                    _levels[i].button.interactable = true;
                    _levels[i].levelNumberText.gameObject.SetActive(true);

                    // Restore the original width/height
                    if (_originalSizes != null && _originalSizes.Length > i)
                    {
                        rectTransform.sizeDelta = _originalSizes[i];
                    }

                    int index = i;
                    _levels[i].button.onClick.RemoveAllListeners();
                    _levels[i].button.onClick.AddListener(() => OnLevelClicked(index));
                }
                else
                {
                    // LOCKED STATE
                    _levels[i].buttonImage.sprite = _lockedSprite;
                    _levels[i].button.interactable = false;
                    _levels[i].levelNumberText.gameObject.SetActive(false);

                    // Set Height to 286.8, Keep original Width
                    float currentWidth = rectTransform.sizeDelta.x;
                    rectTransform.sizeDelta = new Vector2(currentWidth, 286.8f);
                }
            }
        }

        public void OnLevelClicked(int index)
        {
            string levelName = _levels[index].sceneName;
            _selectedLevel = levelName;

            // 1. Load Saved Data
            int starsEarned = PlayerPrefs.GetInt(levelName + "_Stars", 0);
            int harvestCount = PlayerPrefs.GetInt(levelName + "_Harvest", 0);
            float bestTime = PlayerPrefs.GetFloat(levelName + "_BestTime", 0f);
            float totalTime = PlayerPrefs.GetFloat(levelName + "_TotalTime", 0f);

            // 2. Show Popup
            _starPopupPanel.SetActive(true);
            _popupLevelName.text = "Level " + (index + 1);

            // 3. Update Text Stats
            if (starsEarned > 0)
            {
                _debtText.text = "Debt Paid!";
                _harvestText.text = $"{harvestCount}";
                _timeText.text = $"{FormatTime(bestTime)} / {FormatTime(totalTime)}";
            }
            else
            {
                // If never played/won, show placeholders
                _debtText.text = "Debt: ???";
                _harvestText.text = "0";
                _timeText.text = "--:-- / --:--";
            }

            // 4. Update Star Images
            for (int i = 0; i < _stars.Length; i++)
            {
                Image starImg = _stars[i].GetComponent<Image>();
                starImg.sprite = (i < starsEarned) ? _starFilled : _starEmpty;
            }

            // 5. Setup Play Button
            _playButton.onClick.RemoveAllListeners();
            _playButton.onClick.AddListener(() =>
            {
                SceneManager.LoadScene(_selectedLevel);
            });
        }

        public void ClosePopup()
        {
            _starPopupPanel.SetActive(false);
        }

        private string FormatTime(float timeInSeconds)
        {
            if (timeInSeconds <= 0) return "00:00"; // Safety check
            float minutes = Mathf.FloorToInt(timeInSeconds / 60);
            float seconds = Mathf.FloorToInt(timeInSeconds % 60);
            return $"{minutes:00}:{seconds:00}";
        }

        [ContextMenu("Reset All Progress")]
        public void ResetProgress()
        {
            PlayerPrefs.DeleteAll();
            UpdateLevelButtons();
            Debug.Log("Progress Reset!");
        }
    }
}