using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Code
{
    /// <summary>
    ///  Represents the growth or fruiting rate of a crop.
    /// </summary>
    public enum GrowthRate
    {
        Slow = 1,
        Medium = 2,
        Fast = 3,
    }

    namespace GrowthRateExtension
    {
        public static class GrowthRateExtensions
        {
            /// <summary>
            /// Format the growth rate as a text item for use in tooltips
            /// </summary>
            /// <param name="rate"></param>
            /// <returns></returns>
            public static string Text(this GrowthRate rate)
            {
                return new string('>', (int)rate);
            }

            /// <summary>
            /// Get the text colour that this growth rate should show up as in tooltips
            /// </summary>
            /// <param name="rate"></param>
            /// <returns></returns>
            /// <exception cref="ArgumentOutOfRangeException"></exception>
            public static StyleColor Color(this GrowthRate rate)
            {
                return rate switch
                {
                    GrowthRate.Slow => new StyleColor(new Color(229f / 255f, 40f / 255f, 24f / 255f, 1)),
                    GrowthRate.Medium => new StyleColor(new Color(246f / 255f, 166f / 255f, 75f / 255f, 1)),
                    GrowthRate.Fast => new StyleColor(new Color(65f / 255f, 1f, 0f, 1f)),
                    _ => throw new ArgumentOutOfRangeException(nameof(rate), rate, null)
                };
            }
        }
    }
}
