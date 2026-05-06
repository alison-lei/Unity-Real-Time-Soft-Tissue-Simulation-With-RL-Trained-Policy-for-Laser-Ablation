using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    public CharacterController scalpelObject;
    public GameObject scalpel;
    public Transform scalpelTransform;

    void Start()
    {
        OnStart();        
    }

    void OnStart()
    {
        scalpelTransform.position = new Vector3(0, 13, 0);
        scalpelObject.enabled = true;
    }    
}
