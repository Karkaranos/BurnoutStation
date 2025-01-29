/*************************************************
Brandon Koederitz
1/29/2025
1/29/2025
Draws images that are shared across the network.
FishNet, InputSystem
***************************************************/

using FishNet.Object;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GraffitiGala.Drawing
{
    [RequireComponent(typeof(PlayerInput))]
    public abstract class NetworkBrush : NetworkBehaviour
    {
        #region vars
        #region CONSTS
        private const string PRESSURE_ACTION_NAME = "Pressure";
        private const string PRESS_ACTION_NAME = "Press";
        private const string POSITION_ACTION_NAME = "Position";
        #endregion
        
        #region SerializedField
        [Header("Brush Settings")]
        [SerializeField, Tooltip("The color of this brush.")]
        protected Color brushColor;
        [SerializeField, Tooltip("The distance away from where the last paint object" +
            " was spawned before the brush can spawn another.  Increasing the distance" +
            " will cause less objects to spawn and is better for performance, but will reduce" +
            " stroke quality.")]
        protected internal float drawBuffer;
        #endregion

        // Inputs
        protected InputAction pressureAction;
        protected InputAction pressAction;
        protected InputAction positionAction;
        #endregion

        #region Properties
        public Color BrushColor
        {
            get
            {
                return brushColor;
            }
            set
            {
                brushColor = value;
            }
        }
        #endregion

        #region Methods
        #region Setup
        /// <summary>
        /// Sets up this as a networked object.
        /// </summary>
        public override void OnStartClient()
        {
            base.OnStartClient();

            PlayerInput playerInput = GetComponent<PlayerInput>();
            if (base.IsOwner)
            {
                // Set up InputActions and input functions.
                pressAction = playerInput.currentActionMap.FindAction(PRESS_ACTION_NAME);
                pressureAction = playerInput.currentActionMap.FindAction(PRESSURE_ACTION_NAME);
                positionAction = playerInput.currentActionMap.FindAction(POSITION_ACTION_NAME);

                pressAction.started += PressAction_Started;
                pressAction.canceled += PressAction_Canceled;
                //pressureAction.performed += PressureAction_Performed;
                positionAction.performed += MoveAction_Performed;
            }
            else
            {
                // If this object is not owned by this client, then disable
                // it's PlayerInput and this component.
                playerInput.enabled = false;
                this.enabled = false;
                return;
            }
        }

        public override void OnStopClient()
        {
            if (base.IsOwner)
            {
                // Unsubscribe input functions.
                pressAction.started -= PressAction_Started;
                pressAction.canceled -= PressAction_Canceled;
                positionAction.performed -= MoveAction_Performed;
                //pressureAction.performed -= PressureAction_Performed;
            }
        }
        #endregion

        #region Input Functions

        /// <summary>
        /// Handles the player touching the pen to the tablet.
        /// </summary>
        protected abstract void PressAction_Started(InputAction.CallbackContext obj);

        /// <summary>
        /// Handles the player removing the pen from the tablet.
        /// </summary>
        protected abstract void PressAction_Canceled(InputAction.CallbackContext obj);

        /// <summary>
        /// Handles a change in the player's pen position.
        /// </summary>
        protected abstract void MoveAction_Performed(InputAction.CallbackContext obj);
        #endregion

        #endregion
    }
}