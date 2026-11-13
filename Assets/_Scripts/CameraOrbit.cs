using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    public Transform target;
    public float distance = 12f;
    public float sensitivity = 3f;
    private float currentX = 0f;
    private float currentY = 30f;

    void LateUpdate()
    {
        if (target == null) return;

        if (Input.GetMouseButton(1))
        {
            currentX += Input.GetAxis("Mouse X") * sensitivity;
            currentY -= Input.GetAxis("Mouse Y") * sensitivity;
            currentY = Mathf.Clamp(currentY, 10f, 75f);
        }

        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        transform.position = target.position - (rotation * Vector3.forward * distance);
        transform.LookAt(target.position);
    }
}