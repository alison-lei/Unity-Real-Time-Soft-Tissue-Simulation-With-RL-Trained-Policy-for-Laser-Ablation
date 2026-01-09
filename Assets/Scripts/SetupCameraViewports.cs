using UnityEngine;

public class SetupCameraViewports : MonoBehaviour
{
    public Camera cam1, cam2;

    void Start()
    {
        Debug.Log("Setting camera viewports");
        cam1.rect = new Rect(0f, 0.5f, 1f, 0.5f); // top half
        cam2.rect = new Rect(0f, 0f, 1f, 0.5f);

        cam1.depth = 0;
        cam2.depth = 1;

        // cam1.clearFlags = CameraClearFlags.SolidColor;
        // cam2.clearFlags = CameraClearFlags.Depth;
        // cam3.clearFlags = CameraClearFlags.Depth;
        // cam4.clearFlags = CameraClearFlags.Depth;
    }
}