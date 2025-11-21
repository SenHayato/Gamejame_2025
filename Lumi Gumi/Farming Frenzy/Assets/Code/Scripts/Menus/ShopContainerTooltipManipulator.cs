using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UIElements;

namespace Code.Scripts.Menus
{
    /// <summary>
    /// The `ShopContainerTooltipManipulator` works in conjunction with the `ShopItemTooltipManipulator` to display
    /// tooltips (defined by `UI/shop_item_tooltip.uxml) for items in the shop. The Container manipulator is attached
    /// to the shop root `VisualElement` (defined by `UI/shop.uxml`), whilst the `Item` manipulator is attached to each
    /// individual item in the shop (defined by `UI/shop_item.uxml`).
    /// 
    /// Unfortunately, it is not easy to have the manipulator only on each item. This is because of the following:
    /// #1. We want the tooltip to follow the mouse. This is done by setting its position (`top` and `left` in USS).
    /// #2. The `MouseMoveEvent` for a target `VisualElement` is relative to its own position.
    /// #3. We can't have the tooltip be the child of the shop item itself, or other shop items overlap it (there is no
    ///     z-index in UI toolkit). Therefore, the tooltip must be the child of the shop root visual element itself.
    /// #4. Given #3 (tooltip must be child of root), it is difficult to implement #1 (following mouse), because of #2
    ///     (`MouseMoveEvent` is relative to parent)
    ///
    /// The solution taken with this implementation is to have the root element control the tooltip's position and
    /// whether it is shown or hidden, and then have the child elements (shop items) control the tooltip's content.
    /// </summary>
    public class ShopContainerTooltipManipulator : Manipulator
    {
        [CanBeNull] private VisualElement _tooltipContainer;
        [CanBeNull] private VisualElement _currentTooltip;

        public bool MouseInShop { get; private set; }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseEnterEvent>(MouseEnterRoot);
            target.RegisterCallback<MouseLeaveEvent>(MouseLeaveRoot);
            target.RegisterCallback<MouseMoveEvent>(MouseMove);
            target.Q("after_shop").RegisterCallback<MouseEnterEvent>(MouseEnterAfterShop);
            target.Q("after_shop").RegisterCallback<MouseLeaveEvent>(MouseLeaveAfterShop);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseEnterEvent>(MouseEnterRoot);
            target.UnregisterCallback<MouseLeaveEvent>(MouseLeaveRoot);
            target.UnregisterCallback<MouseMoveEvent>(MouseMove);
            target.Q("after_shop").UnregisterCallback<MouseEnterEvent>(MouseEnterAfterShop);
            target.Q("after_shop").UnregisterCallback<MouseLeaveEvent>(MouseLeaveAfterShop);
        }
        
        private void MouseEnterAfterShop(MouseEnterEvent evt)
        {
            if (_tooltipContainer != null) _tooltipContainer.style.visibility = Visibility.Hidden;
        }

        private void MouseLeaveAfterShop(MouseLeaveEvent evt)
        {
            if (_tooltipContainer != null)
                _tooltipContainer.style.visibility = MouseInShop ? Visibility.Visible : Visibility.Hidden;
        }

        public void SetTooltip(VisualElement tooltip)
        {
            if (_currentTooltip != null)
            {
                _tooltipContainer?.Remove(_currentTooltip);
            }

            _currentTooltip = tooltip;
            _tooltipContainer?.Add(_currentTooltip);
        }

        private void UpdatePosition(Vector2 mousePos)
        {
            var tooltip = _tooltipContainer?.Q<VisualElement>("tooltip");
            if (tooltip == null) return;

            _tooltipContainer.style.left = mousePos.x - tooltip.resolvedStyle.width;
            _tooltipContainer.style.top = Math.Min(mousePos.y + 25, Screen.currentResolution.height - tooltip.resolvedStyle.height);
        }
        
        private void MouseEnterRoot(MouseEnterEvent e)
        {
            if (_tooltipContainer == null)
            {
                _tooltipContainer = new VisualElement {
                    style = {
                        position = Position.Absolute,
                        left = 0,
                        top = 0
                    }
                };

                target.Add(_tooltipContainer);
            }

            MouseInShop = true;
            UpdatePosition(e.localMousePosition);
            _tooltipContainer.style.visibility = Visibility.Visible;
            _tooltipContainer.BringToFront();
        }
        
        private void MouseMove(MouseMoveEvent e)
        {
            UpdatePosition(e.localMousePosition);
        }

        private void MouseLeaveRoot(MouseLeaveEvent e)
        {
            MouseInShop = false;
            if (_tooltipContainer != null) _tooltipContainer.style.visibility = Visibility.Hidden;
        }
    }
}
