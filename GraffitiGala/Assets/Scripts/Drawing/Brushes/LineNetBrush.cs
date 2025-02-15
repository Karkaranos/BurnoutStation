/*************************************************
Brandon Koederitz
1/27/2025
1/27/2025
Creates and updates a LineRenderer object to draw across a network.
FishNet, InputSystem, NaughtyAttributes.
***************************************************/

using FishNet.Connection;
using FishNet.Managing.Server;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GraffitiGala.Drawing
{
    [RequireComponent(typeof(PlayerInput))]
    public class LineNetBrush : NetworkBrush
    {
        #region vars
        [SerializeField, Tooltip("The paint prefab to spawn.  Use different types" +
            " of paint prefabs to create different brush textures.")]
        internal LineBrushTexture brushTexturePrefab;

        // List of spawned game objects.
        private readonly SyncList<LineBrushTexture> drawnObjects = new();
        // Temporary list to display the spawned objects SyncList in the inspector.
        [SerializeField, ReadOnly, Header("Testing")] private List<LineBrushTexture> testObjects = new();
        // List that stores drawing states that have not initialized.
        private readonly List<DrawingState> drawingStateQueue = new();

        /// <summary>
        /// State Machine
        /// </summary>
        private DrawState state = new NotDrawingState();
        #endregion

        #region Properties
        public LineBrushTexture BrushPrefab
        {
            get
            {
                return brushTexturePrefab;
            }
            set
            {
                brushTexturePrefab = value;
            }
        }
        #endregion

        #region Nested Classes
        /// <summary>
        /// Parent class for drawing and not drawing states for this networked brush.
        /// </summary>
        private abstract class DrawState
        {
            internal abstract void HandleBrushMove(LineNetBrush brush, InputAction positionAction);
            internal abstract void InitializeLine(LineBrushTexture line);

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
            internal override void HandleBrushMove(LineNetBrush brush, InputAction positionAction) { }
            internal override void InitializeLine(LineBrushTexture line) { }
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
            private LineBrushTexture currentLine;

            internal DrawingState(LineBrushTexture line, LineNetBrush brush, InputAction positionAction)
            {
                currentLine = line;

                // Call the brush's draw function twice to make the line visibly
                // show as a dot.
                brush.Draw(line, GetMousePosition(positionAction));
                brush.Draw(line, GetMousePosition(positionAction));
            }

            /// <summary>
            /// Sets this object's reference to a server spawned line and initialize
            /// that line based on the changes made to the client line.
            /// </summary>
            /// <param name="line">The spawned line.</param>
            internal override void InitializeLine(LineBrushTexture line)
            {
                // Save a reference to the temporary client line that this DrawState
                // was updating.
                LineBrushTexture tempClientLine = currentLine;
                // Initializes the passed in line as a line that is communicating over the network.
                line.InitializeAsNetworked(tempClientLine);
                // Sets this state's currentLine as the new network line.
                currentLine = line;
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
            internal override void HandleBrushMove(LineNetBrush brush, InputAction positionAction)
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
        /// Handles the player touching the pen to the tablet.
        /// </summary>
        protected override void PressAction_Started(InputAction.CallbackContext obj)
        {
            // Updates this object's state to handle pointer movement drawing.
            // And create a new line object for this client.  This temporary
            // client line will be updated until the server passes backa  reference
            // to the spawned server line.
            DrawingState drawingState = new DrawingState(StartNewLine(
                brushTexturePrefab,
                GetMousePosition(positionAction),
                Quaternion.identity,
                null,
                CurrentColor
                ), this, positionAction);
            state = drawingState;
            // Adds the newly created drawing state to a queue to be initialized.  Even if the state changes, drawing
            // States still need to be initialized so that lines accurately reflect user inputs.
            drawingStateQueue.Add(drawingState);


            //Debug.Log("Drawing");
        }

        /// <summary>
        /// Handles the player removing the pen from the tablet.
        /// </summary>
        protected override void PressAction_Canceled(InputAction.CallbackContext obj)
        {
            // Change this brush's state to not drawing.
            state = new NotDrawingState();
            //Debug.Log("Not Drawing");
        }

        /// <summary>
        /// Handles a change in the player's pen position.
        /// </summary>
        /// <param name="obj"></param>
        protected override void MoveAction_Performed(InputAction.CallbackContext obj)
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
        internal void Draw(LineBrushTexture line, Vector3 position)
        {
            float pressure = pressureAction.ReadValue<float>();
            //DrawOnServer(line, position, pressure);
            //Adds a new point to the current line.
            line.AddPoint(position, pressure);
        }

        /// <summary>
        /// Spawns a new line.
        /// </summary>
        /// <param name="linePrefab">The prefab to spawn the line from.</param>
        /// <param name="position">The position to spawn the line at.</param>
        /// <param name="rotation">The rotation to spawmn the line with.</param>
        /// <param name="parent">The transform that will act as the parent of the line.</param>
        /// <param name="color">The color of the line.</param>
        /// <returns>The created line object over the client.</returns>
        private LineBrushTexture StartNewLine(
            LineBrushTexture linePrefab,
            Vector3 position,
            Quaternion rotation,
            Transform parent,
            Color color
            )
        {
            // Creates a line only for this client that will be updated
            LineBrushTexture clientPlaceholderLine = Instantiate(linePrefab, position, rotation, parent) as LineBrushTexture;
            clientPlaceholderLine.Configure(color);
            // Spawns and sets up the line over the server.
            StartNewLineServer(linePrefab, position, rotation, parent, this.Owner, this, color);
            // Returns the created client line.  This will be stored by the current
            // DrawingState and updated to keep the client responsive.
            return clientPlaceholderLine;
        }

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
        private void StartNewLineServer(
            LineBrushTexture linePrefab,
            Vector3 position,
            Quaternion rotation,
            Transform parent,
            NetworkConnection owner,
            LineNetBrush brush,
            Color color
            )
        {
            // Instantiates the new Line for the client
            LineBrushTexture spawnedLine = Instantiate(linePrefab, position, rotation, parent) as LineBrushTexture;
            // Spawns the line over the server.
            ServerManager.Spawn(spawnedLine.gameObject, owner);
            // Configures the line with set values such as color.
            spawnedLine.ObserverConfigure(color);
            // Adds the spawned line object to a stored SyncList of lines this 
            // object has spawned.
            drawnObjects.Add(spawnedLine);
            // Sets a reference to the spawned server line of the current drawing state
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
        private void InitializeDrawState(LineBrushTexture line, LineNetBrush brush)
        {
            if(this.IsOwner)
            {
                // Initializes the state earliest on in the queue.
                brush.drawingStateQueue[0].InitializeLine(line);
                // Removes the newly initialized state from the queue.  If that state is not the current state, no 
                // references to it will exist and it will be garbage collected.
                brush.drawingStateQueue.RemoveAt(0);
            }
        }

        ///// <summary>
        ///// Adds points to a given line at a given position.
        ///// </summary>
        ///// <param name="line">The line to update.</param>
        ///// <param name="position">The position to put the new point at.</param>
        ///// <param name="pressure">The pressure of the pen that determines the width of the line.</param>
        //private void DrawOnServer(LRBrushTexture line, Vector3 position, float pressure)
        //{

        //}

        protected override void ClearLines()
        {
            throw new System.NotImplementedException();
        }

        #region Testing
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
        #endregion
    }

}