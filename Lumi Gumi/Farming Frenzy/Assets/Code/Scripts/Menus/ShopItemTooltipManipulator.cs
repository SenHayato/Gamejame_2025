using Code.Scripts.Plants.Powers;
using Code.Scripts.Plants.Powers.PowerExtension;
using UnityEngine.UIElements;

namespace Code.Scripts.Menus
{
    /// <summary>
    /// The `ShopContainerTooltipManipulator` works in conjunction with the `ShopItemTooltipManipulator` to display
    /// tooltips (defined by `UI/shop_item_tooltip.uxml) for items in the shop. The Container manipulator is attached
    /// to the shop root `VisualElement` (defined by `UI/shop.uxml`), whilst the `Item` manipulator is attached to each
    /// individual item in the shop (defined by `UI/shop_item.uxml`).
    ///
    /// <see cref="ShopContainerTooltipManipulator"/>See `ShopContainerTooltipManipulator` for a more detailed
    /// explanation of how this all works.<see/>
    /// </summary>
    public class ShopItemTooltipManipulator : Manipulator
    {
        private readonly ShopContainerTooltipManipulator _tooltipManipulator;
        private readonly VisualElement _customTooltip;
        private readonly PowerKind? _powerKind;

        public ShopItemTooltipManipulator(ShopContainerTooltipManipulator tooltipManipulator, VisualElement customTooltip, PowerKind? powerKind)
        {
            _tooltipManipulator = tooltipManipulator;
            _customTooltip = customTooltip;
            _powerKind = powerKind;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseEnterEvent>(MouseEnter);
            target.RegisterCallback<MouseLeaveEvent>(MouseLeave);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseEnterEvent>(MouseEnter);
            target.UnregisterCallback<MouseLeaveEvent>(MouseLeave);
        }

        private void MouseEnter(MouseEnterEvent _)
        {
            _powerKind?.AoeState()?.SetHovering(true);
            _tooltipManipulator.SetTooltip(_customTooltip);   
        }

        private void MouseLeave(MouseLeaveEvent _)
        {
            _powerKind?.AoeState()?.SetHovering(false);
        }
    }
}
