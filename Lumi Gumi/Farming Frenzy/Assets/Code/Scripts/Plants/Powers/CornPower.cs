using System;
using System.Collections.Generic;
using System.Linq;
using Code.Scripts.Managers;
using Unity.VisualScripting;
using UnityEngine;

namespace Code.Scripts.Plants.Powers
{
    public class CornPower : Power
    {
        private const float Distance = 8f;
        public const int EffectPercent = 5;
        public const int MaxEffectPercent = 50;
        public SpriteRenderer[] Sprites;

        private void Awake()
        {
            var colliders = gameObject.GetComponents<BoxCollider2D>();
            var verticalCollider = colliders[0];
            var horizontalCollider = colliders[1];
            verticalCollider.size = new Vector2(Distance - 0.1f, 0.9f);
            horizontalCollider.size = new Vector2(0.9f, Distance - 0.1f);

            Sprites = gameObject.GetComponentsInChildren<SpriteRenderer>();
            var verticalSprite = Sprites[0];
            var horizontalSprite = Sprites[1];
            verticalSprite.size = new Vector2(1.0f, Distance);
            verticalSprite.sortingOrder = 1001;
            horizontalSprite.size = new Vector2(Distance, 1.0f);
            horizontalSprite.sortingOrder = 1001;
        }

        private void Start()
        {
            Sprites[0].enabled = PlantManager.Instance.CornPowerAoe.Visible;
            Sprites[1].enabled = PlantManager.Instance.CornPowerAoe.Visible;
            PlantManager.Instance.CornPowerAoe.OnVisibilityChange += VisibilityChange;
        }

        private void VisibilityChange(bool visible)
        {
            foreach (var spriteRenderer in Sprites)
            {
                spriteRenderer.enabled = visible;
            }
        }

        private void OnDestroy()
        {
            PlantManager.Instance.CornPowerAoe.OnVisibilityChange -= VisibilityChange;
        }

        /// <summary>
        ///  Calculate the corn fruiting modifier for a given place (either tile or plant)
        /// </summary>
        /// <param name="place"></param>
        /// <returns></returns>
        public static float CalculateCornFruitingModifier(Collider2D place)
        {
            var collisions = new List<Collider2D>();
            
            place.Overlap(collisions);
 
            var numberCorn = collisions
                .Select(other => other.gameObject.GetComponent<CornPower>())
                .NotNull()
                .Distinct()
                .Count();

            return 1.0f + Math.Min(MaxEffectPercent, numberCorn * EffectPercent) / 100.0f;
        }
    }
}