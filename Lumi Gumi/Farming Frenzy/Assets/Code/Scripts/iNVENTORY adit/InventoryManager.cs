using System.Collections.Generic;
using UnityEngine;
using Code.Scripts.Player; // To access Money
using Code.Scripts.Menus;
namespace Code.Scripts.Managers
{
    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance;

        // Key = Plant Name (e.g., "Tomato"), Value = Quantity owned
        private Dictionary<string, int> _inventory = new Dictionary<string, int>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void AddItem(string plantName, int amount = 1)
        {
            if (_inventory.ContainsKey(plantName))
            {
                _inventory[plantName] += amount;
            }
            else
            {
                _inventory.Add(plantName, amount);
            }

            // Refresh UI whenever data changes
            if (InventoryUI.Instance != null) InventoryUI.Instance.RefreshInventoryUI();
        }

        public bool TrySellItem(string plantName, PlantData data)
        {
            if (_inventory.ContainsKey(plantName) && _inventory[plantName] > 0)
            {
                // 1. Remove item
                _inventory[plantName]--;
                if (_inventory[plantName] <= 0)
                {
                    _inventory.Remove(plantName);
                }

                // 2. Give Gold (Using the separate generated gold value)
                // Assuming PlayerController has an AddMoney or simple Money += logic
                PlayerController.Instance.Purchase(-data._goldGenerated); // Negative purchase = Add money? 
                // Or if you have specific method: PlayerController.Instance.AddMoney(data._goldGenerated);

                AudioManager.Instance.PlaySFX("kaching");

                // 3. Refresh UI
                if (InventoryUI.Instance != null) InventoryUI.Instance.RefreshInventoryUI();
                return true;
            }
            return false;
        }

        public Dictionary<string, int> GetInventory()
        {
            return _inventory;
        }
    }
}