using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CameraOrbit : MonoBehaviour
{
    public Transform target;
    public float distance = 15f;
    public float speed = 0.4f;
    private float x = 180f, y = 25f;

    void LateUpdate()
    {
        var mouse = Mouse.current;
        if (mouse != null)
        {
            GameObject selected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            bool interactingUI = selected != null && (selected.GetComponent<Slider>() != null || selected.GetComponent<Button>() != null);

            if ((mouse.leftButton.isPressed && !interactingUI) || mouse.rightButton.isPressed)
            {
                Vector2 delta = mouse.delta.ReadValue();
                x += delta.x * speed;
                y = Mathf.Clamp(y - delta.y * speed, 10f, 80f);
            }

            float scroll = mouse.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) > 0.01f)
                distance = Mathf.Clamp(distance - scroll * 0.01f, 6f, 35f);
        }

        Vector3 center = target != null ? target.position : Vector3.zero;
        Quaternion rot = Quaternion.Euler(y, x, 0);
        transform.position = center + rot * new Vector3(0, 0, -distance);
        transform.LookAt(center + Vector3.up * 1.5f);
    }
}