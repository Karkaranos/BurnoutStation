/*************************************************
Brandon Koederitz
2/22/2025
2/22/2025
Hides an object on the UI if the player clicks off of the UI.
***************************************************/
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace GraffitiGala.UI
{
    [RequireComponent(typeof(PlayerInput))]
    public class ClickoffHider : MonoBehaviour
    {
        protected InputAction pressAction;
        protected InputAction positionAction;
        private void Awake()
        {
            if (TryGetComponent(out PlayerInput input))
            {
                pressAction = input.currentActionMap.FindAction("Press");
                positionAction = input.currentActionMap.FindAction("Position");

                pressAction.performed += PressAction_Performed;
            }
        }

        private void OnDestroy()
        {
            pressAction.performed -= PressAction_Performed;
        }

        /// <summary>
        /// Hides this object if the player clicks off of the UI.
        /// </summary>
        /// <param name="obj">Unused CallbackContext.</param>
        private void PressAction_Performed(InputAction.CallbackContext obj)
        {
            if (!UIHelpers.IsPositionOverUI(positionAction.ReadValue<Vector2>()))
            {
                gameObject.SetActive(false);
            }
        }
    }
}