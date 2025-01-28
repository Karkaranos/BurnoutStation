using FishNet.Connection;
using FishNet.Managing.Server;
using FishNet.Object.Synchronizing;
using FishNet.Object;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/*************************************************
Brandon Koederitz
1/27/2025
1/27/2025
Creates and updates a LineRenderer object to draw across a network.
FishNet, InputSystem
***************************************************/

namespace GraffitiGala.Drawing
{
    [RequireComponent(typeof(PlayerInput))]
    public class NetworkedLRBrush : NetworkBehaviour
    {
        #region CONSTS
        private const string PRESSURE_ACTION_NAME = "Pressure";
        private const string PRESS_ACTION_NAME = "Press";
        private const string POSITION_ACTION_NAME = "Position";
        #endregion

        #region vars
        [Header("Brush Settings")]
        [SerializeField, Tooltip("The paint prefab to spawn.  Use different types" +
            " of paint prefabs to create different brush textures.")]
        internal LRBrushTexture brushTexturePrefab;
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
        private readonly SyncList<LRBrushTexture> drawnObjects = new();
        // Temporary list to display the spawned objects SyncList in the inspector.
        [SerializeField, ReadOnly, Header("Testing")] private List<LRBrushTexture> testObjects = new();

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
            internal abstract void HandleBrushMove(NetworkedLRBrush brush, InputAction positionAction);
            internal abstract void InitializeLine(LRBrushTexture line, NetworkedLRBrush brush, InputAction positionAction);
            internal abstract LRBrushTexture GetLineReference();

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
            // NotDrawingState should do nothing when it's functions are called.
            internal override void HandleBrushMove(NetworkedLRBrush brush, InputAction positionAction) { }
            internal override void InitializeLine(LRBrushTexture line, NetworkedLRBrush brush, InputAction positionAction) { }
            internal override LRBrushTexture GetLineReference() { return null; }
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
            private LRBrushTexture currentLine;

            /// <summary>
            /// Sets this object's reference to a server spawned line.
            /// </summary>
            /// <param name="line">The spawned line.</param>
            internal override void InitializeLine(LRBrushTexture line, NetworkedLRBrush brush, InputAction positionAction)
            {
                // Only do this if line has yet to be set.
                if(currentLine == null)
                {
                    currentLine = line;

                    // Call the brush's draw function twice to make the line visibly
                    // show as a dot.
                    brush.Draw(line, GetMousePosition(positionAction));
                    brush.Draw(line, GetMousePosition(positionAction));
                }
                //else
                //{
                //    Debug.LogError("Duplicate line reference was attempted to be assigned to brush " + brush.name);
                //}
            }

            /// <summary>
            /// Returns this object's reference to the line that is currently being 
            /// drawn.
            /// </summary>
            /// <returns>The currentl line being drawn.</returns>
            internal override LRBrushTexture GetLineReference()
            {
                return currentLine;
            }

            /// <summary>
            /// While the brush is in the drawing state, whenever the brush is moved
            /// tell the brush comnponent to draw some new prefabs.
            /// </summary>
            /// <param name="brush">The NetworkBrush component that is in the draw state.</param>
            /// <param name="positionAction">
            /// The InputAction that handles the pointer position (pen or mouse)
            /// Used to find where to draw.
            /// </param>
            internal override void HandleBrushMove(NetworkedLRBrush brush, InputAction positionAction)
            {
                // Prevents MoveBrush from giving a nullref.  Simply wait until the current line is set.
                if(currentLine == null)
                {
                    return;
                }

                Vector2 currentPosition = GetMousePosition(positionAction);

                // If the current position is far enough away from the last position
                // that an object was spawned at, spawn a new paint object.
                if (!CheckProximity(currentPosition, lastPosition, brush.drawBuffer))
                {
                    brush.Draw(currentLine, currentPosition);
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
                //pressureAction.performed -= PressureAction_Performed;
            }
        }

        /// <summary>
        /// Handles the player touching the pen to the tablet.
        /// </summary>
        private void PressAction_Started(InputAction.CallbackContext obj)
        {
            // Updates this object's state to handle pointer movement drawing.
            state = new DrawingState();

            // Creates a new line to draw.
            StartNewLine(
                brushTexturePrefab, 
                GetMousePosition(positionAction), 
                Quaternion.identity,
                null,
                this.Owner,
                this,
                brushColor
                );
            //Debug.Log("Drawing");
        }

        /// <summary>
        /// Handles the player removing the pen from the tablet.
        /// </summary>
        private void PressAction_Canceled(InputAction.CallbackContext obj)
        {
            // Change this brush's state to not drawing.
            state = new NotDrawingState();
            //Debug.Log("Not Drawing");
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
        /// Gets the mouse position in world space from the InputAction that tracks it.
        /// </summary>
        /// <param name="positionAction">The input action that is bound to mouse position.</param>
        /// <returns>The mouse position as a Vector3 in world space.</returns>
        internal static Vector2 GetMousePosition(InputAction positionAction)
        {
            return Camera.main.ScreenToWorldPoint(positionAction.ReadValue<Vector2>());
        }

        /// <summary>
        /// Tells this brush to spawn a new paint/draw object.
        /// </summary>
        /// <param name="line"> The line to update.</param>
        /// <param name="position">The position to spawn the paint at.</param>
        internal void Draw(LRBrushTexture line, Vector3 position)
        {
            float pressure = pressureAction.ReadValue<float>();
            DrawOnServer(line, position, pressure);
        }

        ///// <summary>
        ///// Spawns a new line.
        ///// </summary>
        ///// <param name="linePrefab">The prefab to spawn the line from.</param>
        ///// <param name="position">The position to spawn the line at.</param>
        ///// <param name="rotation">The rotation to spawmn the line with.</param>
        ///// <param name="parent">The transform that will act as the parent of the line.</param>
        ///// <param name="owner">The network connection that will own this line.</param>
        ///// <param name="color">The color of the line.</param>
        ///// <returns>The created line object over the client.</returns>
        //private LRBrushTexture StartNewLine(
        //    LRBrushTexture linePrefab,
        //    Vector3 position,
        //    Quaternion rotation,
        //    Transform parent,
        //    NetworkConnection owner,
        //    Color color
        //    )
        //{

        //    // Spawns and sets up the line over the server.
        //    StartNewLineServer(spawnedLine, position, rotation, parent, owner, color);
        //    return spawnedLine;
        //}

        /// <summary>
        /// Spawns a new line object over the network to be updated as the player
        /// moves their pointer.
        /// </summary>
        /// <param name="linePrefab">The prefab to spawn the line from.</param>
        /// <param name="position">The position the line should be spawned at.</param>
        /// <param name="rotation">The rotation of the line object.</param>
        /// <param name="parent">
        /// The transform that this object will be a child 
        /// of for hierarchy organization.
        /// </param>
        /// <param name="owner">The network connection that owns this spawned object.</param>
        /// <param name="brush">The brush that is creating this line.</param>
        /// <param name="color">The color of the line.</param>
        /// <returns>The created line object.</returns>
        [ServerRpc]
        private void StartNewLine(
            LRBrushTexture linePrefab,
            Vector3 position,
            Quaternion rotation,
            Transform parent,
            NetworkConnection owner,
            NetworkedLRBrush brush,
            Color color
            )
        {
            // Instantiates the new Line for the client
            LRBrushTexture spawnedLine = Instantiate(linePrefab, position, rotation, parent) as LRBrushTexture;
            // Spawns the line over the server.
            ServerManager.Spawn(spawnedLine.gameObject, owner);
            // Configures the line with set values such as color.
            spawnedLine.Configure(color);
            // Adds the spawned line object to a stored SyncList of lines this 
            // object has spawned.
            drawnObjects.Add(spawnedLine);
            // Sets a reference to the spawned line of the current drawing state
            // of the object that spawned this new line.
            InitializeDrawState(spawnedLine, brush);
        }

        /// <summary>
        /// Gives the current state a refrence to a created line and starts 
        /// drawing that line.
        /// </summary>
        /// <remarks>
        /// Need to use an ObserversRPC here because a ServerRpc cant return anything
        /// and the state object cannot directly have an ObserverRpc.
        /// </remarks>
        /// <param name="line">The created line.</param>
        /// <param name="brush">The brush that created the line.</param>
        [ObserversRpc]
        private void InitializeDrawState(LRBrushTexture line, NetworkedLRBrush brush)
        {
            // Passes a reference to a spawned line to the current state.
            brush.state.InitializeLine(line, this, positionAction);
        }

        /// <summary>
        /// Adds points to a given line at a given position.
        /// </summary>
        /// <param name="line">The line to update.</param>
        /// <param name="position">The position to put the new point at.</param>
        /// <param name="pressure">The pressure of the pen that determines the width of the line.</param>
        private void DrawOnServer(LRBrushTexture line, Vector3 position, float pressure)
        {
            //Adds a new point to the current line.
            line.AddPoint(position, pressure);
        }


        [Button]
        private void RefreshTestList()
        {
            testObjects.Clear();
            testObjects.AddRange(drawnObjects);
        }

        [Button, ServerRpc(RequireOwnership = false)]
        private void ClearDrawing()
        {
            testObjects.Clear();
            foreach (var obj in drawnObjects)
            {
                ServerManager.Despawn(obj.gameObject);
            }
            drawnObjects.Clear();
        }
        #endregion
    }

}