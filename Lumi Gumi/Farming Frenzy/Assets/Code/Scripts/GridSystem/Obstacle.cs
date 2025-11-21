using Code.Scripts.Player;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Code.Scripts.GridSystem
{
    public class Obstacle : MonoBehaviour
    {
        #region Editor Fields
        [SerializeField] private int _cost;
        #endregion

        #region Properties
        public int Cost => _cost;
        public Collider2D Collider { get; private set; }
        public delegate void ObstacleHoverIn(Obstacle tile);

        public delegate void ObstacleHoverOut(Obstacle tile);

        public event ObstacleHoverIn OnObstacleHoverIn;
        public event ObstacleHoverOut OnObstacleHoverOut;
        private bool _isMouseOver;

        #endregion

        #region Methods

        private void Start()
        {
            Collider = GetComponent<BoxCollider2D>();
        }

        private void OnMouseDown()
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;
            if(PlayerController.Instance.CurrentlyActiveCursor != PlayerController.CursorState.Shovel) return;
            if (!PlayerController.Instance.TryPurchase(_cost)) return;

            GridManager.Instance.UnlockTiles(GetComponent<BoxCollider2D>().bounds);
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            PlayerController.Instance.EndContextualCursor(PlayerController.CursorState.Shovel);
        }

        private void OnMouseEnter()
        {
            _isMouseOver = true;
            OnObstacleHoverIn?.Invoke(this);
            PlayerController.Instance.StartContextualCursor(PlayerController.CursorState.Shovel);
        }

        private void OnMouseExit()
        {
            _isMouseOver = false;
            OnObstacleHoverOut?.Invoke(this);
            PlayerController.Instance.EndContextualCursor(PlayerController.CursorState.Shovel);
        }

        private void Update()
        {
            if (!_isMouseOver) return;
            var ray = Camera.main!.ScreenPointToRay(Input.mousePosition);
            var hit = Physics2D.GetRayIntersection(ray, 1500f);
            if (hit.transform?.gameObject == gameObject) return;
            _isMouseOver = false;
            PlayerController.Instance.EndContextualCursor(PlayerController.CursorState.Shovel);
        }

        #endregion
    }
}
