using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/*************************************************
Brandon Koederitz
1/24/2025
1/26/2025
Creates spawned objects to draw across the network.
FishNet, InputSystem
***************************************************/

namespace GraffitiGala.Drawing
{
    [RequireComponent(typeof(PlayerInput))]
    public class NetworkedBrush : NetworkBehaviour
    {
        #region CONSTS
        private const string PRESSURE_ACTION_NAME = "Pressure";
        private const string PRESS_ACTION_NAME = "Press";
        private const string MOVE_ACTION_NAME = "Move";
        #endregion

        #region vars
        [Header("Brush Settings")]
        [SerializeField, Tooltip("The paint prefab to spawn.  Use different types" +
            " of paint prefabs to create different brush textures.")]
        internal BrushTexture brushTexturePrefab;
        [SerializeField, Tooltip("The color of this brush.")]
        private Color brushColor;
        [SerializeField, Tooltip("The distance away from where the last paint object" +
            " was spawned before the brush can spawn another.  Increasing the distance" +
            " will cause less objects to spawn and is better for performance, but will reduce" +
            " stroke quality.")]
        internal float drawBuffer;

        // Inputs
        private InputAction pressureAction;
        private InputAction pressAction;
        private InputAction positionAction;

        // List of spawned game objects.
        private readonly SyncList<GameObject> drawnObjects = new();
        // Temporary list to display the spawned objects SyncList in the inspector.
        [SerializeField, ReadOnly, Header("Testing")] private List<GameObject> testObjects = new();

        /// <summary>
        /// State Machine
        /// </summary>
        private DrawState state = new NotDrawingState();
        #endregion

        #region Nested Classes
        /// <summary>
        /// Parent class for drawing and not drawing states for this networked brush.
        /// </summary>
        private abstract class DrawState
        {
            internal abstract void HandleBrushMove(NetworkedBrush brush, InputAction positionAction);

            /// <summary>
            /// Checks to see if two vectors are within a certain range of each other
            /// </summary>
            /// <param name="vector1"> A vector to compare. </param>
            /// <param name="vector2"> A second vector to compare. </param>
            /// <param name="range"> The numerical range that the two must be within. (Inclusive) </param>
            /// <returns> Whether the two vectors are within range of each other. </returns>
            protected static bool CheckProximity(Vector3 vector1, Vector3 vector2, float range)
            {
                float xDelta = vector2.x - vector1.x;
                float yDelta = vector2.y - vector1.y;
                float zDelta = vector2.z - vector1.z;
                float distance = Mathf.Sqrt(Mathf.Pow(xDelta, 2) + Mathf.Pow(yDelta, 2) + Mathf.Pow(zDelta, 2));
                return Mathf.Abs(distance) <= range;
            }
        }
        
        /// <summary>
        /// Brush state that nulls the move function when the player is not pressing
        /// their pen.
        /// </summary>
        private class NotDrawingState : DrawState
        {
            internal override void HandleBrushMove(NetworkedBrush brush, InputAction positionAction)
            {
                // Do Nothing.
            }
        }

        /// <summary>
        /// Brush state that handles when to spawn a new drawing object.
        /// </summary>
        /// <remarks>
        /// Place any limitations placed on brushes such as 
        /// </remarks>
        private class DrawingState : DrawState
        {
            private Vector2 lastPosition;

            /// <summary>
            /// While the brush is in the drawing state, whenever the brush is moved
            /// tell the brush comnponent to draw some new prefabs.
            /// </summary>
            /// <param name="brush">The NetworkBrush component that is in the drawstate.</param>
            /// <param name="positionAction">
            /// The InputAction that handles the pointer position (pen or mouse)
            /// Used to find where to draw.
            /// </param>
            internal override void HandleBrushMove(NetworkedBrush brush, InputAction positionAction)
            {
                Vector2 currentPosition = Camera.main.ScreenToWorldPoint(positionAction.ReadValue<Vector2>());

                // If the current position is far enough away from the last position
                // that an object was spawned at, spawn a new paint object.
                if (!CheckProximity(currentPosition, lastPosition, brush.drawBuffer))
                {
                    brush.Draw(currentPosition, Quaternion.identity);
                    lastPosition = currentPosition;
                }
            }
        }
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
                positionAction = playerInput.currentActionMap.FindAction(MOVE_ACTION_NAME);

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
                //pressureAction.performed -= PressureAction_Performed;
            }
        }

        /// <summary>
        /// Handles the player touching the pen to the tablet.
        /// </summary>
        private void PressAction_Started(InputAction.CallbackContext obj)
        {
            // Change this brush's state to drawing.
            state = new DrawingState();
            Debug.Log("Drawing");
        }

        /// <summary>
        /// Handles the player removing the pen from the tablet.
        /// </summary>
        private void PressAction_Canceled(InputAction.CallbackContext obj)
        {
            // Change this brush's state to not drawing.
            state = new NotDrawingState();
            Debug.Log("Not Drawing");
        }

        /// <summary>
        /// Handles a change in the player's pen position.
        /// </summary>
        /// <param name="obj"></param>
        private void MoveAction_Performed(InputAction.CallbackContext obj)
        {
            /// Tells the current state to handle a movement in the brush.
            state.HandleBrushMove(this, positionAction);
        }

        /// <summary>
        /// Tells this brush to spawn a new paint/draw object.
        /// </summary>
        /// <param name="position">The position to spawn the paint at.</param>
        /// <param name="rotation">The rotation to spawn the pait with.</param>
        internal void Draw(Vector3 position, Quaternion rotation)
        {
            float pressure = pressureAction.ReadValue<float>();
            DrawOnServer(brushTexturePrefab, position, rotation, this.Owner, brushColor, pressure);
        }

        /// <summary>
        /// Spawns a new paint object.
        /// </summary>
        /// <param name="objToSpawn"> The paint prefab to spawn.</param>
        /// <param name="position"> The position to spawn the paint object at.</param>
        /// <param name="rotation">The rotation to spawn the paint object with.</param>
        /// <param name="owner">The network connection that owns this GameObject.</param>
        /// <param name="color"> The color of this spawned paint object.</param>
        /// <param name="pressure">
        /// The pressure of the pen to determine the size of
        /// the paint object.
        /// </param>
        [ServerRpc]
        private void DrawOnServer(
            BrushTexture objToSpawn, 
            Vector3 position, 
            Quaternion rotation, 
            NetworkConnection owner,
            Color color,
            float pressure = 0f
            )
        {
            // Instantiate the paint object for the client.
            BrushTexture spawnedPaint = Instantiate(objToSpawn, position, rotation) as BrushTexture;
            // Modifies the instantiated paint object based on color and pressure.
            spawnedPaint.ConfigurePaint(color, pressure);
            // Spawn the created paint object for all other clients.
            ServerManager.Spawn(spawnedPaint.gameObject, owner);
            // Add the newly spawned object to this component's synced list of spawned objects.
            drawnObjects.Add(spawnedPaint.gameObject);
        }

        ///// <summary>
        ///// Updates the synced pressure value whenever it changes.
        ///// </summary>
        ///// <param name="obj"></param>
        //private void PressureAction_Performed(InputAction.CallbackContext obj)
        //{
            
        //}



        ///// <summary>
        ///// Notifies the server that this client's pen is moving.
        ///// </summary>
        //[ServerRpc]
        //private void SendPenMove(Vector2 position)
        //{
        //    RecievePenMove(position);
        //}

        ///// <summary>
        ///// Recieved by client objects when the server is notified that
        ///// an owned client's pen is moving.
        ///// </summary>
        //[ObserversRpc]
        //private void RecievePenMove(Vector2 position)
        //{
        //    // Call the assigned PenMove Delegate. This will be set by a separate
        //    // Drawing script to handle drawing.
        //    if (OnPenMove != null)
        //    {
        //        OnPenMove(position);
        //    }
        //    // Testing code
        //    Debug.Log("Mouse position of " + name + " is: " + position);
        //}


        [Button]
        private void RefreshTestList()
        {
            testObjects.Clear();
            testObjects.AddRange(drawnObjects);
        }

        [Button, ServerRpc]
        private void ClearDrawing()
        {
            testObjects.Clear();
            foreach (var obj in drawnObjects)
            {
                ServerManager.Despawn(obj);
            }
            drawnObjects.Clear();
        }
        #endregion
    }

}