/*************************************************
Brandon Koederitz
1/30/2025
2/22/2025
Draws images using a mesh that are shared across the network.
FishNet, InputSystem, NaughtyAttributes
***************************************************/

using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FMOD.Studio;
using GraffitiGala.Admin;
using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GraffitiGala.Drawing
{
    public delegate void ClearLineCallback(MeshBrushTexture localLine);
    [RequireComponent(typeof(PlayerInput))]
    public class MeshNetBrush : NetworkBrush
    {
        #region vars
        #region CONSTS
        private const float Z_MIN = 0f;
        private const float Z_MAX = -5f;
        #endregion
        [SerializeField, Tooltip("The paint prefab to spawn.  Use different types" +
    " of paint prefabs to create different brush textures.")]
        internal MeshBrushTexture brushTexturePrefab;
        // Material is not supported for serialization so it cant be sent over the network.
        // Brush textures must have a material assigned.
        //[SerializeField, Tooltip("The material to use for this brush.")]
        //internal Material brushMaterial;
        [SerializeField] private bool playSoundEffects;
        [SerializeReference, Tooltip("The width of this line when the player is applying" +
    " the minimum amount of pressure to the pen.  If this value is larger than" +
    " Max Pressure Width, then the line will appear larger when less pressure is applied.")]
        private float minPressureWidth;
        [SerializeReference, Tooltip("The width of this line when the player is applying" +
            " the maximum amount of pressure to the pen.  If this value is smaller than" +
            " Min Pressure Width, then the line will appear smaller when more pressure is applied.")]
        private float maxPressureWidth;

        // List of spawned game objects.
        private readonly SyncList<MeshBrushTexture> drawnObjects = new();
        private readonly List<MeshBrushTexture> localLines = new();
        // Temporary list to display the spawned objects SyncList in the inspector.
        [SerializeField, ReadOnly, Header("Testing")] private List<MeshBrushTexture> testObjects = new();
        // List that stores drawing states that have not initialized.
        private readonly List<DrawingState> drawingStateQueue = new();

        /// <summary>
        /// State Machine
        /// </summary>
        private DrawState state = new NotDrawingState();

        public static PlayTimer PlayTimer { private get; set; }
        private static Transform drawingParent;

        private EventInstance spray;
        #endregion

        #region Properties
        private static Transform DrawingParent
        {
            get
            {
                if (drawingParent == null)
                {
                    drawingParent = GameObject.FindGameObjectWithTag("DrawingParent").transform;
                }
                return drawingParent;
            }
        }
        #endregion

        #region Nested Classes
        /// <summary>
        /// Parent class for drawing and not drawing states for this networked brush.
        /// </summary>
        private abstract class DrawState
        {
            internal abstract void HandleBrushMove(MeshNetBrush brush);
            internal abstract void InitializeLine(MeshBrushTexture line, ClearLineCallback clc);

            ///// <summary>
            ///// Checks to see if two vectors are within a certain range of each other
            ///// </summary>
            ///// <param name="vector1"> A vector to compare. </param>
            ///// <param name="vector2"> A second vector to compare. </param>
            ///// <param name="range"> The numerical range that the two must be within. (Inclusive) </param>
            ///// <returns> Whether the two vectors are within range of each other. </returns>
            //protected static bool CheckProximity(Vector3 vector1, Vector3 vector2, float range)
            //{
            //    float xDelta = vector2.x - vector1.x;
            //    float yDelta = vector2.y - vector1.y;
            //    float zDelta = vector2.z - vector1.z;
            //    float distance = Mathf.Sqrt(Mathf.Pow(xDelta, 2) + Mathf.Pow(yDelta, 2) + Mathf.Pow(zDelta, 2));
            //    return Mathf.Abs(distance) <= range;
            //}
        }

        /// <summary>
        /// Brush state that nulls the move function when the player is not pressing
        /// their pen.
        /// </summary>
        private class NotDrawingState : DrawState
        {
            // NotDrawingState should do nothing when it's functions are called.
            internal override void HandleBrushMove(MeshNetBrush brush) { }
            internal override void InitializeLine(MeshBrushTexture line, ClearLineCallback clc) { }
        }

        /// <summary>
        /// Brush state that handles the playing drawing with their pen to the tablet.
        /// </summary>
        /// <remarks>
        /// Place any limitations placed on brushes here.
        /// </remarks>
        private class DrawingState : DrawState
        {
            private Vector2 lastPosition;
            private MeshBrushTexture currentLine;

            internal DrawingState(MeshBrushTexture line, MeshNetBrush brush)
            {
                // Sets the current line that this state is updating to be the temporary localLine that was
                // created.
                currentLine = line;
                lastPosition = brush.GetPointerPosition();
            }

            /// <summary>
            /// Sets this object's reference to a server spawned line and initialize
            /// that line based on the changes made to the client line.
            /// </summary>
            /// <param name="spawnedLine">The spawned line.</param>
            internal override void InitializeLine(MeshBrushTexture spawnedLine, ClearLineCallback clc)
            {
                // Initializes the spawned line as now communication over the network, and passes the temporary
                // localLine (which is currently stored in currentLine) to it to laod the mesh from it and to
                // destroy it.
                spawnedLine.InitializeAsNetworked(currentLine, clc);
                currentLine = spawnedLine;
            }

            /// <summary>
            /// While the brush is in the drawing state, whenever the brush is moved
            /// tell the associated line that is being drawn to add new points
            /// </summary>
            /// <param name="brush">The NetworkBrush component that is in the draw state.</param>
            internal override void HandleBrushMove(MeshNetBrush brush)
            {
                // Check for NullRefs
                if(currentLine == null)
                {
                    return;
                }

                Vector2 currentPosition = brush.GetPointerPosition();
                // Calculates the vector representing the change in position from the last to the current..
                Vector2 moveDelta = currentPosition - lastPosition;
                // Checks if the new position is outside the range of the drawBuffer.  If the pen as not move enough, 
                // then a new point is not added to prevent multiple points from being too close together and
                // causing memory issues.
                if(Mathf.Abs(moveDelta.magnitude) > brush.drawBuffer)
                {
                    brush.Draw(currentLine, currentPosition, moveDelta);
                    lastPosition = currentPosition;
                }
            }
        }
        #endregion

        #region Setup and Cleanup
        /// <summary>
        /// Clears lines made by this objecct when the client starts.
        /// </summary>
        public override void OnStartClient()
        {
            base.OnStartClient();
            // Clear out all lines when a client starts for debugging purposes.  Sometimes lines will be left over
            // on the client from errors.
             ClearLinesOwner();

            if (FindObjectOfType<BuildManager>().BuildTypeRef == BuildType.TabletStation)
            {
                spray = AudioManager.instance.CreateEventInstance(FMODEventsManager.instance.Spraypaint);
            }
        }

        /// <summary>
        /// Clears all local lines if this client disconnects so that they do not persist between connections.
        /// </summary>
        public override void OnStopClient()
        {
            ClearLocalLines();
            if (playSoundEffects)
            {
                spray.stop(STOP_MODE.IMMEDIATE);
            }
        }

        /// <summary>
        /// Stop sound effects when the brush is disabled.
        /// </summary>
        protected override void DisableBrush()
        {
            base.DisableBrush();
            if (playSoundEffects)
            {
                spray.stop(STOP_MODE.IMMEDIATE);
            }
        }
        #endregion

        #region Input Functions
        /// <summary>
        /// Handles the playing touching the pen to the tablet.
        /// </summary>
        /// <param name="obj">Unused.</param>
        protected override void PressAction_Started(InputAction.CallbackContext obj)
        {
            //Debug.Log(EventSystem.current.IsPointerOverGameObject());
            // Prevvents drawing if the pointer is over a UI element like a button.
            // Will need to test if it works with drawing pens and touch input.
            if (!UIHelpers.IsPositionOverUI(positionAction.ReadValue<Vector2>()))
            {
                // Creates a new line and a new drawing state to link to that line.
                DrawingState drawingState = new DrawingState(CreateNewLine(
                    brushTexturePrefab,
                    GetPointerPosition(),
                    DrawingParent,
                    CurrentColor
                    ), this);
                // Sets the current state as the newly created drawing state.
                state = drawingState;
                if (playSoundEffects)
                {
                    spray.start();
                }
                // Adds the drawing state to the queue for initialization of it's line over the network.
                drawingStateQueue.Add(drawingState);
            }
        }

        /// <summary>
        /// Handles the player removing the pen from the tablet.
        /// </summary>
        /// <param name="obj">Unused.</param>
        protected override void PressAction_Canceled(InputAction.CallbackContext obj)
        {
            // Sets the current state to not drawing.
            state = new NotDrawingState();
            if (playSoundEffects)
            {
                spray.stop(STOP_MODE.IMMEDIATE);
            }
        }

        /// <summary>
        /// Handles the player moving the stylus across the tablet screen.
        /// </summary>
        /// <param name="obj">Unused.</param>
        protected override void MoveAction_Performed(InputAction.CallbackContext obj)
        {
            // Delegates control to the current state.
            state.HandleBrushMove(this);
        }
        #endregion

        /// <summary>
        /// Lets an external script give this script a reference to the current play timer.
        /// </summary>
        /// <param name="timer">The current play timer.</param>
        /// <returns>Whether the play timer was assigned or if it already has a value.</returns>
        public static bool SetPlayTimer(PlayTimer timer)
        {
            if(PlayTimer != null && PlayTimer != timer)
            {
                return false;
            }
            else
            {
                PlayTimer = timer;
                return true;
            }
        }

        /// <summary>
        /// Gets the current z position of line points as determined by the current timer.
        /// </summary>
        /// <returns></returns>
        private static float GetZLayer()
        {
            return Mathf.Lerp(Z_MIN, Z_MAX, PlayTimer.NormalizedProgress);
        }

        /// <summary>
        /// Gets the width of a line based on either pressure or a set thickness.
        /// </summary>
        /// <returns>The width that the line should be.</returns>
        private float GetThickness()
        {
            return CurrentThickness;
            //return Mathf.Lerp(minPressureWidth, maxPressureWidth, pressureAction.ReadValue<float>());
        }

        /// <summary>
        /// Tells a given line to add a new point to the line.
        /// </summary>
        /// <param name="line">The line to add a point to.</param>
        /// <param name="position">The position to draw that point at.</param>
        /// <param name="drawDirection">The direction the line is moving to calculate the verticies.</param>
        internal void Draw(MeshBrushTexture line, Vector3 position, Vector2 drawDirection)
        {
            // Gets the z position of this new point on the line so that it overlaps previous points.
            position.z = GetZLayer();
            // Adds a new point tot he currently draw mesh-based line.
            line.AddPoint(position, drawDirection, GetThickness());
        }

        /// <summary>
        /// Spawns a new line.
        /// </summary>
        /// <param name="meshPrefab">The prefab to spawn the line from.</param>
        /// <param name="position">The position to spawn the line at.</param>
        /// <param name="parent">The transform that will act as the parent for this line.</param>
        /// <param name="color">The color of the line</param>
        /// <returns>The created tmeporary localLine.</returns>
        private MeshBrushTexture CreateNewLine(
            MeshBrushTexture meshPrefab, 
            Vector3 position,  
            Transform parent, 
            Color color)
        {
            // Gets the z position of this new point on the line so that it overlaps previous points.
            position.z = GetZLayer();
            // Instantiate a new line purely on the local client side.
            MeshBrushTexture localLine = Instantiate(meshPrefab, Vector2.zero, Quaternion.identity, parent);
            localLine.Initialize(color);
            localLine.Local_InitializeMesh(GetThickness(), position);
            // Adds the local line to a list of created local lines.  Keeps track of this to ensure that they are
            // cleared properly if they never are removed by the server.
            localLines.Add(localLine);
            // Spawns a separate new line over the server.  This line is the actual line that will be used
            // in the drawing, but a temporary local line is created to have better responsiveness.
            Server_CreateNewLine(meshPrefab, position, parent, this.Owner, this, color);
            // Returns the created local line.
            return localLine;
        }



        /// <summary>
        /// Spawns a new line over the network.
        /// </summary>
        /// <param name="meshPrefab">The line prefab to spawn.</param>
        /// <param name="position">The position to spawn the line at.</param>
        /// <param name="parent">The transform that will act as the parent for this line.</param>
        /// <param name="owner">The NetworkConnection taht owns this line.</param>
        /// <param name="callbackBrush">The brush that created this line.</param>
        /// <param name="color">Tjhe color of the line.</param>
        [ServerRpc]
        private void Server_CreateNewLine(
            MeshBrushTexture meshPrefab, 
            Vector2 position, 
            Transform parent, 
            NetworkConnection owner, 
            MeshNetBrush callbackBrush, 
            Color color)
        {
            // Instantiates the new line for the client.
            MeshBrushTexture spawnedLine = Instantiate(meshPrefab, Vector2.zero, Quaternion.identity, parent);
            // Spawns the new line over the server, with it's owner set as the network connection that spawned it.
            ServerManager.Spawn(spawnedLine.gameObject, owner);
            spawnedLine.Observer_Initialize(color);
            // Stores this line in a SyncList of lines created by this brush.
            drawnObjects.Add(spawnedLine);
            // Initializes a reference to the spawned server line for the brush that created it.
            // Done this way because server communication has some inherent delay, so to avoid dropping some of the
            // user's drawing inputs, a  client placeholder is used and then loaded to created server line once
            // the callback brush recieves the InitializeDrawState call.
            InitializeDrawState(spawnedLine, callbackBrush);
        }

        /// <summary>
        /// Initializes uninitialized DrawingStates with a reference to the spawned server line.
        /// </summary>
        /// <remarks>
        /// Needs to be done this way because ServerRPCs can't return anything and the state object cannot
        /// have an ObserverRpc directly.
        /// </remarks>
        /// <param name="line">The spawned line that the state will now be linked to.</param>
        /// <param name="brush">The brush that needs it's state to have a refernece to the spawned line.</param>
        [ObserversRpc]
        private void InitializeDrawState(MeshBrushTexture line, MeshNetBrush brush)
        {
            if(this.IsOwner)
            {
                // Initializes the earlies drawing state in the queue with a reference to the spawned server line
                // to replace the localLine.
                brush.drawingStateQueue[0].InitializeLine(line, DestroyLocalLine);
                // Removes the newly initialized state from the queue.  If this state is not the current state, then
                // no references to it will exist and it will be garbate collected.
                brush.drawingStateQueue.RemoveAt(0);
            }
        }

        /// <summary>
        /// Clears all lines made by this brush.
        /// </summary>
        [ServerRpc]
        protected override void ClearLines()
        {
            foreach (var obj in drawnObjects)
            {
                ServerManager.Despawn(obj.gameObject);
            }
            drawnObjects.Clear();
            ClearLocalLines();
        }

        /// <summary>
        /// Removes a local line and ensure sthe reference to it in the local line list is removed as well.
        /// </summary>
        /// <param name="localLine">The local line to destroy.</param>
        private void DestroyLocalLine(MeshBrushTexture localLine)
        {
            localLines.Remove(localLine);
            Destroy(localLine.gameObject);
        }

        /// <summary>
        /// Clears all lines that never got initialized by the server to ensure they dont stick around.
        /// </summary>
        [ObserversRpc]
        private void ClearLocalLines()
        {
            foreach (var obj in localLines)
            {
                Destroy(obj.gameObject);
            }
            localLines.Clear();
        }

        /// <summary>
        /// Provides the lines created by this brush to the PlayerHider so they can be disabled by the admin.
        /// </summary>
        /// <param name="hider">The PlayerHider component requesting the lines.</param>
        protected override void ProvideLines(PlayerHider hider)
        {
            // Only active brushes should provide lines.
            if (gameObject.activeSelf == true)
            {
                //base.ProvideLines(hider);
                MeshBrushTexture[] lines = new MeshBrushTexture[drawnObjects.Count];
                for (int i = 0; i < drawnObjects.Count; i++)
                {
                    lines[i] = drawnObjects[i];
                }
                hider.ProvideLines(lines, base.Owner);
            }
        }

        #region Testing
        [Button]
        private void RefreshTestList()
        {
            testObjects.Clear();
            testObjects.AddRange(drawnObjects);
        }

        [Button, ServerRpc(RequireOwnership = false)]
        private void ClearDrawingTest()
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