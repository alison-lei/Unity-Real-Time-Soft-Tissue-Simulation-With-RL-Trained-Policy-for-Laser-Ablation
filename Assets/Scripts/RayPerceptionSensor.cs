using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayPerceptionSensor : MonoBehaviour
{
    [Range(0.0f, 10.0f)]
    public float pressure = 10.0f;
    public ArrayList ToolTouches = new ArrayList();
    public float toolDepth;
    // public AgentScript agentScript;
    

    void Start()
    {
        toolDepth = 3.0f;
    }

    public float rayLength = 0.5f;
    public LayerMask layerMask;

    void FixedUpdate()
    {
        CastRays();
    }


    public void CastRays()
    {
        Vector3 origin = new Vector3(transform.position.x, transform.position.y + 2, transform.position.z);
        Vector3 rayDirection = -transform.up;
        Ray ray = new Ray(origin, rayDirection);
        RaycastHit hit;
        Debug.DrawRay(origin, rayDirection * rayLength, Color.red);
        if (Physics.Raycast(ray, out hit, rayLength, layerMask, QueryTriggerInteraction.Collide))
        {
            if (hit.distance <= rayLength)
            {
                Vector3 p = hit.collider.gameObject.transform.position;
                ToolTouches.Add(new Vector3(p.x, p.z, pressure));
                // if (transform.position.y < p.y + toolDepth)
                // {
                //     ToolTouches.Add(new Vector3(p.x, p.z, pressure));
                // }

            }
        }
    }

    // void OnTriggerEnter(Collider other)
    // {
    //     if (other.CompareTag("boundary"))
    //     {
    //         agentScript.rewardFunction(-5f);
    //     }
        
    // }


}
