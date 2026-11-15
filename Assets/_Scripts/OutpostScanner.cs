using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using TMPro;

public class OutpostScanner : MonoBehaviour
{
    public Camera mainCam;
    public TMP_Text scanCardText;

    void Start()
    {
        if (mainCam == null) mainCam = Camera.main;
        if (scanCardText != null)
            scanCardText.text = "<b>[3D SCANNER]</b> Click the Rover, Hangar, or Shield Dome to inspect NASA tech!";
    }

    void Update()
    {
        var mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            Ray ray = mainCam.ScreenPointToRay(mouse.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                string obj = hit.collider.gameObject.name.ToLower();
                if (obj.Contains("rover"))
                    ShowFact("PERSEVERANCE ROVER", "Nuclear MMRTG-powered rover with SuperCam laser & MOXIE oxygen generator at Jezero Crater.");
                else if (obj.Contains("dish") || obj.Contains("antenna"))
                    ShowFact("DEEP SPACE NETWORK ANTENNA", "High-gain X-band & Ka-band parabolic dish transmitting Mars telemetry directly to Earth DSN stations.");
                else if (obj.Contains("shield") || obj.Contains("dome"))
                    ShowFact("MAGNETIC RADIATION DOME", "Simulates an active superconducting magnetosphere to deflect Galactic Cosmic Rays & Solar Flares.");
                else
                    ShowFact("ARTEMIS MARS HABITAT", "Pressurized regolith-shielded crew module recycling 98% of water & oxygen via NASA ECLSS.");
            }
        }
    }

    void ShowFact(string title, string detail)
    {
        if (scanCardText != null)
            scanCardText.text = $"<b>[SCAN: {title}]</b>\n{detail}";
    }
}