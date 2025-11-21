using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    private Slider slider;
    public Image fillImage;
    
    void Awake()
    {
        slider = gameObject.GetComponent<Slider>();
        fillImage.color = Color.green;
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
        if(slider.value > 0.66) fillImage.color = Color.green;
        else if(slider.value < 0.33) fillImage.color = Color.red;
        else fillImage.color = Color.yellow;
    }

}