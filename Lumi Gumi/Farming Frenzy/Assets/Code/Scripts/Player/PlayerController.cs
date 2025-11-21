using System;
using Code.Scripts.Managers;
using Code.Scripts.Menus;
using Code.Scripts.Plants.Powers.PowerExtension;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Code.Scripts.Player
{
    public class PlayerController : MonoBehaviour
    {
        #region Editor Fields
        [SerializeField] private Sprite _groundSprite;
        [SerializeField] private int _money;
        [FormerlySerializedAs("_sprayPurchaseAmount")] [SerializeField] public int SprayPurchaseAmount;
        [Header("Cursors")]
        [SerializeField] private Texture2D _defaultCursor;
        [SerializeField] private Texture2D _wateringCanCursor;
        [SerializeField] private Texture2D _shovelCursor;
        [SerializeField] private Texture2D _sprayBottleCursor;
        [SerializeField] private Texture2D _scytheCursor;
        [Header("Hotbar Icons")]
        [SerializeField] private Image _defaultCursorBackground;
        [SerializeField] private Image _shovelCursorBackground;
        [SerializeField] private Image _scytheCursorBackground;
        [Header("Effects")]
        [SerializeField] private ParticleSystem _sprayBottleParticleSystem;
        [SerializeField] private ParticleSystem _coinParticles;
        #endregion

        #region Properties
        public Sprite GroundSprite => _groundSprite;
        public int Money => _money;
        public delegate void MoneyChangeEvent(int newAmount);
        public event MoneyChangeEvent OnMoneyChange;

        [Serializable]
        public enum CursorState { Default, Spray, Shovel, Scythe, Planting }

        private CursorState? _contextualCursor;
        private CursorState _currentlyPicked = CursorState.Default;
        [CanBeNull] private PlantData _currentPlant;
        [CanBeNull] private Texture2D _seedBagTexture;
        public CursorState CurrentlyActiveCursor => GameManager.Instance.Paused ? CursorState.Default : _contextualCursor ?? _currentlyPicked;
        public bool IsContextualActive => _contextualCursor != null;

        private Color _defaultCursorBackgroundColor;
        private readonly Color _activeCursorBackgroundColor = new Color32(149, 81,19, 255);
        public float lastHarvestablePlant;
        private float? _lastContextualCursor;

        #endregion

        #region Singleton
        public static PlayerController Instance;
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
        #endregion
        
        #region Methods
        private void Start()
        {
            _defaultCursorBackgroundColor = _defaultCursorBackground.color;
        }
        public void Purchase(int amount)
        {
            _money -= amount;
            OnMoneyChange?.Invoke(_money);
            _coinParticles.Play();
        }

        public void SetPausedCursor()
        {
            lock (this)
            {
                Cursor.SetCursor(_defaultCursor, Vector2.zero, CursorMode.Auto);
            }
        }

        public bool TryPurchase(int amount)
        {
            if (_money < amount) return false;
            Purchase(amount);
            return true;
        }


        public void IncreaseMoney(int amount)
        {
            _money += amount;
            OnMoneyChange?.Invoke(_money);
        }

        /// <summary>
        /// Sends out an OnMoneyChange event to refresh UI elements
        /// </summary>
        public void RefreshMoney()
        {
            OnMoneyChange?.Invoke(_money);
        }

        public void StartContextualCursor(CursorState contextual)
        {
            lock (this)
            {
                _contextualCursor = contextual;
                _currentPlant?.power.AoeState()?.SetPlanting(false);
                _lastContextualCursor = null;
            }
        }

        public void EndContextualCursor(CursorState contextual)
        {
            lock (this)
            {
                if (_contextualCursor != contextual) return;
                _currentPlant?.power.AoeState()?.SetPlanting(true);
                _lastContextualCursor = Time.time;
            }
        }

        public void SetPickedCursor(CursorState picked, [CanBeNull] string currentPlant, [CanBeNull] Texture2D seedBag)
        {
            lock (this)
            {
                _currentlyPicked = picked;
                _contextualCursor = null;
                _lastContextualCursor = null;

                _currentPlant?.power.AoeState()?.SetPlanting(false);

                if (currentPlant != null)
                {
                    _currentPlant = PlantManager.Instance.GetPlantData(currentPlant);
                    _currentPlant?.power.AoeState()?.SetPlanting(true);
                }
                else
                {
                    _currentPlant = null;
                }
                
                if (ShopUI.Instance.lastSelectedItem != null)
                {
                    ShopUI.Instance.lastSelectedItem.style.backgroundColor = FarmingFrenzyColors.ShopItemBackground;
                    ShopUI.Instance.lastSelectedItem = null;
                }
    
                _seedBagTexture = seedBag;
                if (_seedBagTexture)
                {
                    Cursor.SetCursor(_seedBagTexture, Vector2.zero, CursorMode.Auto);
                }
            }
        }

        private void ResetBackgrounds()
        {
            _defaultCursorBackground.color = _defaultCursorBackgroundColor;
            _shovelCursorBackground.color = _defaultCursorBackgroundColor;
            _scytheCursorBackground.color = _defaultCursorBackgroundColor;
        }

        private void SetupBackgrounds()
        {
            var elt = _currentlyPicked switch
            {
                CursorState.Default => _defaultCursorBackground,
                CursorState.Shovel => _shovelCursorBackground,
                CursorState.Scythe => _scytheCursorBackground,
                CursorState.Planting or CursorState.Spray => null,
                _ => throw new ArgumentOutOfRangeException()
            };

            if (elt != null) elt.color = _activeCursorBackgroundColor;
        }

        private void Update()
        {
            lock (this)
            {
                switch (CurrentlyActiveCursor)
                {
                    case CursorState.Default:
                        Cursor.SetCursor(_defaultCursor, Vector2.zero, CursorMode.Auto);
                        break;
                    case CursorState.Spray:
                        Cursor.SetCursor(_sprayBottleCursor, Vector2.zero, CursorMode.Auto);
                        break;
                    case CursorState.Shovel:
                        var hotspot = new Vector2(0, _shovelCursor.height);
                        Cursor.SetCursor(_shovelCursor, hotspot, CursorMode.Auto);
                        break;
                    case CursorState.Scythe:
                        Cursor.SetCursor(_scytheCursor, Vector2.zero, CursorMode.Auto);
                        break;
                    case CursorState.Planting:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                ResetBackgrounds();
                SetupBackgrounds();

                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    SetPickedCursor(CursorState.Default, null, null);
                } else if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    SetPickedCursor(CursorState.Shovel, null, null);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    SetPickedCursor(CursorState.Scythe, null, null);
                }

                var stopScythe = _contextualCursor == CursorState.Scythe && Time.time - lastHarvestablePlant > 0.1 &&
                                 !Input.GetKey(KeyCode.Mouse0);
                var stopContextualCursor = _lastContextualCursor != null && Time.time - _lastContextualCursor > 0.1;

                if (stopScythe || stopContextualCursor)
                {
                    _contextualCursor = null;
                    if (_currentlyPicked == CursorState.Planting)
                    {
                        Cursor.SetCursor(_seedBagTexture, Vector2.zero, CursorMode.Auto);
                    }
                }
            }
        }

        public void SprayParticles()
        {
            Instantiate(_sprayBottleParticleSystem, Camera.main!.ScreenToWorldPoint(Input.mousePosition), Quaternion.identity);
        }
        #endregion
    }
}
