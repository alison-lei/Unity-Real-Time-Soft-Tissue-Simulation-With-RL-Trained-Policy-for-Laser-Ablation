using UnityEngine;

public class FollowPlayer : MonoBehaviour
{

    public Transform scalpel, stick;
    public CharacterController scalpelController, stickController;
    public Vector3 prevStickPosition, prevScalpelPosition;
    //private bool prevStickState, prevScalpelState;
    private Vector3 camOffset;
    private Vector3 zoomVec, rotVec;
    // Start is called before the first frame update
    void Start()
    {
        //stickController = GameObject.FindGameObjectWithTag("Stick").GetComponent<CharacterController>();
        //scalpelController = GameObject.FindGameObjectWithTag("Scalpel").GetComponent<CharacterController>();
        prevStickPosition = stick.position;
        prevScalpelPosition = scalpel.position;
        camOffset = new Vector3(-1.0f, 40.0f, -10f);
        // prevStickState = false;
        // prevScalpelState = false;
        zoomVec = new Vector3();
        rotVec = new Vector3();
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            zoomVec += Vector3.forward;
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            zoomVec += Vector3.back;
        }
        rotVec = transform.rotation * zoomVec;


        if (stickController.enabled == true)
        {
            transform.position = stick.position + camOffset + rotVec; 
        }
        else if (scalpelController.enabled == true)
        {
            transform.position = scalpel.position + camOffset + rotVec;
        }
        else if (scalpelController.enabled == false && stickController.enabled == false)
        {
            zoomVec = new Vector3();
        }

    }
}
