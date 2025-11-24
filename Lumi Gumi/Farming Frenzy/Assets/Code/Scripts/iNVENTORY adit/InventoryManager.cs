using System.Collections.Generic;
using UnityEngine;
using Code.Scripts.Player;
using Code.Scripts.Menus;

namespace Code.Scripts.Managers
{
    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance;

        private Dictionary<string, int> _inventory = new Dictionary<string, int>();

        // --- NEW: Track total harvested for the end screen ---
        public int TotalHarvestedCount { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void AddItem(string plantName, int amount = 1)
        {
            // --- NEW: Increment total stats ---
            TotalHarvestedCount += amount;

            if (_inventory.ContainsKey(plantName))
            {
                _inventory[plantName] += amount;
            }
            else
            {
                _inventory.Add(plantName, amount);
            }

            if (InventoryUI.Instance != null) InventoryUI.Instance.RefreshInventoryUI();
        }

        // ... rest of your TrySellItem and GetInventory methods remain the same ...
        public bool TrySellItem(string plantName, PlantData data)
        {
            if (_inventory.ContainsKey(plantName) && _inventory[plantName] > 0)
            {
                _inventory[plantName]--;
                if (_inventory[plantName] <= 0)
                {
                    _inventory.Remove(plantName);
                }
                PlayerController.Instance.Purchase(-data._goldGenerated);
                AudioManager.Instance.PlaySFX("Coins");
                if (InventoryUI.Instance != null) InventoryUI.Instance.RefreshInventoryUI();
                return true;
            }
            return false;
        }

        public Dictionary<string, int> GetInventory() { return _inventory; }
    }
}