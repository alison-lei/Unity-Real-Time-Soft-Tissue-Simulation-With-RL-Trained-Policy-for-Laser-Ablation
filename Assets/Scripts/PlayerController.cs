using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //private Rigidbody playerRb;
    // the playercontroller script interacts with charactercontroller
    private CharacterController controller;
    public Transform cam;
    [SerializeField] float speed;
    // float horizontalInput, verticalInput;
    float result_speed;
    // float yMoveVec;
    Vector3 moveVec;
    // [SerializeField] float yRotateAngle; //1
    public bool outofBounds;
    public GameObject quad;

    public AgentScript agentScript;
    // public Vector3 scalpeloffset;
    // public Vector3 originoffset;
    // public Vector3 envOrigin;

    // void Awake()
    // {
    //     scalpeloffset = transform.position - Environment.transform.position;
    //     originoffset = Vector3.zero - Environment.transform.position;
    //     envOrigin = Environment.transform.position;
    // }

    // Start is called before the first frame update
    void Start()
    {
        //playerRb = GetComponent<Rigidbody>();
        controller = GetComponent<CharacterController>();
        // Debug.Log(quad.transform.localPosition + new Vector3(0f, 5.8f, 5f));
        
        // homePosition();
        // homePosition();
        // horizontalInput = 0.0f;
        // verticalInput = 0.0f;
    }

    // 3 up down movement and can cut mesh
    // public void MoveLaser(float horizontalInput, float yMoveVec, float verticalInput)
    // {
    //     if (controller.enabled)
    //     {
    //         result_speed = speed;
    //         // rotates movement vector to match the camera rotation, so always move forward relative to camera and not with respect to world
    //         moveVec = Quaternion.Euler(cam.eulerAngles) * new Vector3(horizontalInput, 0.0f, verticalInput);
    //         moveVec.y = yMoveVec;
    //         // moveVec.y = Mathf.Clamp(yMoveVec, -0.001f, 0.001f);
    //         if (moveVec != Vector3.zero)
    //         {
    //             moveVec = moveVec.normalized;
    //         }
    //         Vector3 resultingMovVec = moveVec * Time.deltaTime * result_speed;
    //         resultingMovVec.y = Mathf.Clamp(resultingMovVec.y, -0.01f, 0.01f);
    //         controller.Move(resultingMovVec);
    //     }
    // }

    // 1
    // public void MoveLaser(float horizontalInput, float yMoveVec, float verticalInput)
    // {
    //     if (controller.enabled)
    //     {
    //         result_speed = speed;
    //         // rotates movement vector to match the camera rotation, so always move forward relative to camera and not with respect to world
    //         moveVec = Quaternion.Euler(cam.eulerAngles) * new Vector3(horizontalInput, 0.0f, verticalInput);
    //         moveVec.y = yMoveVec;
    //         controller.Move(moveVec.normalized * Time.deltaTime * result_speed);
    //     }
    // }

    // 2 set height
    public void MoveLaser(float horizontalInput, float verticalInput)
    {
        if (controller.enabled)
        {
            result_speed = speed;
            // rotates movement vector to match the camera rotation, so always move forward relative to camera and not with respect to world
            moveVec = Quaternion.Euler(cam.eulerAngles) * new Vector3(horizontalInput, 0.0f, verticalInput);
            moveVec.y = 0.0f;
            if (moveVec != Vector3.zero)
            {
                moveVec = moveVec.normalized;
            }
            controller.Move(moveVec * Time.deltaTime * result_speed);
            // Debug.Log(moveVec);
        }
    }


    public void homePosition()
    {
        // transform.position = new Vector3(-1.0f, 5.8f, 0.0f);
        // transform.localPosition = quad.transform.localPosition + new Vector3(0f, 5.8f, 5.95448f);
        transform.localPosition = quad.transform.localPosition + new Vector3(-1.0f, 5.8f, 0.0f);
        // transform.position = transform.parent.Find("Home").transform.position;
        // transform.position = transform.InverseTransformPoint(new);
        // transform.localPosition = new Vector3(0f, 5.8f, 0f);
        // transform.position = Environment.transform.TransformPoint(new Vector3(0f, 5.8f, 0f));
        // transform.position = Environment.transform.position + scalpeloffset;
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
