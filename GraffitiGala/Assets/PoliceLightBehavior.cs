using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraffitiGala
{
    public class PoliceLightBehavior : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            StartCoroutine(DeleteSelf());

        }

        IEnumerator DeleteSelf()
        {
            float waitTime = FindObjectOfType<PlayTimer>().WarningTime+2;
            yield return new WaitForSeconds(waitTime);
            Destroy(gameObject);
        }
    }
}