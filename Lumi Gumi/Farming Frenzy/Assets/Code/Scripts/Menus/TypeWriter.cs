using System.Collections;
using TMPro;
using UnityEngine;

public class TypeWriter : MonoBehaviour
{
    #region Editor Fields
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private float _typeSpeed;
    #endregion

    #region Properties
    private string _fullText;
    private string _currentText;
    #endregion

    #region Methods


    private void OnEnable()
    {
        _fullText = _text.text;
        StartCoroutine(TypeText());
    }

    private IEnumerator TypeText()
    {
        for (int i = 0; i <= _fullText.Length; i++)
        {
            _currentText = _fullText.Substring(0, i);
            _text.text = _currentText;
            yield return new WaitForSeconds(_typeSpeed);
        }
    }
    #endregion
}
