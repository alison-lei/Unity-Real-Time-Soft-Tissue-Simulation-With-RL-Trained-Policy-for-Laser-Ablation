using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] float sensitivity;
    [SerializeField] float camSpeed;

    void FixedUpdate()
    {

        //camera rotation
        float rotateHorizontal = Input.GetAxis("Mouse X");
        float rotateVertical = Input.GetAxis("Mouse Y");
        transform.Rotate(Vector3.right * rotateVertical * sensitivity * Time.deltaTime, Space.World); //use transform.Rotate(-transform.up * rotateHorizontal * sensitivity) instead if you dont want the camera to rotate around the player
        transform.Rotate(Vector3.up * rotateHorizontal * sensitivity * Time.deltaTime, Space.World); // again, use transform.Rotate(transform.right * rotateVertical * sensitivity) if you don't want the camera to rotate around the player

        //camera translation
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow))
        {
            float inputVertical = Input.GetAxis("Vertical");
            transform.Translate(Vector3.forward * inputVertical * camSpeed * Time.deltaTime, Space.Self); //use transform.Rotate(-transform.up * rotateHorizontal * sensitivity) instead if you dont want the camera to rotate around the player
        }
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))
        {
            float inputHorizontal = Input.GetAxis("Horizontal");
            transform.Translate(Vector3.right * inputHorizontal * camSpeed * Time.deltaTime, Space.Self); // again, use transform.Rotate(transform.right * rotateVertical * sensitivity) if you don't want the camera to rotate around the player
        }

        //camera zoom
        float scrollMouse = Input.GetAxis("Mouse ScrollWheel");
        transform.Translate(Vector3.up * scrollMouse * camSpeed * Time.deltaTime, Space.Self);

    }
}
