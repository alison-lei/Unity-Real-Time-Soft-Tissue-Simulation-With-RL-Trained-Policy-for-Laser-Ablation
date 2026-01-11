using UnityEngine;

public class FollowPlayer : MonoBehaviour
{

    public Transform scalpel;
    public CharacterController scalpelController;
    private Vector3 camOffset;
    private Vector3 zoomVec, rotVec;
    // Start is called before the first frame update
    void Start()
    {
        camOffset = new Vector3(-1.0f, 40.0f, -10f);
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


        if (scalpelController)
        {
            transform.position = scalpel.position + camOffset + rotVec;
        }
        else
        {
            zoomVec = new Vector3();
        }

    }
}
