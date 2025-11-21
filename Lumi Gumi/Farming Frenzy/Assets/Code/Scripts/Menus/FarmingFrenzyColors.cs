using System.Diagnostics.CodeAnalysis;
using Code.Scripts.Player;
using Unity.VisualScripting;
using UnityEngine;

namespace Code.Scripts.Menus
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")] // We want to keep these as public consts
    public static class FarmingFrenzyColors
    {
        public static Color PurchasableGold = new Color32(255, 205, 0, 255);
        public static Color TooExpensiveGold = new Color32(241, 107, 82, 255);
        public static Color DisabledOverlay = new(0, 0, 0, 0.5f);
        public static Color HighlightGreen = new Color32(28, 200, 11, 255);
        public static Color ShopItemBackground = new Color32(0, 0, 0, 64);

        public static Color PriceColor(int price) =>
            PlayerController.Instance.Money >= price ? PurchasableGold : TooExpensiveGold;

        public static string PriceRichText(int price) =>
            $"<color=#{PriceColor(price).ToHexString()}>{price}G</color>";
    }
}
