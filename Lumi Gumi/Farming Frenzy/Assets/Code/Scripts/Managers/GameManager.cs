using System;
using System.Linq;
using Code.Scripts.Player;
using Code.Scripts.GridSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using Code.Scripts.Menus;
using Object = UnityEngine.Object;

namespace Code.Scripts.Managers
{
    public class GameManager : MonoBehaviour
    {
        private static readonly int NightTime = Animator.StringToHash("NightTime");

        #region Editor Fields

        [Header("Menus")]
        [SerializeField] private Canvas _canvas;
        [SerializeField] private GameObject _pauseMenu;
        [SerializeField] private GameObject _pauseBaseMenu;
        [SerializeField] private GameObject _shopMenu;
        [SerializeField] private GameObject _helpMenu;
        [SerializeField] private GameObject _optionsMenu;

        [SerializeField] private GameObject _gameOverMenu;
        [SerializeField] private TextMeshProUGUI _score;
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private TextMeshProUGUI _dayText;
        [SerializeField] private TextMeshProUGUI _weekText;
        [SerializeField] private TextMeshProUGUI _quotaText;
        [SerializeField] private TextMeshProUGUI _quotaPayText;
        [SerializeField] private GameObject _quotaButton;
        [SerializeField] private Image _clockHand;
        [SerializeField] private Animator _dayNightAnimator;
        [SerializeField] private Image _quotaButtonImg;
        [SerializeField] private GameObject _floatingTextPrefab;
        
        [Header("Game Options")]
        [SerializeField] private int _quota;
        [SerializeField] private int _quotaIncreaseRate;
        [SerializeField] private float _timerRate;
        [SerializeField] private float _dayTime; [Tooltip("Daytime in Seconds")]
        [SerializeField] private int _enemyDifficulty;
        [SerializeField] private int _enemySpawnFrequency;
        #endregion

        #region Properties
        public static GameManager Instance;
        public bool Paused { get; private set; }
        private float _time;
        private float _timeLeft;
        private float _toggleStartTime;
        private bool _quotaClose;
        private Color32 _quotaBaseCol;
        private int _quotaPaymentLeft;
        private bool _playFirstClockSound;
        private bool _playSecondClockSound;
        private int _dayCount;
        private int _weekCount;
        private int _currentQuotaPayment;
        private bool _animationStarted;
        private float _lastFloatingText;
        private bool IsTimerRunning { get; set; }
        private int QuotaPayPerClick => _quota / 10;

        public int _goats;
        private readonly string[] _days = {"Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"};

        #endregion

        private void Start()
        {
            AudioManager.Instance.SetInitialMusicVolume();
            GridManager.Instance.Restart();
            EnemySpawnManager.Instance.Restart();
            PlantManager.Instance._camera = Camera.main;
            IsTimerRunning = true;
            _timeLeft = _dayTime * 7;
            // TODO standardise $ vs G
            SetupQuotaText();
            _goats = 0;
            _quotaClose = false;
            _quotaBaseCol = _quotaButtonImg.color;
            _quotaPaymentLeft = _quota;
            _playFirstClockSound = false;
            _playSecondClockSound = false;
            ShopUI.Instance.SetHidden(false);
            //Start up tutorial on first play
            if(AudioManager.Instance.gameStart) {
                print("Tut opened");
                PauseGame();
                OpenTutorial();
            }
            AudioManager.Instance.gameStart = false;
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        #region Methods
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
            {
                if (!Paused) { PauseGame(); }
                else { ResumeGame(); }
            }

            if (!IsTimerRunning) return;
            _time += Time.deltaTime;
            _timeLeft -= Time.deltaTime;


            UpdateQuotaClose();
            UpdateTimerDisplay();
            UpdateGoatCount();
            UpdateDate();
        }

        private void UpdateTimerDisplay()
        {
            float minutes = Mathf.FloorToInt(_timeLeft / 60);
            float seconds = Mathf.FloorToInt(_timeLeft % 60);
            _timerText.text = $"{minutes:00}:{seconds:00}";
        }

        public void ShowFloatingText(string text)
        {
            if (_lastFloatingText != 0 && Time.time - _lastFloatingText < 0.5f)
            {
                return;
            }

            _lastFloatingText = Time.time;

            var id = Quaternion.identity;
            var pos = Input.mousePosition / _canvas.scaleFactor;
            pos.z = 0;
    
            var floatingText = Instantiate(_floatingTextPrefab, pos, id, _canvas.transform);
            floatingText.GetComponent<FloatingText>().SetText(text, pos);
        }

        private void UpdateQuotaClose() {
            if (!_quotaClose && _timeLeft <= _dayTime * 3 + 2 && _quotaPaymentLeft > 0)
            {
                _quotaClose = true;           
                _quotaButtonImg.color = Color.red;          
                _toggleStartTime = Time.time;   
            }

            if (_quotaClose && _quotaPaymentLeft > 0)
            {
                if(_timeLeft <= 6)
                {
                    if(!_playSecondClockSound) {
                        AudioManager.Instance.PlaySFX("clockFast");
                        _playSecondClockSound = true;
                    }
                    if (Mathf.FloorToInt((Time.time - _toggleStartTime)/0.5f) % 2 == 0)
                    {
                        _quotaButtonImg.color = _quotaBaseCol;
                    }
                    else
                    {
                        _quotaButtonImg.color = Color.red;
                    }
                } else {
                    if(_timeLeft <= 10 && !_playFirstClockSound) {
                        AudioManager.Instance.PlaySFX("clockSlow");
                        _playFirstClockSound = true;
                    }

                    if (Mathf.FloorToInt(Time.time - _toggleStartTime) % 2 == 0)
                    {
                        _quotaButtonImg.color = _quotaBaseCol;
                    }
                    else
                    {
                        _quotaButtonImg.color = Color.red;
                    }
                }
            }

            if(_quotaPaymentLeft <= 0) _quotaButtonImg.color = _quotaBaseCol;
        }

        private void UpdateDate()
        {
            var nextDayTime = (_dayCount + 1) * _dayTime;

            // Start animation 3s before
            if (!_animationStarted && _time >= nextDayTime - 3)
            {
                _animationStarted = true;
                _dayNightAnimator.SetTrigger(NightTime);
            }

            if (_time >= nextDayTime)
            {
                _dayCount++;
                AudioManager.Instance.PlaySFX("rooster");
                _animationStarted = false;

                // Spawn only once every $period days
                var rightDayForSpawn = _dayCount % _enemySpawnFrequency == 0;
                var monOrTues = _dayCount % 7 is 0 or 1; // Give mon & tues off

                // Only spawn on Thursday and Saturday in Week 1 to reduce load on player
                var gracePeriod = _dayCount <= 7;
                var reducedSpawnDay = gracePeriod && _dayCount is not (3 or 5);

                if(rightDayForSpawn && !reducedSpawnDay && !monOrTues)
                {
                    var week = Mathf.CeilToInt(_dayCount / 7.0f);
                    var numEnemies = Mathf.RoundToInt((1.0f + 2f / 5f) * Random.Range(1.0f, 2.0f) * Math.Max(1, (float) Math.Pow(week - 1, 2)));
                    EnemySpawnManager.Instance.SpawnEnemies(numEnemies);
                    for(int i = 0; i < Math.Min(numEnemies, 5); i++) {
                        AudioManager.Instance.PlayRandomGoatNoise();
                    }
                }

                if (_dayCount % 7 == 0)
                {
                    _weekCount++;
                    CheckGameOver();
                }
            }

            var clockFaceAngle = Mathf.FloorToInt(_time) / _dayTime * 360;
            _clockHand.transform.eulerAngles = new Vector3(0, 0, -clockFaceAngle);
            _dayText.text = $"{_days[_dayCount % 7]}";
            _weekText.text = $"Week:{_weekCount + 1:0}";
        }

        private void UpdateGoatCount() {
            var allGameObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            // Count how many of them have the name "Enemy(Clone)"
            var enemyCount = allGameObjects.Count(obj => obj.name.StartsWith("Enemy"));
            _goats = enemyCount;
        }

        private void PauseGame()
        {
            _pauseMenu.gameObject.SetActive(true);
            Time.timeScale = 0f;
            Paused = true;
            PlayerController.Instance.SetPausedCursor();
        }

        private void OpenTutorial()
        {
            _pauseBaseMenu.gameObject.SetActive(false);
            _helpMenu.gameObject.SetActive(true);
            _shopMenu.gameObject.SetActive(false);
        }

        public void ResumeGame()
        {
            _pauseMenu.gameObject.SetActive(false);

            var wasActive = _shopMenu.gameObject.activeSelf;
            _shopMenu.gameObject.SetActive(true);
            if (!wasActive)
            {
                _shopMenu.GetComponent<ShopUI>().InitShop();
            }

            _optionsMenu.gameObject.SetActive(false);
            _pauseBaseMenu.gameObject.SetActive(true);
            _helpMenu.gameObject.SetActive(false);

            Time.timeScale = 1f;
            Paused = false;
        }

        private void SetupQuotaText()
        {
            _quotaText.text = $"You Owe: <b><u>{_quota - _currentQuotaPayment}G</u></b>";
            var canPay = Math.Min(_quota - _currentQuotaPayment, QuotaPayPerClick);
            _quotaPayText.text = $"Pay {canPay}G\n(Shift-click to pay max)";
        }

        public void PayQuota()
        {
            if (_currentQuotaPayment >= _quota) return;

            var amount = QuotaPayPerClick;
            
            var diff = _quota - _currentQuotaPayment;
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                var maxCanBuy = Math.Min(_quota - _currentQuotaPayment, PlayerController.Instance.Money);
                amount = maxCanBuy;
            }

            amount = Math.Min(amount, diff);
            if (amount == 0 || !PlayerController.Instance.TryPurchase(amount)) return;

            AudioManager.Instance.PlaySFX("kaching");
            _quotaButton.GetComponent<Animator>().Play("QuotaClick");

            _currentQuotaPayment += amount;
            _quotaPaymentLeft = _quota - _currentQuotaPayment;
            SetupQuotaText();
        }

        private void CheckGameOver()
        {
            var diff = _quota - _currentQuotaPayment;

            // Player hasn't paid enough
            if (diff > 0)
            {
                if (PlayerController.Instance.Money >= diff)
                {
                    // Just pay for them and carry on
                    PlayerController.Instance.Purchase(diff);
                }
                else
                {
                    // Not enough to pay - game over!
                    Time.timeScale = 0f;
                    AudioManager.Instance.ToggleSFX();
                    AudioManager.Instance.ToggleSFX();
                    AudioManager.Instance.PlaySFX("gameOver");
                    AudioManager.Instance.ToggleMusic();
                    _score.text = $"You got to <u>week {Mathf.CeilToInt(_dayCount / 7.0f)}</u>";
                    _gameOverMenu.SetActive(true);
                    ShopUI.Instance.SetHidden(true);
                    PlayerController.Instance.SetPickedCursor(PlayerController.CursorState.Default, null, null);
                    return;
                }
            }

            // Setup next quota
            _quota *= _quotaIncreaseRate;
            _quotaPaymentLeft = _quota;
            _currentQuotaPayment = 0;
            _timeLeft = _dayTime*7;
            _quotaClose = false;
            _quotaButtonImg.color = _quotaBaseCol;
            _playFirstClockSound = false;
            _playSecondClockSound = true;
            SetupQuotaText();
        }
        #endregion
    }
}
