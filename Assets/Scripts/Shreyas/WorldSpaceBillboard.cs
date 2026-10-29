using UnityEngine;

public class WorldSpaceBillboard : MonoBehaviour
{
    [SerializeField] private Camera cam;
    private Camera targetCam;

    private void OnEnable() { Refresh(); Apply(); }
    private void LateUpdate() { Apply(); }

    private void Apply()
    {
        Refresh();
        if (targetCam != null)
            transform.rotation = targetCam.transform.rotation;
    }

    private void Refresh()
    {
        if (targetCam == null)
            targetCam = cam != null ? cam : Camera.main;
    }
}