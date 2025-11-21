using Code.Scripts.Plants.Powers;
using Code.Scripts.Plants.Powers.PowerExtension;
using UnityEngine;

namespace Code.Scripts.Managers
{
    public class PlantAoeState
    {
        public delegate void VisibilityChangeEvent(bool visible);

        public VisibilityChangeEvent OnVisibilityChange;
        public bool Visible { get; private set; }
        private bool _isPlanting;
        private bool _isHovering;
        private readonly PowerKind _powerKind;
        private GameObject _aoePlacementIndicator;

        public PlantAoeState(PowerKind power)
        {
            _powerKind = power;
        }

        private void RecalculateVisibility()
        {
            var old = Visible;
            Visible = _isHovering || _isPlanting;

            if (Visible != old)
            {
                OnVisibilityChange?.Invoke(Visible);
            }
        }

        public void Start()
        {
            _aoePlacementIndicator = _powerKind.PlaceAoeIndicator();
            _aoePlacementIndicator.SetActive(false);
        }

        public void Tick(Camera camera)
        {
            if (!_isPlanting || !_aoePlacementIndicator) return;
            var pos = camera.ScreenToWorldPoint(Input.mousePosition);
            pos.z = 1;
            _aoePlacementIndicator.transform.position = pos;
        }

        public void SetPlanting(bool isPlanting)
        {
            _isPlanting = isPlanting;
            if (_aoePlacementIndicator)
            {
                _aoePlacementIndicator.SetActive(isPlanting);
            }

            RecalculateVisibility();
        }

        public void SetHovering(bool isHovering)
        {
            _isHovering = isHovering;
            RecalculateVisibility();
        }
    }
}