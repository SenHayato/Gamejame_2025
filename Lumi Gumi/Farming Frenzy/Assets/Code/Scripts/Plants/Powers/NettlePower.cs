using UnityEngine;

namespace Code.Scripts.Plants.Powers
{
    public class NettlePower : SpikyPower
    {
        private float _lastUpdate;
        private const int BaseDamage = 5;
        private float _damageMultiplier = 1.0f;
        public override float Damage => _damageMultiplier * BaseDamage;
        private Collider2D _plantCollider;

        private void Awake()
        {
            _lastUpdate = Time.time;
            _plantCollider = transform.parent.gameObject.GetComponent<Plant>().aoeCollider;
        }

        private void Update()
        {
            if (Time.time - _lastUpdate < 2.0f)
            {
                return;
            }

            _lastUpdate = Time.time;
            _damageMultiplier = ChiliPower.CalculateDamageModifier(_plantCollider);
        }
    }
}