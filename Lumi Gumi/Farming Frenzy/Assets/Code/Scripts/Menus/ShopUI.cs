using System.Collections.Generic;
using System.Linq;
using Code.GrowthRateExtension;
using Code.Scripts.GridSystem;
using Code.Scripts.Plants.Powers.PowerExtension;
using Code.Scripts.Player;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UIElements;

namespace Code.Scripts.Menus
{
    public class ShopUI : MonoBehaviour
    {
        private VisualTreeAsset _itemTemplate;
        private VisualTreeAsset _itemTooltipTemplate;
        private ShopContainerTooltipManipulator _tooltipManipulator;
        private VisualElement _root;
        public bool MouseInShop => _tooltipManipulator.MouseInShop;

        private static ShopUI _instance;
        public static ShopUI Instance => _instance ??= GameObject.Find("Shop").GetComponent<ShopUI>();
        [CanBeNull] public VisualElement lastSelectedItem;

        private void Start()
        {
            InitShop();
        }

        public void SetHidden(bool hidden)
        {
            _root.style.display = hidden ? DisplayStyle.None : DisplayStyle.Flex;
        }

        public void InitShop()
        {
            _root = ((UIDocument)gameObject.GetComponent(typeof(UIDocument))).rootVisualElement;
            _itemTemplate = Resources.Load<VisualTreeAsset>("Shop_item");
            _itemTooltipTemplate = Resources.Load<VisualTreeAsset>("Shop_item_tooltip");
            _tooltipManipulator = new ShopContainerTooltipManipulator();
            _root.AddManipulator(_tooltipManipulator);

            var defaultSprite = Resources.Load<Sprite>("Placeholder");
            
            var plants = new List<string>
            {
                // Tier 1
                "Tomato", "Corn", "Clover", "Blister Berry",
                
                // Tier 2
                "Pumpkin", "Banana", "Beans", "Shrub Rose", // "Scarecrow", "Marigold",

                // Tier 3
                "Cauli", "Chili",
            };

            foreach (var plant in plants)
            {
                AddShopItem(Resources.Load<PlantData>(plant), defaultSprite);
            }

            Resources.UnloadUnusedAssets();
            PlayerController.Instance.RefreshMoney();
        }

        private void AddShopItem(PlantData data, Sprite defaultSprite)
        {
            // Set up item template
            VisualElement ui = _itemTemplate.Instantiate();
            var sprite = data._growthSprite?.LastOrDefault() ?? defaultSprite;
            ui.Q("plant_icon").style.backgroundImage = new StyleBackground(sprite);
            ui.Q("role_icon").style.backgroundImage = new StyleBackground(data._roleIcon);
            ui.Q<Label>("plant_name").text = data.name;

            var shopEntryPrice = ui.Q<Label>("price");
            shopEntryPrice.text = $"{data._price}G";

            ui.RegisterCallback<ClickEvent>(_ =>
            {
                GridManager.Instance.SetActivePlant(data.name);

                if (lastSelectedItem != null)
                {
                    lastSelectedItem.style.backgroundColor = FarmingFrenzyColors.ShopItemBackground;
                }
    
                print($"Clicked on a {data.name}");
                lastSelectedItem = ui;
                ui.style.backgroundColor = FarmingFrenzyColors.HighlightGreen;
            });

            // Set up tooltip
            VisualElement tooltip = _itemTooltipTemplate.Instantiate();
            tooltip.Q<Label>("plant_name").text = data.name;
            var tooltipPrice = tooltip.Q<Label>("cost");
            tooltipPrice.text = $"{data._price}G";
            tooltip.Q<Label>("yield").text = $"{data._goldGenerated}G";

            var flavour = tooltip.Q<Label>("flavour");
            if (!string.IsNullOrEmpty(data.flavorText))
            {
                flavour.text = data.flavorText;
            }
            else
            {
                flavour.style.display = DisplayStyle.None;
            }

            var powerLabel = tooltip.Q<Label>("power");
            powerLabel.enableRichText = true;

            var powerText = data.power.Text();
            powerLabel.text = !string.IsNullOrEmpty(powerText) ? $"<u>{powerText}</u>" : "";

            var growth = tooltip.Q<Label>("growth");
            growth.text = data.GrowthRateBand.Text();
            growth.style.color = data.GrowthRateBand.Color();

            var fruiting = tooltip.Q<Label>("fruiting");
            fruiting.text = data.FruitingRateBand.Text();
            fruiting.style.color = data.FruitingRateBand.Color();

            // Add tooltip to item, and item to list
            ui.AddManipulator(new ShopItemTooltipManipulator(_tooltipManipulator, tooltip, data.power));
            _root.Q("shop_list").Add(ui);

            var overlay = ui.Q<VisualElement>("overlay");
            var tooExpensive = tooltip.Q<VisualElement>("too_expensive");

            // Add event listeners
            PlayerController.Instance.OnMoneyChange += amount =>
            {
                var color = FarmingFrenzyColors.PriceColor(data._price);
                shopEntryPrice.style.color = color;
                tooltipPrice.style.color = color;

                if (data._price > amount)
                {
                    overlay.style.backgroundColor = FarmingFrenzyColors.DisabledOverlay;
                    tooExpensive.style.display = DisplayStyle.Flex;
                }
                else
                {
                    var color2 = Color.white;
                    color2.a = 0;
                    overlay.style.backgroundColor = color2;
                    tooExpensive.style.display = DisplayStyle.None;
                }
            };
        }
    }
}
