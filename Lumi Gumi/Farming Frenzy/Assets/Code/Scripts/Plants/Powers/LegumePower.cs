using System;
using System.Collections.Generic;
using System.Linq;
using Code.Scripts.Managers;
using Code.Scripts.Player;
using Unity.VisualScripting;
using UnityEngine;

namespace Code.Scripts.Plants.Powers
{
    public abstract class LegumePower : Power
    {
        protected abstract float Radius { get; }
        protected abstract float EffectStrength { get; }
        public const int MaxEffectPercent = 50;
        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            GetComponent<CircleCollider2D>().radius = 0.5f;
            transform.localScale = new Vector3(Radius - 0.1f, Radius - 0.1f, 1.0f);
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _spriteRenderer.sortingOrder = 1000; // EBA7CD
        }

        private void Start()
        {
            _spriteRenderer.enabled = PlantManager.Instance.LegumePowerAoe.Visible;
            PlantManager.Instance.LegumePowerAoe.OnVisibilityChange += VisibilityChange;
        }

        private void VisibilityChange(bool visible)
        {
            _spriteRenderer.enabled = visible;
        }
        
        private void OnDestroy()
        {
            PlantManager.Instance.LegumePowerAoe.OnVisibilityChange -= VisibilityChange;
        }

        /// <summary>
        ///  Calculate the growth modifier for a given place (either tile or plant)
        /// </summary>
        /// <param name="place"></param>
        /// <returns></returns>
        public static float CalculateGrowthModifier(Collider2D place)
        {
            var collisions = new List<Collider2D>();
            place.Overlap(collisions);

            var rate = collisions
                .Select(other => other.gameObject.GetComponent<LegumePower>())
                .NotNull()
                .Select(legume => legume.EffectStrength)
                .Sum();

            return Math.Min(1.0f + MaxEffectPercent / 100.0f, 1.0f + rate);
        }
    }
}
