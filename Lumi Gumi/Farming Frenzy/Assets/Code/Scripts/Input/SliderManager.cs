using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SliderManager : MonoBehaviour, IPointerUpHandler
{
   Slider thisSlider;
   public void OnPointerUp(PointerEventData eventData)
    {
        AudioManager.Instance.PlaySFX("goatNoises2");
    }
}
