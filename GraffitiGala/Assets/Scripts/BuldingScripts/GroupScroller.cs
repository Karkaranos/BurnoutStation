/*************************************************
Brandon Koederitz
3/9/2025
3/8/2025
Scrolls an entire group of objects at the same speed.  Also applies a color tint to the sprites.
***************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraffitiGala.City
{
    public class GroupScroller : MonoBehaviour
    {
        [SerializeField] private Transform point1;
        [SerializeField] private Transform point2;
        [SerializeField] private SpriteRenderer[] decorationsToScroll;
        [SerializeField] private float speed;
        [SerializeField] private Color colorTint;

        /// <summary>
        /// Applies the color tint to all decorations
        /// </summary>
        private void Awake()
        {
            foreach(var item in decorationsToScroll)
            {
                item.color = colorTint;
            }
        }

        /// <summary>
        /// Scrolls all the decorations.
        /// </summary>
        void Update()
        {
            foreach(var item in decorationsToScroll)
            {
                Transform trans = item.transform;
                Vector3 pos = trans.position;
                if (trans.position.x - point2.transform.position.x < 0.1f)
                {
                    pos.x = point1.transform.position.x;
                }
                pos.x = Mathf.MoveTowards(pos.x, point2.transform.position.x, speed * Time.deltaTime);
                trans.position = pos;
            }
        }
    }

}