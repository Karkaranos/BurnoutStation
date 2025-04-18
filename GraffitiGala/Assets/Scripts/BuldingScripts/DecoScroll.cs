using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecorationScroll : MonoBehaviour
{

    [SerializeField] GameObject point1;
    [SerializeField] GameObject point2;
    [SerializeField] private float speed;
    //private int currentIndex;


    //void Start()
    //{
    //    currentIndex = 0;
    //   // transform.position = point1.transform.position;
    //}


    void Update()
    {
        //if (Vector3.Distance(transform.position, point2.transform.position) < 0.1f) // take the distance between current location and the index (next point)
        //{
   
        //     transform.position = point1.transform.position;
             
        //}
        //transform.position = Vector3.MoveTowards(transform.position, point2.transform.position, speed
        //         * Time.deltaTime);

        // Had to change it so that decos only scroll along the x axis.  Having to configure individual points for each element would have been
        // very tedious to acccount for the changes in y pos.
        Vector3 pos = transform.position;
        if (transform.position.x - point2.transform.position.x < 0.1f)
        {
            pos.x = point1.transform.position.x;
        }
        pos.x = Mathf.MoveTowards(pos.x, point2.transform.position.x, speed * Time.deltaTime);
        transform.position = pos;
    }
}
