using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet.Connection;
using UnityEngine.InputSystem;
using NaughtyAttributes;
using FishNet.Object.Synchronizing;

/*************************************************
Brandon Koederitz
1/24/2025
1/24/2025
Handles sending and recieving pen inputs across a network to allow for syncronized
drawing.
FishNet
***************************************************/

namespace GraffitiGala.Networking
{
    [RequireComponent(typeof(PlayerInput))]
    public class NetworkedBrush : NetworkBehaviour
    {
        public delegate void OnPenMove(Vector2 position);
        #region vars
        [SerializeField, ReadOnly] private bool isPressedTest;
        [SerializeField, ReadOnly] private float pressureTest;

        private const string PRESSURE_ACTION_NAME = "Pressure";
        private const string PRESS_ACTION_NAME = "Press";
        private const string MOVE_ACTION_NAME = "Move";

        private readonly SyncVar<float> pressure = new();
        private readonly SyncVar<bool> isPressed = new();

        private InputAction pressureAction;
        private InputAction pressAction;
        private InputAction moveAction;
        #endregion

        #region Properties
        public float Pressure
        {
            get
            {
                return pressure.Value;
            }
            private set
            {
                // Only allow an object owned by this client to change the
                // pressure value.
                if(base.IsOwner)
                {
                    pressure.Value = value;
                }
                
            }
        }

        public bool IsPressed
        {
            get
            {
                return isPressed.Value;
            }
            private set
            {
                // Only allow ann objecct owned by this client to change the
                // IsPressed value.
                isPressed.Value = value;
            }
        }

        public OnPenMove PenMove { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// Sets up this as a networked object.
        /// </summary>
        public override void OnStartClient()
        {
            base.OnStartClient();

            PlayerInput playerInput = GetComponent<PlayerInput>();
            if(base.IsOwner)
            {
                // Set up InputActions and input functions.
                pressAction = playerInput.currentActionMap.FindAction(PRESS_ACTION_NAME);
                pressureAction = playerInput.currentActionMap.FindAction(PRESSURE_ACTION_NAME);
                moveAction = playerInput.currentActionMap.FindAction(MOVE_ACTION_NAME);

                pressAction.started += PressAction_Started;
                pressAction.canceled += PressAction_Canceled;
                pressureAction.performed += PressureAction_Performed;
                moveAction.performed += MoveAction_Performed;
            }
            else
            {
                // If this object is not owned by this client, then disable
                // it's PlayerInput
                playerInput.enabled = false;
                return;
            }
        }

        private void OnDestroy()
        {
            if (base.IsOwner)
            {
                // Unsubscribe input functions.
                pressAction.started -= PressAction_Started;
                pressAction.canceled -= PressAction_Canceled;
                pressureAction.performed -= PressureAction_Performed;
            }
        }

        // Done purely for testing purposes to display pressure and isPressed values
        // in the inspector.
        private void Update()
        {
            isPressedTest = IsPressed;
            pressureTest = Pressure;
        }

        /// <summary>
        /// Handles the player touching the pen to the tablet.
        /// </summary>
        private void PressAction_Started(InputAction.CallbackContext obj)
        {
            IsPressed = true;
        }

        /// <summary>
        /// Handles the player removing the pen from the tablet.
        /// </summary>
        /// <param name="obj"></param>
        private void PressAction_Canceled(InputAction.CallbackContext obj)
        {
            IsPressed = false;
        }

        /// <summary>
        /// Handles a change in the player's pen position.
        /// </summary>
        /// <param name="obj"></param>
        private void MoveAction_Performed(InputAction.CallbackContext obj)
        {
            if(base.IsOwner)
            {
                SendPenMove(moveAction.ReadValue<Vector2>());
            }
        }

        /// <summary>
        /// Updates the synced pressure value whenever it changes.
        /// </summary>
        /// <param name="obj"></param>
        private void PressureAction_Performed(InputAction.CallbackContext obj)
        {
            // Updates the pressure value;.
            Pressure = pressureAction.ReadValue<float>();
        }

        /// <summary>
        /// Notifies the server that this client's pen is moving.
        /// </summary>
        [ServerRpc]
        private void SendPenMove(Vector2 position)
        {
            RecievePenMove(position);
        }

        /// <summary>
        /// Recieved by client objects when the server is notified that
        /// an owned client's pen is moving.
        /// </summary>
        [ObserversRpc]
        private void RecievePenMove(Vector2 position)
        {
            // Call the assigned PenMove Delegate. This will be set by a separate
            // Drawing script to handle drawing.
            if (PenMove != null)
            {
                PenMove(position);
            }
            // Testing code
            Debug.Log("Mouse position of " + name + " is: " + position);
        }

        /// <summary>
        /// Tests networking by changing the sprite's color across all clients.
        /// </summary>
        [ServerRpc, Button]
        private void SendTest()
        {
            RecieveTest(Random.ColorHSV());
        }

        [ObserversRpc]
        private void RecieveTest(Color setColor)
        {
            GetComponent<SpriteRenderer>().color = setColor; ;
        }

        /// <summary>
        /// Tests that NetworkTransform is working by changing the transform 
        /// local position;
        /// </summary>
        [Button]
        private void TestTransform()
        {
            if(this.IsOwner)
            {
                transform.position += Vector3.up;
            }
        }
        #endregion
    }

}