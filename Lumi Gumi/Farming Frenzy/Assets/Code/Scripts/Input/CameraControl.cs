using Code.Scripts.Menus;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class CameraControl : MonoBehaviour
{
    #region Editor Fields
    [Header("Camera Properties")]
    [SerializeField] private float _dragSpeed = 2.0f;
    [SerializeField] private float _scrollSpeed = 2.0f;
    [SerializeField] private float _smoothSpeed = 0.125f;

    [Header("Zoom Properties")]
    [SerializeField] private float _minZoom = 5f;
    [SerializeField] private float _maxZoom = 12f;

    [Header("Camera Bounds")]
    [SerializeField] private Tilemap _tilemap;
    #endregion

    #region Properties
    private Vector3 _dragOrigin;
    private Camera _attachedCamera;
    private Vector3 _minBounds;
    private Vector3 _maxBounds;
    #endregion

    #region Methods
    public void Start()
    {
        _attachedCamera = GetComponent<Camera>();
        UpdateCameraBounds();
    }
    private void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        var speed = 0.03f * _attachedCamera.orthographicSize;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            transform.Translate(Vector3.up * speed, Space.World);
        }

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            transform.Translate(Vector3.left * speed, Space.World);
        }

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            transform.Translate(Vector3.right * speed, Space.World);
        }

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            transform.Translate(Vector3.down * speed, Space.World);
        }
        
        HandleDrag();
        HandleZoom();
        ClampCameraPosition();
    }

    private void HandleZoom()
    {
        var scrollData = Input.GetAxis("Mouse ScrollWheel");

        // Don't zoom if in shop
        if (ShopUI.Instance.MouseInShop)
        {
            return;
        }

        if(scrollData != 0)
        {
            _attachedCamera.orthographicSize = Mathf.Clamp(_attachedCamera.orthographicSize - Input.GetAxis("Mouse ScrollWheel") * _scrollSpeed, _minZoom, _maxZoom);
            UpdateCameraBounds();
        }
    }

    private void HandleDrag()
    {
        if (Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
        {
            _dragOrigin = Input.mousePosition;
            return;
        }

        if (!(Input.GetMouseButton(1) || Input.GetMouseButton(2))) return;

        var currentMousePos = Input.mousePosition;
        var difference = Camera.main.ScreenToViewportPoint(_dragOrigin - currentMousePos);
        var move = new Vector3(difference.x, difference.y, 0) * _dragSpeed * _attachedCamera.orthographicSize;
        transform.Translate(move, Space.World);
        _dragOrigin = currentMousePos;
    }

    private void UpdateCameraBounds()
    {
        var tilemapBounds = _tilemap.localBounds;
        var camHeight = _attachedCamera.orthographicSize;
        var camWidth = camHeight * _attachedCamera.aspect;

        _minBounds = tilemapBounds.min + new Vector3(camWidth, camHeight, 0);
        _maxBounds = tilemapBounds.max - new Vector3(camWidth, camHeight, 0);
    }

    private void ClampCameraPosition()
    {
        var cameraPosition = transform.position;
        cameraPosition.x = Mathf.Clamp(cameraPosition.x, _minBounds.x, _maxBounds.x);
        cameraPosition.y = Mathf.Clamp(cameraPosition.y, _minBounds.y, _maxBounds.y);
        transform.position = Vector3.Lerp(transform.position, cameraPosition, _smoothSpeed);
    }
    #endregion
}
