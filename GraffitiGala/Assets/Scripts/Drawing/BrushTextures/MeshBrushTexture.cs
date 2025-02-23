/*************************************************
Brandon Koederitz
1/30/2025
2/22/2025
Control script for instantiated paint prefabs that use the Mesh drawing system.
FishNet, InputSystem, NaughtyAttributes
***************************************************/
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using System.Collections;
using UnityEngine;

namespace GraffitiGala.Drawing
{
    public class MeshBrushTexture : NetworkBehaviour
    {

        #region vars
        #region CONSTS
        private static readonly Vector3 SCREEN_NORMAL = new Vector3(0f, 0f, -1f);
        #endregion
        [Header("Component References")]
        [SerializeReference] internal MeshFilter meshFilter;
        [SerializeReference] internal MeshRenderer meshRenderer;
        [Header("Line settings")]
        [SerializeReference, Tooltip("The material to be used by this line. Do not set material in the MeshRenderer.")]
        private Material referenceMaterial;
        [SerializeReference, Tooltip("The width of this line when the player is applying" +
            " the minimum amount of pressure to the pen.  If this value is larger than" +
            " Max Pressure Width, then the line will appear larger when less pressure is applied.")]
        private float minPressureWidth;
        [SerializeReference, Tooltip("The width of this line when the player is applying" +
            " the maximum amount of pressure to the pen.  If this value is smaller than" +
            " Min Pressure Width, then the line will appear smaller when more pressure is applied.")]
        private float maxPressureWidth;

        private Mesh mesh;
        private bool isNetworked;
        private bool isQuad;
        private Vector3 originPoint;

        private static bool isSetup;
        private static Coroutine newClientRoutine;
        #endregion


        #region Mesh Requesting
        /// <summary>
        /// Fetches the mesh from the server when this client connects.
        /// </summary>
        public override void OnStartClient()
        {
            if(!isSetup)
            {
                Server_RequestMesh(base.Owner);
                Debug.Log(" Client " + InstanceFinder.ClientManager.Connection + " Requesting Mesh from Owner " + base.Owner.ClientId);
            }
            // Sets this client as not new at the end of frame.
            if (newClientRoutine == null && !isSetup)
            {
                newClientRoutine = StartCoroutine(InitNewClient());
            }
        }

        /// <summary>
        /// Resets this client to set up the next time it connects to as a client.
        /// </summary>
        public override void OnStopClient()
        {
            isSetup = false;
        }

        /// <summary>
        /// Provides a mesh from the server to a connection that lacks one.
        /// </summary>
        /// <param name="requestingConnection">The network connection that is requesting a mesh.</param>
        /// <param name="ownerConnection">The connection that owns the line object to request a mesh from.</param>
        [ServerRpc(RequireOwnership = false)]
        private void Server_RequestMesh(NetworkConnection ownerConnection, NetworkConnection requestingConnection = null)
        {
            Debug.Log($"Connection {requestingConnection.ClientId} is requesting a mesh from connection {ownerConnection.ClientId}");
            Target_RequestMesh(ownerConnection, requestingConnection);
        }

        /// <summary>
        /// Gives a mesh to a specific network connection that doesn't have one.
        /// </summary>
        /// <param name="ownerConnection">The network connection to provide a mesh for this object for.</param>
        /// <param name="requestingConnection">The connection requesting a mesh.</param>
        [TargetRpc]
        private void Target_RequestMesh(NetworkConnection ownerConnection, NetworkConnection requestingConnection)
        {
            Debug.Log($"Connection {ownerConnection.ClientId} is providing a mesh for connection {requestingConnection.ClientId}");
            GetMeshInfo(mesh, out Vector3[] verticies, out Vector2[] uv, out int[] triangles);
            Server_ProvideMesh(requestingConnection, verticies, uv, triangles);
        }

        /// <summary>
        /// Provides a mesh to a different target network connection.
        /// </summary>
        /// <param name="requestingConnection">The connection to provide a mesh to.</param>
        /// <param name="vertices">The verticies of the mesh.</param>
        /// <param name="uv">The UVs of the mesh.</param>
        /// <param name="triangles">The triangles of the mesh.</param>
        [ServerRpc]
        private void Server_ProvideMesh(NetworkConnection requestingConnection, Vector3[] vertices, Vector2[] uv, 
            int[] triangles)
        {
            Debug.Log($"Connection {requestingConnection.ClientId} is recieving a mesh.");
            Target_ProvideMesh(requestingConnection, vertices, uv, triangles);
        }

        /// <summary>
        /// Recieves a provide mesh from the owner connection from across the server.
        /// </summary>
        /// <param name="requestingConnection">This network connection.</param>
        /// <param name="vertices">The verticies of the mesh.</param>
        /// <param name="uv">The UVs of the mesh.</param>
        /// <param name="triangles">The triangles of the mesh.</param>
        [TargetRpc]
        private void Target_ProvideMesh(NetworkConnection requestingConnection, Vector3[] vertices, Vector2[] uv,
            int[] triangles)
        {
            Debug.Log($"Connection {requestingConnection.ClientId} is recieving a mesh.");
            SetMesh(vertices, uv, triangles);
        }

        /// <summary>
        /// Sets the newClient variable for this client to false once the strokes have requested a mesh from the 
        /// server.
        /// </summary>
        /// <returns>Coroutine.</returns>
        private IEnumerator InitNewClient()
        {
            yield return new WaitForEndOfFrame();
            isSetup |= true;
        }
        #endregion

        /// <summary>
        /// Gets the width of a line based on a given pressure.
        /// </summary>
        /// <param name="pressure">The pressure to use when caluclating line width.</param>
        /// <returns>The width that the line should be.</returns>
        private float GetWidth(float pressure)
        {
            return Mathf.Lerp(minPressureWidth, maxPressureWidth, pressure);
        }

        #region Set Mesh
        /// <summary>
        /// Sets this object's mesh data values.
        /// </summary>
        /// <param name="vertices">The verticies of the mesh.</param>
        /// <param name="uv">The UVs of the mesh.</param>
        /// <param name="triangles">The triangles of the mesh.</param>
        private void SetMesh(Vector3[] vertices, Vector2[] uv, int[] triangles)
        {
            // Creates a new mesh if the mesh is null.
            if (mesh == null)
            {
                mesh = new Mesh();
                mesh.MarkDynamic();
                meshFilter.mesh = mesh;
            }

            // Sets the mesh data.
            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
        }

        /// <summary>
        /// Sets the mesh of all lines across the network
        /// </summary>
        /// /// <param name="verticies">The verticies of the mesh.</param>
        /// <param name="uv">The UVs of the mesh.</param>
        /// <param name="triangles">The triangles of the mesh.</param>
        [ServerRpc]
        private void Server_SetMesh(Vector3[] verticies, Vector2[] uv, int[] triangles)
        {
            Client_SetMesh(verticies, uv, triangles);
        }

        /// <summary>
        /// Sets this client's mesh to a set of given values.
        /// </summary>
        /// <param name="verticies">The verticies of the mesh.</param>
        /// <param name="uv">The UVs of the mesh.</param>
        /// <param name="triangles">The triangles of the mesh.</param>
        [ObserversRpc(ExcludeOwner = true)]
        private void Client_SetMesh(Vector3[] verticies, Vector2[] uv, int[] triangles)
        {
            SetMesh(verticies, uv, triangles);
        }
        #endregion

        /// <summary>
        /// Splits a mesh into it's component info.
        /// </summary>
        /// <param name="mesh">The mesh to get the info of.</param>
        /// <param name="verticies">The verticies array of the mesh.</param>
        /// <param name="uv">The UV array of the mesh.</param>
        /// <param name="triangles">The triangles array of the mesh.</param>
        private static void GetMeshInfo(Mesh mesh, out Vector3[] verticies, out Vector2[] uv, out int[] triangles)
        {
            verticies = mesh.vertices;
            uv = mesh.uv;
            triangles = mesh.triangles;
        }

        #region Initialization
        /// <summary>
        /// Creates a new mesh that this object will use to render it's line.
        /// </summary>
        /// <param name="pressure"> The pressure of the pen when this line was created.</param>
        /// <param name="position">The position that this line will start from.</param>
        public void Local_InitializeMesh(float pressure, Vector3 position)
        {
            // Creates a new set of mesh information
            Vector3[] verticies = new Vector3[4];
            Vector2[] uv = new Vector2[4];
            int[] triangles = new int[6];

            float halfWidth = GetWidth(pressure) / 2;

            // Uses half the width to create a simple quad whose width is equal to the line width based on pressure.
            verticies[0] = position + new Vector3(-halfWidth, halfWidth, 0); // Top Left
            verticies[1] = position + new Vector3(-halfWidth, -halfWidth, 0); // Bottom Left
            verticies[2] = position + new Vector3(halfWidth, halfWidth, 0); // Top Right
            verticies[3] = position + new Vector3(halfWidth, -halfWidth, 0); // Bottom Right

            // Sets the UVs of this mesh to show the entire material inside the quad.
            uv[0] = new Vector2(0, 1);
            uv[1] = new Vector2(0, 0);
            uv[2] = new Vector2(1, 1);
            uv[3] = new Vector2(1, 0);

            // Sets up the first triangle
            triangles[0] = 1;
            triangles[1] = 0;
            triangles[2] = 2;
            // Sets up the second triangle
            triangles[3] = 1;
            triangles[4] = 2;
            triangles[5] = 3;
            // Tells this component that the current mesh is only a quad, and that it needs to be updated to be fully
            // line compatible.
            isQuad = true;
            originPoint = position;

            // Sets the mesh's values to the ones calculated.
            SetMesh(verticies, uv, triangles);
        }

        /// <summary>
        /// Sets up this line to send information across the network.
        /// </summary>
        /// <param name="localReferenceLine">
        /// The local placeholder line that was used to keep graphics updated that should be loaded to this line.
        /// </param>
        public void InitializeAsNetworked(MeshBrushTexture localReferenceLine, ClearLineCallback clc)
        {
            // Creates a new set of mesh information based on the local reference line.
            GetMeshInfo(localReferenceLine.mesh, out Vector3[] verticies, out Vector2[] uv, out int[] triangles);
            // Sets this object's mesh to be the same as the local reference line's
            SetMesh(verticies, uv, triangles);

            // Sets this mesh to reflect the client mesh for all mesh lines across the network.
            Server_InitializeAsNetworked(verticies, uv, triangles, localReferenceLine.isQuad, 
                localReferenceLine.originPoint);

            // Sets information of this line's current state based on the local reference line's state.
            isQuad = localReferenceLine.isQuad;
            originPoint = localReferenceLine.originPoint;

            isNetworked = true;
            // Destroy the local reference line as it is no longer needed, newly initialized network line will
            // be updated instead.
            clc(localReferenceLine);
            //Destroy(localReferenceLine.gameObject);
        }

        /// <summary>
        /// Initializes this line as networked over the server.
        /// </summary>
        /// <remarks>
        /// Sets the mesh of spawned line objects across the server and gives them values that they will need
        /// for continued updates.
        /// </remarks>
        /// <param name="verticies">The verticies of the mesh.</param>
        /// <param name="uv">The UVs of the mesh.</param>
        /// <param name="triangles">The triangles of the mesh.</param>
        /// <param name="isQuad">Whether this mesh is still just a quad or if it has had points added to it.</param>
        /// <param name="originPoint">The origion point of the line.</param>
        [ServerRpc]
        private void Server_InitializeAsNetworked(Vector3[] verticies, Vector2[] uv, int[] triangles, bool isQuad, 
            Vector3 originPoint)
        {
            Client_SetMesh(verticies, uv, triangles);
            Client_InitializeAsNetworked(isQuad, originPoint);
        }

        /// <summary>
        /// Sets this line up as networked for other clients.
        /// </summary>
        /// <param name="isQuad">Whether this mesh is still just a quad or if it has had points added to it.</param>
        /// <param name="originPoint">The origion point of the line.</param>
        [ObserversRpc(ExcludeOwner = true, BufferLast = true)]
        private void Client_InitializeAsNetworked(bool isQuad, Vector3 originPoint)
        {
            this.isQuad = isQuad;
            this.originPoint = originPoint;
            isNetworked = true;
        }

        /// <summary>
        /// Configures this line with the correct color and material for the brush.
        /// </summary>
        /// <param name="color">The color of this line.</param>
        /// <param name="z">The z position that this mesh renders at.</param>
        public void Initialize(Color color)
        {
            // Creates a new temporary material that is a clone of the reference material.
            Material thisMaterial = new Material(referenceMaterial);
            // Sets the color of the new clone material based on the passed in color.
            thisMaterial.SetColor("_Color", color);
            // Applies the cloned material to this object's mesh renderer.
            meshRenderer.material = thisMaterial;
            // Sets NewClient to true, as if a line is being spawned locally on a client then it is definitely
            // not new.
            isSetup |= true;
        }

        /// <summary>
        /// Configures this line with the correct color and material for the brush from the server across the
        /// network.
        /// </summary>
        /// <param name="color">The color of this line.</param>
        /// <param name="z">The z position that this mesh renders at.</param>
        [ObserversRpc(BufferLast = true)]
        public void Observer_Initialize(Color color)
        {
            Initialize(color);
        }
        #endregion

        #region Add Point
        /// <summary>
        /// Adds a point to this line.
        /// </summary>
        /// <param name="position">The position of the new point.</param>
        /// <param name="drawDirection">The direction that this point was drawn from.</param>
        /// <param name="pressure">The pen pressure when this point was added.</param>
        public void AddPoint(Vector3 position, Vector2 drawDirection, float pressure)
        {
            // Adds a point to this line locally.
            Local_AddPoint(position, drawDirection, pressure);
            // Adds a point to this line for the entire server if this object is set up as a networked object.
            if(isNetworked)
            {
                // Updates the entire mesh to allow for new connections to load lines correctly.
                //GetMeshInfo(mesh, out Vector3[] verticies, out Vector2[] uv, out int[] triangles);
                //Server_SetMesh(verticies, uv, triangles);
                Server_AddPoint(position, drawDirection, pressure);
            }

        }

        /// <summary>
        /// Adds a point to this line locally.
        /// </summary>
        /// <remarks>
        /// I distinguish between local and client here so that locally, the person whose drawing can see their
        /// drawing being updated in real time without dealing with server lag.  The result is (theoretically) the
        /// same on both ends.
        /// This function contains the actual code that adds points to the mesh.
        /// </remarks>
        /// <param name="position">The position of the new point.</param>
        /// <param name="drawDirection">The direction that this point was drawn from.</param>
        /// <param name="pressure">The pen pressure when this point was added.</param>
        private void Local_AddPoint(Vector3 position, Vector2 drawDirection, float pressure)
        {
            if(isQuad)
            {
                RotateMesh(mesh, originPoint, drawDirection);
                isQuad = false;
            }

            // Define a new set of array to update the mesh values.
            Vector3[] verticies = new Vector3[mesh.vertices.Length + 2]; // Add two additional verticies.
            Vector2[] uv = new Vector2[mesh.uv.Length + 2]; // Add two additional UV positions.
            int[] triangles = new int[mesh.triangles.Length + 6]; // Add 6 new triangle indicies, 3 for each polygon

            // Copy data from the mesh to the new temporary lists.
            mesh.vertices.CopyTo(verticies, 0);
            mesh.uv.CopyTo(uv, 0);
            mesh.triangles.CopyTo(triangles, 0);

            // Assigns variables to hold the 4 indicies needed to construct the new line point.
            int lastTopIndex = (verticies.Length - 4) + 0;
            int lastBottomIndex = (verticies.Length - 4) + 1;
            int newTopIndex = (verticies.Length - 4) + 2;
            int newBottomIndex = (verticies.Length - 4) + 3;

            // Calculates a vector that will be used to calculate the vertex positions based on the given point 
            // position.
            Vector3 drawParallel = Vector3.Cross(drawDirection.normalized, SCREEN_NORMAL) * (GetWidth(pressure) / 2);
            //Debug.Log(drawDirection);
            //Debug.Log(drawParallel);
            // Calculates the new vertex positions.
            Vector3 newTopPosition = position + drawParallel;
            Vector3 newBottomPosition = position - drawParallel;

            // Adds the new top and bottom positions to the verticies array.
            verticies[newTopIndex] = newTopPosition;
            verticies[newBottomIndex] = newBottomPosition;

            // Sets the UVs of the last indicies so that they load the middle of the material.
            uv[lastTopIndex] = new Vector2(0.5f, 1f);
            uv[lastBottomIndex] = new Vector2(0.5f, 0f);

            // Sets the UVs of the new indicies so that they load the edges of the material.
            uv[newTopIndex] = new Vector2(1f, 1f);
            uv[newBottomIndex] = new Vector2(1f, 0f);

            // Set up the triangles.
            int tIndex = triangles.Length - 6;
            
            triangles[tIndex + 0] = lastBottomIndex;
            triangles[tIndex + 1] = lastTopIndex;
            triangles[tIndex + 2] = newTopIndex;

            triangles[tIndex + 3] = lastBottomIndex;
            triangles[tIndex + 4] = newTopIndex;
            triangles[tIndex + 5] = newBottomIndex;

            SetMesh(verticies, uv, triangles);
        }

        /// <summary>
        /// Adds a point to all lines across the network.
        /// </summary>
        /// <param name="position">The position of the new point.</param>
        /// <param name="drawDirection">The direction that this point was drawn from.</param>
        /// <param name="pressure">The pen pressure when this point was added.</param>
        [ServerRpc]
        private void Server_AddPoint(Vector3 position, Vector2 drawDirection, float pressure)
        {
            Client_AddPoint(position, drawDirection, pressure);
        }

        /// <summary>
        /// Adds a line to this non-owner client.
        /// </summary>
        /// <param name="position">The position of the new point.</param>
        /// <param name="drawDirection">The direction that this point was drawn from.</param>
        /// <param name="pressure">The pen pressure when this point was added.</param>
        [ObserversRpc(ExcludeOwner = true)]
        private void Client_AddPoint(Vector3 position, Vector2 drawDirection, float pressure)
        {
            Local_AddPoint(position, drawDirection, pressure);
        }

        /// <summary>
        /// Rotates a mesh around a given pivot point.
        /// </summary>
        /// <param name="mesh">The mesh to rotate.</param>
        /// <param name="pivotPoint">The pivot point to rotate the mesh around.</param>
        /// <param name="newDirection">The vector that the mesh should be facing in.</param>
        private static void RotateMesh(Mesh mesh, Vector3 pivotPoint, Vector3 newDirection)
        {
            //Debug.Log("Rotating mesh");
            Vector3[] verticies = mesh.vertices;
            float additiveAngle = MathHelpers.VectorToAngle(newDirection);

            for(int i = 0; i < verticies.Length; i++)
            {
                Vector2 relativePosition = verticies[i] - pivotPoint;
                // Adds the angle specified by newDirection to the angle of the current verrticie's relative position
                // from the pivot point in polar coordinates.
                float angle = MathHelpers.VectorToAngle(relativePosition);
                angle = angle + additiveAngle;
                // Sets the verticie's relative position in cartesian coordinates equal to the newly calculated
                // position in polar coordinates.
                relativePosition = MathHelpers.AngleToUnitVector(angle) * relativePosition.magnitude;
                verticies[i] = (Vector3)relativePosition + pivotPoint;
            }

            mesh.vertices = verticies;
        }
        #endregion
    }

}