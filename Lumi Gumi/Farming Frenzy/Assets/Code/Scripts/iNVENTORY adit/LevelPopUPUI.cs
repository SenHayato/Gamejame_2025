using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Needed for loading scenes

namespace Code.Scripts.Menus
{
    public class LevelPopupUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject _panel;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _debtText;
        [SerializeField] private TextMeshProUGUI _harvestText;
        [SerializeField] private TextMeshProUGUI _timeText;

        [Header("Stars")]
        [SerializeField] private GameObject[] _stars;
        [SerializeField] private Sprite _starFilled;
        [SerializeField] private Sprite _starEmpty;

        [Header("Button")]
        [SerializeField] private Button _continueButton;

        // Change this string in the Inspector if your menu scene is named differently!
        [SerializeField] private string _mainMenuSceneName = "MainMenu";

        public void ShowPopup(bool isWin, int debtLeft, int totalHarvested, float timeLeft, float totalTime)
        {
            _panel.SetActive(true);

            // 1. Set Title and Colors
            if (isWin)
            {
                _titleText.text = "Level Completed";
                _titleText.color = new Color(0.2f, 0.4f, 0.2f); // Dark Green
            }
            else
            {
                _titleText.text = "Level Failed";
                _titleText.color = Color.red;
            }

            // 2. Set Stats Text
            _debtText.text = isWin ? "Debt Paid!" : $"Debt Left: {debtLeft}G";
            _harvestText.text = $"{totalHarvested}";

            // Format Time
            string timeStr = FormatTime(timeLeft);
            string totalTimeStr = FormatTime(totalTime);
            _timeText.text = $"{timeStr} / {totalTimeStr}";

            // 3. Calculate Stars
            int starCount = 0;

            if (isWin)
            {
                starCount = 1;
                float percentageLeft = timeLeft / totalTime;

                if (percentageLeft > 0.2f) starCount++;
                if (percentageLeft > 0.5f) starCount++;
            }
            else
            {
                starCount = 0;
            }

            // 4. Update Star Visuals
            for (int i = 0; i < _stars.Length; i++)
            {
                Image starImg = _stars[i].GetComponent<Image>();
                starImg.sprite = (i < starCount) ? _starFilled : _starEmpty;
            }

            // 5. Setup Continue Button (Return to Main Menu)
            _continueButton.onClick.RemoveAllListeners();
            _continueButton.onClick.AddListener(() =>
            {
                // Unpause the game so the main menu animations work
                Time.timeScale = 1f;

                // Load the Main Menu Scene
                SceneManager.LoadScene(_mainMenuSceneName);
            });
        }

        private string FormatTime(float timeInSeconds)
        {
            float minutes = Mathf.FloorToInt(timeInSeconds / 60);
            float seconds = Mathf.FloorToInt(timeInSeconds % 60);
            return $"{minutes:00}:{seconds:00}";
        }
    }
}