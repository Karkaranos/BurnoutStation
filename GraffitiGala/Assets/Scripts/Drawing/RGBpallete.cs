/// Cristian Lesco
/// 2/4/25
/// TTransforms a sphere into a collor palete. Dependent on the index it gives a special color


using System;
using System.Collections;
using System.Collections.Generic;
using GraffitiGala.Drawing;
using UnityEngine;

public class RGBpallete : MonoBehaviour
{
    private MeshRenderer meshRenderer; //sphere's rendrer, used to paint the sphere itself
    public int index;
    private Color paint;


    public void paintTransfer(Color[] paintStorage)
    {
        if (index < paintStorage.Length)
        {
            paint = paintStorage[index]; // if first sphere grab 1st color in the array  
            meshRenderer = GetComponent<MeshRenderer>();
            meshRenderer.material.color = paint; // makes the sample red

        }
    }


    

    // if the sphere (paint sample) toucches with the mouse pointer in checks its index and gives a specific color
    public void OnCollisionEnter2D(Collision2D collision) 
    {

        if (collision.gameObject.name == "MeshBrush(Clone)")
        {
            
                collision.gameObject.GetComponent<MeshNetBrush>().BrushColor = paint; // makes the pointer red


        }

    }
}
