using UnityEngine;

public class AlwaysFaceUser : MonoBehaviour
{
    private void Update()
    {
        // Update target rotation
        Vector3 targetToCamera = (Camera.main.transform.position - transform.position).normalized;
        transform.forward = -targetToCamera;
    }
}
