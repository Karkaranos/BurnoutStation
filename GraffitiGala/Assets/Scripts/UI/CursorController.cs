/*************************************************
Brandon Koederitz
3/26/2025
3/26/2025
Snycronizes a cursor position in the UI to this game object's position.
***************************************************/
using FishNet.Object;
using UnityEngine;

namespace GraffitiGala.UI
{
    public class CursorController : NetworkBehaviour
    { 
        [SerializeField] private RectTransform ownerCursorPrefab;
        [SerializeField] private RectTransform otherCursorPrefab;

        private static Transform cursorParent;

        private RectTransform cursorPrefab;
        private RectTransform cursorTransform;
        
        /// <summary>
        /// Find the object that will act as the parent of cursors on awake.
        /// </summary>
        private void Awake()
        {
            // Need to null check here because this line will fail on builds where the canvas is hidden.
            GameObject go = GameObject.FindGameObjectWithTag("CursorParent");
            if (go != null)
            {
                cursorParent = go.transform;
            }
        }
        /// <summary>
        /// Set the prefab this object uses in OnStartClient as this is when IsOwner is initialized.
        /// </summary>
        public override void OnStartClient()
        {
            cursorPrefab = base.IsOwner ? ownerCursorPrefab : otherCursorPrefab;
            // Call OnEnable here to make sure the cursor spawns.
            OnEnable();
        }
        /// <summary>
        /// Create and destroy the cursor for this object when it is enabled/disabled.
        /// </summary>
        private void OnEnable()
        {
            if (cursorTransform == null && cursorParent != null && cursorPrefab != null)
            {
                cursorTransform = Instantiate(cursorPrefab, cursorParent);
            }
        }
        private void OnDisable()
        {
            if (cursorTransform != null)
            {
                Destroy(cursorTransform.gameObject);
                cursorTransform = null;
            }
        }

        /// <summary>
        /// Continually updates the cursor's position to match this object's position in world space.
        /// </summary>
        /// <remarks>
        /// Wish I could do this differently like in an Input Performed event, but since non-owner clients need to
        /// update cursors automatically, this seems like the best way to do it.
        /// </remarks>
        private void Update()
        {
            if (cursorTransform != null)
            {
                cursorTransform.position = Camera.main.WorldToScreenPoint(transform.position);
            }
        }
    }
}
