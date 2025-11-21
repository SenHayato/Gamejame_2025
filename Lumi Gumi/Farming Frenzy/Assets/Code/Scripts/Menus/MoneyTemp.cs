using Code.Scripts.Player;
using TMPro;
using UnityEngine;


public class MoneyTemp : MonoBehaviour
{
    private void Update()
    {
        GetComponent<TextMeshProUGUI>().text = PlayerController.Instance.Money.ToString();
    }
}
