using System;
using System.Linq;
using Code.Scripts.Enemy;
using Code.Scripts.GridSystem;
using Code.Scripts.Plants.GrowthStateExtension;
using Code.Scripts.Plants.Powers;
using Code.Scripts.Plants.Powers.PowerExtension;
using Code.Scripts.Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Code.Scripts.Managers;

namespace Code.Scripts.Plants
{
    public class Plant : MonoBehaviour
    {
        #region Properties
        private PlantData _data;
        private GridTile _tile;
        private Sprite _currentSprite;
        private float _secsSinceGrowth;
        private GrowthState _state;
        private SpriteRenderer _plantSpriteRenderer;
        private int _growthSpriteIndex;
        private bool _readyToHarvest;
        public BoxCollider2D Collider { get; private set; }
        public BoxCollider2D aoeCollider;
        private float _healRate;
        private float _fruitingRate;
        private float _health;
        private float _nextHealTime;
        private float MaxHealth => _data._health;
        private GameObject _healthBar;
        private Slider hBarController;
        [SerializeField] PlayerController _playerController;

        // --- NEW: Yield Tracking ---
        private int _currentYield = 1;
        // ---------------------------

        private int SecsToNextStage
        {
            get
            {
                return _state switch
                {
                    GrowthState.Seedling => Math.Max(0, (int)(_data._maturationCycle - _secsSinceGrowth)),
                    GrowthState.Mature => _data._cannotHarvest ? -1 : Math.Max(0, (int)((_data._fruitingCycle - _secsSinceGrowth) / _fruitingRate)),
                    // Show yield progress if already fruited but not full
                    GrowthState.Fruited => _currentYield < _data._maxYield ? Math.Max(0, (int)(_data._yieldGenerationRate - _secsSinceGrowth)) : 0,
                    _ => 0
                };
            }
        }

        public PowerKind PowerKind => _data.power;

        private bool CanHarvestNow => !_data._cannotHarvest && _state == GrowthState.Fruited;

        public string PlantName => _data.name;

        // Updated Status Text to show stack count
        public string StatusRichText => _state == GrowthState.Fruited
            ? $"Ready to harvest! <color=yellow>${_data._goldGenerated}\n({SecsToNextStage}s until next yield)\nYield: {_currentYield}/{_data._maxYield}"
            : _state.StatusRichText(SecsToNextStage, _data._goldGenerated);

        public delegate void HoverInEvent(Plant plant);

        public delegate void HoverOutEvent(Plant plant);
        public event HoverInEvent OnHoverIn;
        public event HoverOutEvent OnHoverOut;
        private bool _isMouseOverPlant;
        #endregion

        #region Methods
        private void Awake()
        {
            _plantSpriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            _playerController = FindFirstObjectByType<PlayerController>();
            transform.position = new(transform.position.x, transform.position.y, -0.5f);
            transform.rotation = Quaternion.Euler(-20f, 0f, 0f);
        }

        private void Update()
        {
            UpdateState();

            if (!_isMouseOverPlant) return;

            PlayerController.CursorState cursor;
            bool isContextual;
            lock (PlayerController.Instance)
            {
                cursor = PlayerController.Instance.CurrentlyActiveCursor;

                if (CanHarvestNow && cursor != PlayerController.CursorState.Shovel)
                {
                    PlayerController.Instance.lastHarvestablePlant = Time.time;
                    PlayerController.Instance.StartContextualCursor(PlayerController.CursorState.Scythe);
                }

                isContextual = PlayerController.Instance.IsContextualActive;
            }

            if (!Input.GetMouseButton(0)) return;

            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            var hit = Physics2D.GetRayIntersection(ray, 1500f);

            if (hit.transform?.gameObject != gameObject)
            {
                _isMouseOverPlant = false;
                return;
            }

            switch (cursor)
            {
                case PlayerController.CursorState.Scythe:
                    if (EventSystem.current.IsPointerOverGameObject()) return;

                    if (CanHarvestNow)
                    {
                        AudioManager.Instance.PlaySFX("picking");
                        Harvest();
                    }

                    Kill();
                    break;

                case PlayerController.CursorState.Shovel when !isContextual:
                    DigPlant();
                    break;

                case PlayerController.CursorState.Default:
                case PlayerController.CursorState.Spray:
                case PlayerController.CursorState.Planting:
                default:
                    break;
            }
        }

        public void InitPlant(PlantData pdata, GridTile tile, GameObject healthBar)
        {
            lock (tile)
            {
                _data = pdata;
                _state = GrowthState.Seedling;
                _secsSinceGrowth = 0.0f;
                _currentYield = 1; // Reset yield
                Collider = GetComponent<BoxCollider2D>();
                aoeCollider = transform.GetChild(0).GetComponent<BoxCollider2D>();
                _health = pdata._health;
                _healthBar = healthBar.transform.Find("Canvas").Find("Slider").gameObject;
                hBarController = healthBar.transform.Find("Canvas").Find("Slider").gameObject.GetComponent<Slider>();
                _healthBar.SetActive(false);
                if (_data._isTree)
                {
                    Collider.size = new Vector2(3, 2);
                    Collider.offset = new Vector2(0, 0.5f);
                    aoeCollider.size *= new Vector2(3, 2);
                    aoeCollider.offset *= new Vector2(0, 0.5f);
                }

                _tile = tile;
                _tile.HasPlant = true;

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.OnPlantPlanted(_data.name);
                }

                print($"Just placed a {PlantName} on {_tile.name} (hasPlant = {_tile.HasPlant})");

                _plantSpriteRenderer.sprite = _data._maturationSprite.First();
                GetComponent<SpriteRenderer>().sortingOrder = 5000 - Mathf.CeilToInt(gameObject.transform.position.y);
            }
        }

        private void UpdateState()
        {
            if (Time.time >= _nextHealTime)
            {
                _healRate = LegumePower.CalculateGrowthModifier(aoeCollider);
                _fruitingRate = -1f;
                _fruitingRate += PlantName == "Corn" ? CornPower.CalculateCornFruitingModifier(aoeCollider) : 1.0f;
                _fruitingRate += BananaPower.CalculateFruitingModifier(aoeCollider);

                _nextHealTime = Time.time + 2.0f;

                _health = Math.Min(MaxHealth, _health + 2.0f * _healRate);
                hBarController.value = _health / MaxHealth;
                if (hBarController.value == hBarController.maxValue)
                {
                    _healthBar.SetActive(false);
                }
            }

            switch (_state)
            {
                case GrowthState.Seedling:
                    _secsSinceGrowth += Time.deltaTime;

                    if (_secsSinceGrowth >= _data._maturationCycle)
                    {
                        _state = GrowthState.Mature;
                        _plantSpriteRenderer.sprite = _data._growthSprite[0];
                        _data.power.AddTo(gameObject);
                        _secsSinceGrowth = 0;
                    }
                    else
                    {
                        var spriteIndex = Mathf.FloorToInt(_secsSinceGrowth * _data._maturationSprite.Length / _data._maturationCycle);
                        _plantSpriteRenderer.sprite = _data._maturationSprite[spriteIndex];
                    }
                    break;

                case GrowthState.Mature:
                    if (_data._cannotHarvest) { break; }

                    _secsSinceGrowth += Time.deltaTime * _fruitingRate;

                    if (_secsSinceGrowth <= _data._fruitingCycle)
                    {
                        var spriteIndex = Mathf.FloorToInt(_secsSinceGrowth * _data._growthSprite.Length / _data._fruitingCycle);
                        if (spriteIndex > 0) { _plantSpriteRenderer.sprite = _data._growthSprite[spriteIndex - 1]; }
                    }
                    else
                    {
                        _plantSpriteRenderer.sprite = _data._growthSprite[^1];
                        _state = GrowthState.Fruited;
                        _currentYield = 1; // First fruit arrived
                        _secsSinceGrowth = 0;
                    }
                    break;

                // --- NEW: Stacking Logic ---
                case GrowthState.Fruited:
                    // Only run timer if we haven't hit max stack yet
                    if (_currentYield < _data._maxYield)
                    {
                        _secsSinceGrowth += Time.deltaTime; // Standard rate for stacking
                        if (_secsSinceGrowth >= _data._yieldGenerationRate)
                        {
                            _currentYield++;
                            _secsSinceGrowth = 0; // Reset for next fruit
                            // Optional: Visual feedback (Shake?)
                            // transform.DOShake... (if using DOTween)
                        }
                    }
                    break;
                // ---------------------------

                case GrowthState.Harvested:
                    _state = GrowthState.Mature;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Harvest()
        {
            // --- NEW: Add accumulated yield ---
            InventoryManager.Instance.AddItem(_data.name, _currentYield);

            if (GameManager.Instance != null)
            {
                // Trigger objective for EACH item harvested? 
                // Currently triggering once per harvest action, but adding loop to be safe:
                for (int i = 0; i < _currentYield; i++)
                {
                    GameManager.Instance.OnPlantHarvested(_data.name);
                }
            }
            // ----------------------------------

            _playerController.SpawnCoinSpark(transform.position, Quaternion.identity);

            // Show total harvested
            GameManager.Instance.ShowFloatingText($"+{_currentYield} {_data.name}");
        }

        private void DigPlant()
        {
            AudioManager.Instance.PlaySFX("digMaybe");
            PlayerController.Instance.IncreaseMoney(_data._price / 2);
            Kill();
        }

        private void OnMouseEnter()
        {
            _isMouseOverPlant = true;
            OnHoverIn?.Invoke(this);
        }

        private void OnMouseExit()
        {
            _isMouseOverPlant = false;
            OnHoverOut?.Invoke(this);
        }

        private void OnMouseDown()
        {
            _isMouseOverPlant = true;
        }

        private void OnMouseDrag()
        {
            _isMouseOverPlant = true;
        }

        private void Kill()
        {
            lock (_tile)
            {
                _tile.HasPlant = false;
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (_isMouseOverPlant)
            {
                PlayerController.Instance.EndContextualCursor(PlayerController.CursorState.Scythe);
            }
        }

        public bool TakeDamage(float amount)
        {
            _healthBar.SetActive(true);
            _health -= amount;
            print($"{PlantName} took {amount} damage! HP = {_health}");
            hBarController.value = _health / MaxHealth;

            if (_health > 0) return false;

            Kill();
            return true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Enemy")) return;

            var enemy = other.GetComponentInParent<EnemyAgent>();
            if (!enemy.CanAttack) return;

            enemy.StartAttacking(this);
        }
        #endregion
    }
}