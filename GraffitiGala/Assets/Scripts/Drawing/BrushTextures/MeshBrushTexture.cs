/*************************************************
Brandon Koederitz
1/30/2025
1/30/2025
Control script for instantiated paint prefabs that use the Mesh drawing system.
FishNet, InputSystem, NaughtyAttributes
***************************************************/

using FishNet.Object;
using UnityEngine;

namespace GraffitiGala.Drawing
{
    public class MeshBrushTexture : NetworkBehaviour
    {

        #region vars
        #region CONSTS
        private Vector3 SCREEN_NORMAL = new Vector3(0f, 0f, -1f);
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

        /// <summary>
        /// Creates a new mesh that this object will use to render it's line.
        /// </summary>
        /// <param name="pressure"> The pressure of the pen when this line was created.</param>
        /// <param name="position">The position that this line will start from.</param>
        public void InitializeMesh(float pressure, Vector2 position)
        {
            // Creates a new set of mesh information
            Vector3[] verticies = new Vector3[4];
            Vector2[] uv = new Vector2[4];
            int[] triangles = new int[6];

            float halfWidth = GetWidth(pressure) / 2;

            // Uses half the width to create a simple quad whose width is equal to the line width based on pressure.
            verticies[0] = position + new Vector2(-halfWidth, halfWidth); // Top Left
            verticies[1] = position + new Vector2(-halfWidth, -halfWidth); // Bottom Left
            verticies[2] = position + new Vector2(halfWidth, halfWidth); // Top Right
            verticies[3] = position + new Vector2(halfWidth, -halfWidth); // Bottom Right

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

            // Sets the mesh's values to the ones calculated.
            SetMesh(verticies, uv, triangles);
        }

        /// <summary>
        /// Configures this line with the correct color and material for the brush.
        /// </summary>
        /// <param name="color">The color of this line.</param>
        /// <param name="referenceMaterial">The material this line will use.</param>
        public void ConfigureAppearance(Color color)
        {
            // Creates a new temporary material that is a clone of the reference material.
            Material thisMaterial = new Material(referenceMaterial);
            // Sets the color of the new clone material based on the passed in color.
            thisMaterial.SetColor("_Color", color);
            // Applies the cloned material to this object's mesh renderer.
            meshRenderer.material = thisMaterial;
        }

        /// <summary>
        /// Configures this line with the correct color and material for the brush from the server across the
        /// network.
        /// </summary>
        /// <param name="color">The color of this line.</param>
        [ObserversRpc]
        public void Observer_ConfigureAppearance(Color color)
        {
            ConfigureAppearance(color);
        }

        #region Add Point
        /// <summary>
        /// Adds a point to this line.
        /// </summary>
        /// <param name="position">The position of the new point.</param>
        /// <param name="drawDirection">The direction that this point was drawn from.</param>
        /// <param name="pressure">The pen pressure when this point was added.</param>
        public void AddPoint(Vector2 position, Vector2 drawDirection, float pressure)
        {
            // Adds a point to this line locally.
            Local_AddPoint(position, drawDirection, pressure);
            // Adds a point to this line for the entire server if this object is set up as a networked object.
            if(isNetworked)
            {
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
        private void Local_AddPoint(Vector2 position, Vector2 drawDirection, float pressure)
        {
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
            Vector2 drawParallel = Vector3.Cross(drawDirection.normalized, SCREEN_NORMAL) * (GetWidth(pressure) / 2);
            //Debug.Log(drawDirection);
            //Debug.Log(drawParallel);
            // Calculates the new vertex positions.
            Vector2 newTopPosition = position + drawParallel;
            Vector2 newBottomPosition = position - drawParallel;

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
        private void Server_AddPoint(Vector2 position, Vector2 drawDirection, float pressure)
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
        private void Client_AddPoint(Vector2 position, Vector2 drawDirection, float pressure)
        {
            Local_AddPoint(position, drawDirection, pressure);
        }
        #endregion

        #region Network Initialization
        /// <summary>
        /// Sets up this line to send information across the network.
        /// </summary>
        /// <param name="localReferenceLine">
        /// The local placeholder line that was used to keep graphics updated that should be loaded to this line.
        /// </param>
        public void InitializeAsNetworked(MeshBrushTexture localReferenceLine)
        {
            // Creates a new set of mesh information based on the local reference line.
            Vector3[] verticies = localReferenceLine.mesh.vertices;
            Vector2[] uv = localReferenceLine.mesh.uv;
            int[] triangles = localReferenceLine.mesh.triangles;

            // Sets this object's mesh to be the same as the local reference line's
            SetMesh(verticies, uv, triangles);

            // Sets this mesh to reflect the client mesh for all mesh lines across the network.
            Server_InitializeAsNetworked(verticies, uv, triangles);

            isNetworked = true;
            // Destroy the local reference line as it is no longer needed, newly initialized network line will
            // be updated instead.
            Destroy(localReferenceLine.gameObject);
        }

        /// <summary>
        /// Initializes a pre-established mesh to Mesh lines across the network.
        /// </summary>
        [ServerRpc]
        private void Server_InitializeAsNetworked(Vector3[] verticies, Vector2[] uv, int[] triangles)
        {
            Client_InitializeAsNetworked(verticies, uv, triangles);
        }

        /// <summary>
        /// Initializes a pre-established mesh to this Mesh line as a client.
        /// </summary>
        /// <param name="verticies">The verticies of the mesh.</param>
        /// <param name="uv">The UVs of the mesh.</param>
        /// <param name="triangles">The triangles of the mesh.</param>
        [ObserversRpc(ExcludeOwner = true)]
        private void Client_InitializeAsNetworked(Vector3[] verticies, Vector2[] uv, int[] triangles)
        {
            SetMesh(verticies, uv, triangles);
        }

        /// <summary>
        /// Sets this object's mesh data values.
        /// </summary>
        /// <param name="vertices">The verticies of the mesh.</param>
        /// <param name="uv">The UVs of the mesh.</param>
        /// <param name="triangles">The triangles of the mesh.</param>
        private void SetMesh(Vector3[] vertices, Vector2[] uv, int[] triangles)
        {
            // Creates a new mesh if the mesh is null.
            if(mesh == null)
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
        #endregion
    }

}