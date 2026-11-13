using UnityEngine;
using UnityEngine.EventSystems;

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

        // UI Slider/Button mela illama empty 3D screen-la Left-click drag panna camera suthum!
        bool isOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        if (!isOverUI && (Input.GetMouseButton(0) || Input.GetMouseButton(1)))
        {
            currentX += Input.GetAxis("Mouse X") * sensitivity;
            currentY -= Input.GetAxis("Mouse Y") * sensitivity;
            currentY = Mathf.Clamp(currentY, 10f, 75f);
        }

        // Keyboard Left/Right Arrow or Q/E keys-layum camera suthalam!
        if (Input.GetKey(KeyCode.Q)) currentX -= 60f * Time.deltaTime;
        if (Input.GetKey(KeyCode.E)) currentX += 60f * Time.deltaTime;

        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        transform.position = target.position - (rotation * Vector3.forward * distance);
        transform.LookAt(target.position);
    }
}