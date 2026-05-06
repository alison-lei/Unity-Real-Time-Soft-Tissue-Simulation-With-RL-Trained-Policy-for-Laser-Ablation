using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterController controller;
    public Transform cam;
    [SerializeField] float speed;
    float result_speed;
    Vector3 moveVec;
    public bool outofBounds;
    public GameObject quad;

    public AgentScript agentScript;
    
    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    public void MoveLaser(float horizontalInput, float verticalInput)
    {
        if (controller.enabled)
        {
            result_speed = speed;
            moveVec = Quaternion.Euler(cam.eulerAngles) * new Vector3(horizontalInput, 0.0f, verticalInput);
            moveVec.y = 0.0f;
            if (moveVec != Vector3.zero)
            {
                moveVec = moveVec.normalized;
            }
            controller.Move(moveVec * Time.deltaTime * result_speed);
        }
    }


    public void homePosition()
    {
        transform.localPosition = quad.transform.localPosition + new Vector3(-1.0f, 5.8f, 0.0f);
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("boundary"))
        {
            outofBounds = true;
        }

    }

    public void resetBoundsFlag()
    {
        outofBounds = false;
    }

}
