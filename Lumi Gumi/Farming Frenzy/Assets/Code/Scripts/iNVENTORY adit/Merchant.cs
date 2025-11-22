using Code.Scripts.Menus;
using UnityEngine;

namespace Code.Scripts.NPCs
{
    public class Merchant : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private Color _hoverColor = Color.yellow;
        private Color _originalColor;
        private SpriteRenderer _renderer;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _originalColor = _renderer.color;
        }

        // Detect when mouse is over the Merchant
        private void OnMouseEnter()
        {
            // Visual feedback (Highlight)
            _renderer.color = _hoverColor;
        }

        // Detect when mouse leaves
        private void OnMouseExit()
        {
            // Remove Highlight
            _renderer.color = _originalColor;
        }

        // Detect Click (The "Touch")
        private void OnMouseDown()
        {
            // Open the UI
            MerchantUI.Instance.OpenShop();
        }
    }
}