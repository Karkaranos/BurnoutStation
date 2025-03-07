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
    [SerializeField] private GameObject self;




    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    public void Option1()
    {
        
        self.SetActive(false);

    }
    public void Option2()
    {
      self.SetActive(false);
        self.SetActive(false);

    }
    public void Option3()
    {
        self.SetActive(false);

    }
}
