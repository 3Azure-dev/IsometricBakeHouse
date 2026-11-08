using UnityEngine;

// Keeps a world-space prompt facing the camera, no matter how the table is rotated.
public class PromptFaceCamera : MonoBehaviour
{
    private Camera cam;

    private void OnEnable()
    {
        cam = Camera.main;
        Face();
    }

    private void LateUpdate()
    {
        Face();
    }

    private void Face()
    {
        if (cam == null) cam = Camera.main;
        if (cam != null) transform.rotation = cam.transform.rotation;
    }
}