using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

        [Header("Buttons")]
        [SerializeField] private Button _mainMenuButton; // Used for both Win and Loss
        [SerializeField] private Button _restartButton;  // Only visible on Loss

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

            // 3. Calculate Stars (0 stars if lost)
            int starCount = 0;

            if (isWin)
            {
                starCount = 1;
                float percentageLeft = timeLeft / totalTime;
                if (percentageLeft > 0.2f) starCount++;
                if (percentageLeft > 0.5f) starCount++;
            }

            // 4. Update Star Visuals
            for (int i = 0; i < _stars.Length; i++)
            {
                Image starImg = _stars[i].GetComponent<Image>();
                starImg.sprite = (i < starCount) ? _starFilled : _starEmpty;
            }

            // 5. Button Logic
            SetupButtons(isWin);
        }

        private void SetupButtons(bool isWin)
        {
            // --- MAIN MENU BUTTON (Always visible) ---
            _mainMenuButton.gameObject.SetActive(true);
            _mainMenuButton.onClick.RemoveAllListeners();
            _mainMenuButton.onClick.AddListener(() =>
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(_mainMenuSceneName);
            });

            // --- RESTART BUTTON (Only visible if Failed) ---
            if (isWin)
            {
                _restartButton.gameObject.SetActive(false); // Hide on Win
            }
            else
            {
                _restartButton.gameObject.SetActive(true); // Show on Loss
                _restartButton.onClick.RemoveAllListeners();
                _restartButton.onClick.AddListener(() =>
                {
                    Time.timeScale = 1f;
                    // Reload the current active scene
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                });
            }
        }

        private string FormatTime(float timeInSeconds)
        {
            float minutes = Mathf.FloorToInt(timeInSeconds / 60);
            float seconds = Mathf.FloorToInt(timeInSeconds % 60);
            return $"{minutes:00}:{seconds:00}";
        }
    }
}