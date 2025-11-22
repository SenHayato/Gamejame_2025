using Code.Scripts.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Scripts.Menus
{
    public class MerchantUI : MonoBehaviour
    {
        public static MerchantUI Instance;

        [Header("UI References")]
        [SerializeField] private GameObject _merchantCanvas;  // The whole UI Panel
        [SerializeField] private Transform _itemsContainer;   // The Content of the ScrollView
        [SerializeField] private GameObject _itemPrefab;      // The Row Prefab
        [SerializeField] private Button _closeButton;         // The "X" Button

        private void Awake()
        {
            if (Instance == null) Instance = this;
        }

        private void Start()
        {
            // 1. Setup the Close Button listener
            if (_closeButton != null)
            {
                _closeButton.onClick.AddListener(CloseShop);
            }

            // 2. Start hidden
            CloseShop();
        }

        public void OpenShop()
        {
            _merchantCanvas.SetActive(true);
            RefreshMerchantUI();

            // Optional: Pause game while shopping?
            // Time.timeScale = 0f; 
        }

        public void CloseShop()
        {
            _merchantCanvas.SetActive(false);

            // Optional: Resume game
            // Time.timeScale = 1f;
        }

        public void RefreshMerchantUI()
        {
            // 1. Clear old items
            foreach (Transform child in _itemsContainer)
            {
                Destroy(child.gameObject);
            }

            // 2. Get Inventory Data
            var inventory = InventoryManager.Instance.GetInventory();

            // 3. Spawn Items
            foreach (var item in inventory)
            {
                string plantName = item.Key;
                int quantity = item.Value;

                PlantData data = Resources.Load<PlantData>(plantName);

                if (data != null)
                {
                    SpawnItemRow(data, quantity);
                }
            }
        }

        private void SpawnItemRow(PlantData data, int quantity)
        {
            GameObject newItem = Instantiate(_itemPrefab, _itemsContainer);

            // --- Set Visuals ---
            // Icon
            Image icon = newItem.transform.Find("Icon").GetComponent<Image>();
            if (data._cursorSprite != null) icon.sprite = data._cursorSprite;

            // Count
            TextMeshProUGUI countText = newItem.transform.Find("CountText").GetComponent<TextMeshProUGUI>();
            countText.text = $"x{quantity}";

            // Sell Button
            Button sellBtn = newItem.transform.Find("SellButton").GetComponent<Button>();
            TextMeshProUGUI btnText = sellBtn.GetComponentInChildren<TextMeshProUGUI>();
            btnText.text = $"Sell ({data._goldGenerated}G)";

            sellBtn.onClick.AddListener(() =>
            {
                // Sell the item
                bool sold = InventoryManager.Instance.TrySellItem(data.name, data);

                // If sold successfully, refresh the list immediately so the count updates
                if (sold)
                {
                    RefreshMerchantUI();
                }
            });
        }
    }
}