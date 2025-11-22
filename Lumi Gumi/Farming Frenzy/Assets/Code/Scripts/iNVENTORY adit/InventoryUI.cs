using System.Collections.Generic;
using Code.Scripts.Managers; // Needs access to InventoryManager
using Code.Scripts.Player;
using TMPro;                 // For TextMeshPro
using UnityEngine;
using UnityEngine.UI;        // For standard UI (Images, Buttons)

namespace Code.Scripts.Menus
{
    public class InventoryUI : MonoBehaviour
    {
        public static InventoryUI Instance;

        [Header("UI References")]
        [SerializeField] private GameObject _inventoryWindow; // The Panel holding everything
        [SerializeField] private Transform _itemsContainer;   // The "Content" object inside a Scroll View
        [SerializeField] private GameObject _itemPrefab;      // The Prefab we will spawn for each plant

        private void Awake()
        {
            if (Instance == null) Instance = this;
        }

        private void Start()
        {
            // Start hidden
            SetHidden(true);
        }

        public void SetHidden(bool hidden)
        {
            if (_inventoryWindow != null)
            {
                _inventoryWindow.SetActive(!hidden);
                if (!hidden)
                {
                    RefreshInventoryUI();
                }
            }
        }

        public void RefreshInventoryUI()
        {
            // 1. Clear existing items to avoid duplicates
            foreach (Transform child in _itemsContainer)
            {
                Destroy(child.gameObject);
            }

            // 2. Get data
            var inventory = InventoryManager.Instance.GetInventory();

            // 3. Loop through logic
            foreach (var item in inventory)
            {
                string plantName = item.Key;
                int quantity = item.Value;

                // Load Data
                PlantData data = Resources.Load<PlantData>(plantName);

                if (data != null)
                {
                    SpawnInventoryItem(data, quantity);
                }
            }
        }

        private void SpawnInventoryItem(PlantData data, int quantity)
        {
            // Create the object
            GameObject newItem = Instantiate(_itemPrefab, _itemsContainer);

            // --- FIND AND SET COMPONENTS ---
            // NOTE: Ensure your Prefab has these components with these exact names or hierarchies!

            // 1. Set Icon
            Image icon = newItem.transform.Find("Icon").GetComponent<Image>();
            if (data._cursorSprite != null) icon.sprite = data._cursorSprite;

            // 2. Set Name
            TextMeshProUGUI nameText = newItem.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
            nameText.text = data.name;

            // 3. Set Count
            TextMeshProUGUI countText = newItem.transform.Find("CountText").GetComponent<TextMeshProUGUI>();
            countText.text = $"x{quantity}";

            // 4. Set Flavor Text
            TextMeshProUGUI flavorText = newItem.transform.Find("FlavorText").GetComponent<TextMeshProUGUI>();
            flavorText.text = string.IsNullOrEmpty(data.flavorText) ? "Tasty!" : data.flavorText;

            // 5. Setup Sell Button
            Button sellBtn = newItem.transform.Find("SellButton").GetComponent<Button>();
            TextMeshProUGUI btnText = sellBtn.GetComponentInChildren<TextMeshProUGUI>();
            btnText.text = $"Sell ({data._goldGenerated}G)";

            sellBtn.onClick.AddListener(() =>
            {
                // Call the manager
                InventoryManager.Instance.TrySellItem(data.name, data);
            });
        }
    }
}