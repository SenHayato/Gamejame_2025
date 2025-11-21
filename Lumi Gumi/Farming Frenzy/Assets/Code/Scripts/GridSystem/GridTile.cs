using Code.Scripts.Player;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Code.Scripts.GridSystem
{
    public class GridTile : MonoBehaviour
    {
        #region Editor Fields
        [Header("Tile Settings")]
        [SerializeField] private Color _baseColour;
        [SerializeField] private Color _offsetColour;
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private GameObject _highlight;
        [SerializeField] private int _cost; //Duit yang dibutuhkan untuk buka lahan
        [SerializeField] private GameObject _tiletext;
        #endregion

        #region Properties

        private bool _isMouseOver;
        public Collider2D Collider { get; private set; }
        public bool IsPurchased { get; private set; }
        public bool CanBePurchased { get; private set; }
        public bool IsLocked { get; private set; }
        public int Cost => _cost;
        public bool HasPlant;

        public delegate void TileClicked(GridTile tile);
        public delegate void TileHoverIn(GridTile tile);
        public delegate void TileHoverOut(GridTile tile);
        public event TileClicked OnTileClicked;
        public event TileHoverIn OnTileHoverIn;
        public event TileHoverOut OnTileHoverOut;

        #endregion

        #region Methods

        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        public void Init(bool isOffset)
        {
            Collider = GetComponent<BoxCollider2D>();
            _renderer.color = isOffset ? _baseColour : _offsetColour;
        }

        private void OnMouseEnter()
        {
            _isMouseOver = true;

            if (EventSystem.current.IsPointerOverGameObject())
            {
                GridManager.Instance.HighlightTiles(this, false);
                return;
            }

            if(GridManager.Instance.PlantName != "")
            {
                GridManager.Instance.HighlightTiles(this, true);
            }

            if (CanBePurchased && !IsPurchased)
            {
                _tiletext.SetActive(true);
            }

            _highlight.SetActive(true);
            OnTileHoverIn?.Invoke(this);
        }

        private void MouseOut()
        {
            _isMouseOver = false;

            if (GridManager.Instance.PlantName != "")
            {
                GridManager.Instance.HighlightTiles(this, false);
            }

            PlayerController.Instance.EndContextualCursor(PlayerController.CursorState.Shovel);
            _tiletext.SetActive(false);
            _highlight.SetActive(false);
            OnTileHoverOut?.Invoke(this);
        }

        private void OnMouseExit()
        {
            MouseOut();
        }

        private void OnMouseDown()
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            OnTileClicked?.Invoke(this);
        }

        public void PurchaseTile(Sprite tile)
        {
            PlayerController.Instance.EndContextualCursor(PlayerController.CursorState.Shovel);
            _renderer.color = Color.white;
            _renderer.sprite = tile;
            _tiletext.SetActive(false);
            IsPurchased = true;
            CanBePurchased = true;

            // Scale the 2x2 highlight back to 1x1
            _highlight.transform.localPosition = Vector3.zero;
            _highlight.transform.localScale = Vector3.one;
        }

        public void MakePurchasable(Sprite tile)
        {
            _renderer.color = Color.white;
            _renderer.sprite = tile;
    
            if (!CanBePurchased)
            {
                // Make the highlight & 40gp text cover all the other tiles in this 2x2 block
                var pos = new Vector2((int) transform.position.x, (int) transform.position.y);
                var bl = new Vector2((int)pos.x - (int)pos.x % 2, (int)pos.y - (int)pos.y % 2);

                var toBl = bl - pos;
                var xShift = (int) -toBl.x; // 1 or 0
                var yShift = (int) -toBl.y; // 1 or 0
            
                toBl += new Vector2(1, 1); // Correct pos of highlight to centre for BL

                var newPos = toBl / 2.0f + xShift * Vector2.left / 2.0f + yShift * Vector2.down / 2.0f;
                _highlight.transform.localPosition = newPos;
                _highlight.transform.localScale = new Vector3(2.0f, 2.0f, 1.0f);
                _tiletext.transform.localPosition = newPos;
                _tiletext.transform.localScale = new Vector3(2.0f, 2.0f, 1.0f);
            }
    
            CanBePurchased = true;
        }

        public void LockTile()
        {
            Collider.enabled = false;
            IsLocked = true;
        }

        public void HighlightTile()
        {
            _highlight.SetActive(true);
        }

        public void UnHighlightTile()
        {
            _highlight.SetActive(false);
        }

        public void UnlockTile()
        {
            Collider.enabled = true;
            IsLocked = false;
        }

        private void Update()
        {
            // Only start hovering if they weren't dragging across already
            if (_isMouseOver && CanBePurchased && !IsPurchased && !Input.GetMouseButton(0))
            {
                PlayerController.Instance.StartContextualCursor(PlayerController.CursorState.Shovel);
            }

            if (_isMouseOver && Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject()) {
                OnTileClicked?.Invoke(this);
            }

            if (!_tiletext.activeSelf && !_isMouseOver) return;
            var ray = Camera.main!.ScreenPointToRay(Input.mousePosition);
            var hit = Physics2D.GetRayIntersection(ray, 1500f);
            if (hit.transform?.gameObject != gameObject) MouseOut();
        }

        #endregion
    }
}
