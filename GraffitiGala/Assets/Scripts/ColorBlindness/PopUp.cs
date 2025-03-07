using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PopUp : MonoBehaviour
{
    [SerializeField] private GameObject nextPopUp;




    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    public void Option1()
    {
        
        gameObject.SetActive(false);
        nextPopUp.SetActive(true);

    }
    public void Option2()
    {
        gameObject.SetActive(false);


    }
    
}
