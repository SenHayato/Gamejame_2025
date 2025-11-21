using System;
using Code.Scripts.Menus;
using Code.Scripts.Plants;
using Code.Scripts.Plants.Powers;
using Code.Scripts.Plants.Powers.PowerExtension;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UIElements;

namespace Code.Scripts.GridSystem
{
    public class GridTooltipManager : MonoBehaviour
    {
        private float _startedHoverTime;
        private VisualElement _root;
        private VisualElement _tooltip;
        private ShopUI _shopUI;
        [SerializeField] private Canvas _canvas;
        [CanBeNull] private Plant _currentPlant;
        [CanBeNull] private GridTile _currentTile;
        [CanBeNull] private Obstacle _currentObstacle;
        [CanBeNull] private string _currentObstacleType;
        private bool _hasSetAoesVisible;
        private float _lastRebuild;
        private float _prevSizeY;

        public void Start()
        {
            _root = gameObject.GetComponent<UIDocument>().rootVisualElement;
            _root.visible = false;
            _startedHoverTime = float.PositiveInfinity;
            _tooltip = _root.Q<VisualElement>("tooltip");
            _root.RegisterCallback<MouseMoveEvent>(_ => UpdatePosition());
        }

        private void UpdatePosition()
        {
            if (!Mathf.Approximately(_prevSizeY, _canvas.renderingDisplaySize.y))
            {
                _prevSizeY = _canvas.renderingDisplaySize.y;
                foreach (var elt in _tooltip.Children())
                {
                    elt.style.fontSize = 25 / (1080 / _canvas.renderingDisplaySize.y);
                }
                _tooltip.Q<Label>("name").style.fontSize = 28 / (1080 / _canvas.renderingDisplaySize.y);
            }

            _tooltip.style.top = _canvas.renderingDisplaySize.y - Input.mousePosition.y + 25;
            _tooltip.style.left = Math.Max(0, Input.mousePosition.x - _tooltip.resolvedStyle.width);
        }

        public void SubscribeTileEvents(GridTile spawnedTile)
        {
            spawnedTile.OnTileHoverIn += HandleTileHoverIn;
            spawnedTile.OnTileHoverOut += _ => HandleHoverOut();
            // This is OK to just do directly as if we purchase it, we must be mousing over
            spawnedTile.OnTileClicked += _ => RebuildTooltip();
        }
        
        public void SubscribeObstacleEvents(Obstacle obstacle, string type)
        {
            obstacle.OnObstacleHoverIn += obs => HandleObstacleHoverIn(obs, type);
            obstacle.OnObstacleHoverOut += _ => HandleHoverOut();
        }
        
        public void SubscribePlantEvents(Plant plant)
        {
            plant.OnHoverIn += HandlePlantHoverIn;
            plant.OnHoverOut += _ => HandleHoverOut();
        }

        private void HandleTileHoverIn(GridTile tile)
        {
            BuildTileTooltip(tile);
            _currentTile = tile;
            _startedHoverTime = Time.time;
        }

        private void RebuildTooltip()
        {
            _lastRebuild = Time.time;
            if (_currentObstacle) BuildObstacleTooltip(_currentObstacle, _currentObstacleType);
            else if (_currentTile) BuildTileTooltip(_currentTile);
            else if (_currentPlant) BuildPlantTooltip(_currentPlant);
        }

        private void AddTileModifiers(Collider2D thing) 
        {
            AddCornModifier(thing);
            AddLegumeModifier(thing);
            AddBananaModifier(thing);
            AddChiliModifier(thing);
        }

        private void AddLegumeModifier(Collider2D thing)
        {
            var growthModifier = LegumePower.CalculateGrowthModifier(thing);
            var growthPercent = (int) Math.Round(growthModifier * 100.0f);
            if (growthPercent > 100)
            {
                _root.Q<Label>("legume_modifier").text = $"Healing speed: <b>+{growthPercent - 100}%</b>";
                _root.Q<Label>("legume_modifier").style.display = DisplayStyle.Flex;
            }
            else
            {
                _root.Q<Label>("legume_modifier").style.display = DisplayStyle.None;
            }
        }

        private void AddChiliModifier(Collider2D thing)
        {
            var damageMod = ChiliPower.CalculateDamageModifier(thing);
            var dmgPct = (int) Math.Round(damageMod * 100.0f);
            if (dmgPct > 100)
            {
                _root.Q<Label>("chili_modifier").text = $"Defensive plant damage: <b>+{dmgPct - 100}%</b>";
                _root.Q<Label>("chili_modifier").style.display = DisplayStyle.Flex;
            }
            else
            {
                _root.Q<Label>("chili_modifier").style.display = DisplayStyle.None;
            }
        }

        private void AddCornModifier(Collider2D thing)
        {
            var cornModifier = CornPower.CalculateCornFruitingModifier(thing);
            var cornPercent = (int) Math.Round(cornModifier * 100.0f);
            if (cornPercent > 100)
            {
                _root.Q<Label>("corn_modifier").text = $"Corn fruits an extra <b>+{cornPercent - 100}% faster here</b>";
                _root.Q<Label>("corn_modifier").style.display = DisplayStyle.Flex;
            }
            else
            {
                _root.Q<Label>("corn_modifier").style.display = DisplayStyle.None;
            }
        }

        private void AddBananaModifier(Collider2D thing)
        {
            var modifier = BananaPower.CalculateFruitingModifier(thing);
            var percent = (int) Math.Round(modifier * 100.0f);
            if (percent > 100)
            {
                _root.Q<Label>("banana_modifier").text = $"Plants fruit <b>+{percent - 100}% faster here</b>";
                _root.Q<Label>("banana_modifier").style.display = DisplayStyle.Flex;
            }
            else
            {
                _root.Q<Label>("banana_modifier").style.display = DisplayStyle.None;
            }
        }

        private void BuildTileTooltip([NotNull] GridTile tile)
        {
            AddTileModifiers(tile.Collider);
            if (tile.IsPurchased)
            {
                BuildFarmlandTooltip(tile);
            }
            else
            {
                BuildGrassTooltip(tile);
            }

            _root.Q<Label>("power").style.display = DisplayStyle.None;
        }

        private void BuildGrassTooltip([NotNull] GridTile tile)
        {
            _root.Q<Label>("name").text = "Grass";
            _root.Q<Label>("status").text =
                $"Click to buy and till. Cost: {FarmingFrenzyColors.PriceRichText(tile.Cost * 4)}";
        }

        private void BuildFarmlandTooltip([NotNull] GridTile _)
        {
            _root.Q<Label>("name").text = "Farmland";
            _root.Q<Label>("status").text = "Plant here by selecting a plant\nin the shop and then clicking here.";
        }

        private void HandleObstacleHoverIn(Obstacle obstacle, string type)
        {
            _currentObstacle = obstacle;
            _currentObstacleType = type;
            BuildObstacleTooltip(obstacle, type);
            _startedHoverTime = Time.time;
        }

        private void BuildObstacleTooltip([NotNull] Obstacle obstacle, string type)
        {
            AddTileModifiers(obstacle.Collider);
            _root.Q<Label>("name").text = type;
            _root.Q<Label>("status").text = $"Click to buy and remove. Cost: {FarmingFrenzyColors.PriceRichText(obstacle.Cost)}";
            _root.Q<Label>("power").style.display = DisplayStyle.None;
        }

        private void HandlePlantHoverIn(Plant plant)
        {
            _startedHoverTime = Time.time;
            _currentPlant = plant;
            BuildPlantTooltip(plant);
        }
        
        private void BuildPlantTooltip([NotNull] Plant plant)
        {
            _hasSetAoesVisible = false;
            AddTileModifiers(plant.Collider);
            _root.Q<Label>("name").text = plant.PlantName;
            _root.Q<Label>("status").text = plant.StatusRichText;
            _root.Q<Label>("power").style.display = DisplayStyle.None;
        }

        private void HandleHoverOut()
        {
            _currentPlant?.PowerKind.AoeState()?.SetHovering(false);
            _root.visible = false;
            _startedHoverTime = float.PositiveInfinity;
            _currentPlant = null;
            _currentObstacle = null;
            _currentTile = null;
        }

        public void Update()
        {
            if (!(Time.time - _startedHoverTime > 0.2))
            {
                return;
            }

            if (ShopUI.Instance.MouseInShop)
            {
                HandleHoverOut();
                return;
            }

            if (!_hasSetAoesVisible)
            {
                _currentPlant?.PowerKind.AoeState()?.SetHovering(true);
                _hasSetAoesVisible = true;
            }

            if (Time.time - _lastRebuild > 1.0f)
            {
                RebuildTooltip();
            }

            UpdatePosition();
            _root.visible = true;
        }
    }
}
