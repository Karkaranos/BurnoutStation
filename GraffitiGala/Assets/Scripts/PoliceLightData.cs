/*************************************************
Author:                     Cade Naylor
Creation Date:              4/3/2025
Modified Date:              4/3/2025
Summary:                    Contains additional data for police light particle systems
***************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ParticleSystemJobs;
using UnityEngine.UI;

public class PoliceLightData : MonoBehaviour
{
    private ParticleSystem ps;
    [SerializeField, Range(.01f,1)] private float intensityMultiplier = 1;
    // Start is called before the first frame update
    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        StartCoroutine(ColorShift());
    }

    private IEnumerator ColorShift()
    {
        while(true)
        {
            ps.startColor = new Color(255, 0, 0, .7f*intensityMultiplier);
            yield return new WaitForSeconds(Random.RandomRange(.1f, .5f));
            ps.startColor = new Color(0, 0, 255, 1f * intensityMultiplier);
            yield return new WaitForSeconds(Random.RandomRange(.1f, .5f));
        }
    }

}
