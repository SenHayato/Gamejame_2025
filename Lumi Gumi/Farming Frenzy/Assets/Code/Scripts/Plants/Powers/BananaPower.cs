using System;
using System.Collections.Generic;
using System.Linq;
using Code.Scripts.Managers;
using Unity.VisualScripting;
using UnityEngine;

namespace Code.Scripts.Plants.Powers
{
    public class BananaPower : Power
    {
        private const float Radius = 10.0f;
        public const int EffectPercent = 10;
        public const int MaxEffectPercent = 40;
        
        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            GetComponent<CircleCollider2D>().radius = 0.5f;
            transform.localScale = new Vector3(Radius - 0.1f, Radius - 0.1f, 1.0f);
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _spriteRenderer.sortingOrder = 1000;
        }

        private void Start()
        {
            _spriteRenderer.enabled = PlantManager.Instance.BananaPowerAoe.Visible;
            PlantManager.Instance.BananaPowerAoe.OnVisibilityChange += VisibilityChange;
        }

        private void VisibilityChange(bool visible)
        {
            _spriteRenderer.enabled = visible;
        }

        private void OnDestroy()
        {
            PlantManager.Instance.BananaPowerAoe.OnVisibilityChange -= VisibilityChange;
        }

        public static float CalculateFruitingModifier(Collider2D thing)
        {
            var collisions = new List<Collider2D>();
            thing.Overlap(collisions);

            var numBanana = collisions
                .Select(other => other.gameObject.GetComponent<BananaPower>())
                .NotNull()
                .Count();

            return 1.0f + Math.Min(MaxEffectPercent, numBanana * EffectPercent) / 100.0f;
        }
    }
}