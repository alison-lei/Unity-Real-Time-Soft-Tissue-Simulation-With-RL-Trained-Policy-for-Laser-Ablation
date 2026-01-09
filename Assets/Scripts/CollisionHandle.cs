using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionHandle : MonoBehaviour
{

    [Range(0.0f, 10.0f)] public float pressure = 10.0f;
    public ArrayList ToolTouches = new ArrayList();
    public float toolDepth;
    public MeshHandle meshHandle;
    public GameObject quad;


    void Start()
    {
        // toolDepth = 13.0f;
        toolDepth = 6.0f;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MassUnit"))
        {
            Vector3 p = other.gameObject.transform.position;
            if (transform.position.y - toolDepth < p.y)
            {
                ToolTouches.Add(new Vector3(p.x, p.z, pressure));
            }
        }
            
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("MassUnit"))
        {
            Vector3 p = other.gameObject.transform.position;
            if (transform.position.y - toolDepth < p.y)
            {
                ToolTouches.Add(new Vector3(p.x, p.z, pressure));
            }
        }
    }
}
        // if (other.CompareTag("MassUnit"))
        // {
        //     Vector3 p = other.gameObject.transform.localPosition - quad.transform.localPosition;
        //     if ((transform.localPosition - quad.transform.localPosition).y - toolDepth < p.y)
        //     {
        //         ToolTouches.Add(new Vector3(p.x, p.z, pressure));
        //     }
        // }