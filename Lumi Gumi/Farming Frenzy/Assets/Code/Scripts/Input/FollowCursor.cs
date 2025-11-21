using UnityEngine;

public class FollowCursor : MonoBehaviour
{
    #region Editor Fields
    [SerializeField] private Vector3 _offset;
    #endregion

    #region Properties
    #endregion

    #region Methods
    void Update()
    { 
        Vector3 mousePosition = Input.mousePosition;
        transform.position = mousePosition + _offset;
    }
    #endregion
}
