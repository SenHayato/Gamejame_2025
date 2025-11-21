using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Code.Scripts.Menus
{
    public class FloatingText : MonoBehaviour
    {
        private VisualElement _elt;
        private float alpha;
        private float _instantiationTime;
        private float x;
        private float initialY;
        private float deltaY;

        public void SetText(string text, Vector3 pos)
        {
            _instantiationTime = Time.time;
            _elt = GetComponent<UIDocument>().rootVisualElement.Q("tooltip");
            initialY = pos.y;
            x = pos.x;
            _elt.Q<Label>("text").text = text;
            _elt.style.color = FarmingFrenzyColors.TooExpensiveGold;
        }

        private void Update()
        {
            deltaY = 10f * (Time.time - _instantiationTime);

            _elt.style.top = Screen.currentResolution.height - initialY - deltaY;
            _elt.style.left = x;

            var color = _elt.style.color.value;
            color.a = (float) Math.Sin(Math.PI / 2.0 * Time.time - _instantiationTime);
            _elt.style.color = color;

            if (Time.time - _instantiationTime > 2f)
            {
                Destroy(gameObject);
            }
        }
    }
}