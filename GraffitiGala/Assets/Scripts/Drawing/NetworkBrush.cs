/*************************************************
Brandon Koederitz
1/29/2025
2/15/2025
Draws images that are shared across the network.
FishNet, InputSystem
***************************************************/

using FishNet.Object;
using GraffitiGala.Admin;
using System;
using Unity.VisualScripting;
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

        public static Color CurrentColor { protected get; set; }
        public static float CurrentThickness { protected get; set; } = 1f;
        #endregion

        #region Properties
        [Obsolete("BrushColor is obsolete.  Use NetworkBrush.CurrentColor instead.")]
        public Color BrushColor // networkBrush.BrushColor = Blue
        {

            get
            {
               // print("wantToChangeColor");
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

            // Extra null check for if this object exists in case this function gets called from the server when the
            // client has disconnected and this gameobjecct no longer exists.
            if (gameObject == null)
            {
                return;
            }
            // Need to register ProvideLines even for non owner clients.
            // Make sure to register ProvideLines first, so that if OnDisable gets called later in OnStartClient
            // ProvideLines will be correctly unsubscribed.
            //Debug.Log("Subscribed");
            //PlayerHider.LineRequest += ProvideLines;

            //PlayerInput playerInput = GetComponent<PlayerInput>();
            TryGetComponent(out PlayerInput playerInput);
            if (base.IsOwner)
            {
                // Set up InputActions and input functions.
                pressAction = playerInput.currentActionMap.FindAction(PRESS_ACTION_NAME);
                pressureAction = playerInput.currentActionMap.FindAction(PRESSURE_ACTION_NAME);
                positionAction = playerInput.currentActionMap.FindAction(POSITION_ACTION_NAME);

                // Enable on client start for playtesting.  Later, enable it on a broadcast when the experience starts.
                //EnableBrush();

                BrushManager.EnableBrushEvent += EnableBrush;
                BrushManager.ClearLinesEvent += ClearLinesOwner;
                BrushManager.DisableBrushEvent += DisableBrush;

            }
            else
            {
                // If this object is not owned by this client, then disable
                // it's PlayerInput and this component.
                playerInput.enabled = false;
                // We cant disable this component because it is still needed to provide lines to the admin.
                //this.enabled = false;
            }
        }

        ///// <summary>
        ///// unsubscribe ProvideLines on disable as disabled brushes should not provide any lines.
        ///// </summary>
        //private void OnDisable()
        //{
        //    Debug.Log("Unsubscribed from Disable");
        //    PlayerHider.LineRequest -= ProvideLines;
        //}

        public override void OnStopClient()
        {
            if (base.IsOwner)
            {
                DisableBrush();

                BrushManager.EnableBrushEvent -= EnableBrush;
                BrushManager.ClearLinesEvent -= ClearLinesOwner;
                BrushManager.DisableBrushEvent -= DisableBrush;
            }
            //Debug.Log("Unsubscribed from StopClient");
            //PlayerHider.LineRequest -= ProvideLines;
        }
        #endregion

        /// <summary>
        /// Enables this brush on the owner client by subscribing to input functions.
        /// </summary>
        protected virtual void EnableBrush()
        {
            if (base.IsOwner)
            {
                pressAction.started += PressAction_Started;
                pressAction.canceled += PressAction_Canceled;
                positionAction.performed += MoveAction_Performed;
            }
        }

        /// <summary>
        /// Disables this brush on its owner client by unsubscribing from input functions.
        /// </summary>
        protected virtual void DisableBrush()
        {
            if (base.IsOwner)
            {
                pressAction.started -= PressAction_Started;
                pressAction.canceled -= PressAction_Canceled;
                positionAction.performed -= MoveAction_Performed;
            }
        }

        #region Statics
        /// <summary>
        /// Gets the mouse position in world space from the InputAction that tracks it.
        /// </summary>
        /// <returns>The mouse position as a Vector3 in world space.</returns>
        protected internal Vector2 GetPointerPosition()
        {
            return Camera.main.ScreenToWorldPoint(positionAction.ReadValue<Vector2>());
        }
        #endregion

        /// <summary>
        /// Extra check before calling ClearLines so that only clients that are owners of a brush clear that brush's 
        /// lines.
        /// </summary>
        protected void ClearLinesOwner()
        {
            if (base.IsOwner)
            {
                ClearLines();
            }
        }

        protected abstract void ClearLines();

        public virtual void ProvideLines(PlayerHider hider) { }

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