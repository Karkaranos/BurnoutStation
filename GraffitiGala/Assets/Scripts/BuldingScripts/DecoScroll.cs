using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecorationScroll : MonoBehaviour
{

    [SerializeField] GameObject point1;
    [SerializeField] GameObject point2;
    [SerializeField] private float speed;
    private int currentIndex;


    void Start()
    {
        currentIndex = 0;
       // transform.position = point1.transform.position;
    }


    void Update()
    {
        if (Vector3.Distance(transform.position, point2.transform.position) < 0.1f) // take the distance between current location and the index (next point)
        {
   
             transform.position = point1.transform.position;
             
        }
        transform.position = Vector3.MoveTowards(transform.position, point2.transform.position, speed
                 * Time.deltaTime);
    }
}
