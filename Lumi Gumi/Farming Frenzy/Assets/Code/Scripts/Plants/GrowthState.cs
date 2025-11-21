using System;
using Code.Scripts.Menus;
using Unity.VisualScripting;
using UnityEngine;

namespace Code.Scripts.Plants
{
    public enum GrowthState
    {
        Seedling, Mature, Fruited, Harvested
    }
    
    namespace GrowthStateExtension
    {
        public static class GrowthStateExtensions
        {
            // ReSharper disable once MemberCanBePrivate.Global -- we want to keep this as a public API for future use
            public static Color Color(this GrowthState state) => state switch
            {
                GrowthState.Seedling => new Color32(96, 255, 72, 255),
                GrowthState.Mature or GrowthState.Harvested => new Color32(19, 140, 0, 255),
                GrowthState.Fruited => new Color32(255, 56, 131, 255),
                _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
            };

            // ReSharper disable once MemberCanBePrivate.Global -- we want to keep this as a public API for future use
            public static string NameRichText(this GrowthState state) =>
                $"<color=#{state.Color().ToHexString()}>{state}</color>";

            public static string StatusRichText(this GrowthState state, int timeToNext, int harvestGold)
            {
                // Don't show "will fruit" if the plant does not fruit
                var fruitingText = timeToNext > -1 ? $" Will <color=#{GrowthState.Fruited.Color().ToHexString()}>fruit</color> in {timeToNext}s." : "";
                return state switch
                {
                    GrowthState.Seedling => $"{GrowthState.Seedling.NameRichText()}. {GrowthState.Mature.NameRichText()} in {timeToNext}s.",
                    GrowthState.Mature or GrowthState.Harvested => $"{GrowthState.Mature.NameRichText()}.{fruitingText}",
                    GrowthState.Fruited => $"{GrowthState.Fruited.NameRichText()}! Left click to\n harvest it for" +
                                            $" <color=#{FarmingFrenzyColors.PurchasableGold.ToHexString()}>" +
                                                $"${harvestGold}" +
                                            "</color>",
                    _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
                };
            }
        }
    }
}
